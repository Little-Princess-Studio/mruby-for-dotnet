namespace MRuby.UnitTest;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Library;
using Library.Language;

// v4 ROOT-CAUSE EXPERIMENT: does the original macOS CI *native hard crash* (SIGSEGV/
// SIGABRT) reproduce, and WHICH lock prevents it?
//
// Prior experiments established:
//   v1/v2 (mruby-free): generic reverse-P/Invoke GC window does NOT crash (falsified).
//   H1 (real mruby): the managed map dictionaries are genuinely thread-unsafe, but
//      racing them ONLY produced clean managed exceptions (exit 1), NOT native crashes -
//      because the racing tests never called back into native mruby nor let GC collect a
//      dropped delegate during the race.
//
// This class isolates the two candidate NATIVE-crash mechanisms, per Oracle's matrix, so
// a crash is ATTRIBUTABLE (an all-locks-off crash is NOT):
//
//   ARM A (vmlifecycle): disable ONLY VmLifecycleLock (map locks stay ON). Concurrent
//       mrb_open/mrb_close races mruby's native global init/teardown. Tests whether the
//       NATIVE lifecycle race alone is sufficient to hard-crash.
//
//   ARM B (statemapper): keep VmLifecycleLock ON (NO concurrent open/close). Pre-open one
//       state per thread serially, then concurrently define ephemeral callback methods
//       whose ONLY strong root is StateMapper, force a compacting GC, and call them. If
//       StateMapper corruption drops a keeper entry, the delegate is collected and mruby
//       calls a freed function pointer -> native crash. Tests the delegate-drop path.
//
// Each test is gated by MRUBY_V4_ARM so it only runs in its dedicated CI process (env
// flags are process-start state; a hard crash must not poison sibling arms). Default
// (env unset) => every test skips => zero impact on the normal suite.
public class RbV4NativeCrashTest
{
    private sealed class V4Payload
    {
        public long Value { get; set; }
    }

    private static bool ArmEnabled(string arm) =>
        Environment.GetEnvironmentVariable("MRUBY_V4_ARM") == arm;

    // ARM A: native lifecycle race. Run only when MRUBY_V4_ARM=vmlifecycle (which the CI
    // arm pairs with MRUBY_DISABLE_VMLIFECYCLE_LOCK=1, map locks left ON).
    [Fact]
    public void ArmA_ConcurrentOpenCloseRacesNativeLifecycle()
    {
        if (!ArmEnabled("vmlifecycle"))
        {
            return; // skipped unless this arm is selected
        }

        const int threadCount = 6;
        const int iterations = 400;
        var errors = new ConcurrentQueue<Exception>();
        using var barrier = new Barrier(threadCount);

        var threads = Enumerable.Range(0, threadCount)
            .Select(threadId => new Thread(() =>
            {
                try
                {
                    barrier.SignalAndWait();
                    for (var i = 0; i < iterations; i++)
                    {
                        // Concurrent mrb_open/mrb_close: with VmLifecycleLock disabled this
                        // races mruby's native global init/teardown directly.
                        var state = Ruby.Open();
                        var cls = state.DefineClass($"ArmA_{threadId}_{i}", null);
                        cls.DefineMethod("m", (_, self, _) => self, RbHelper.MRB_ARGS_NONE(), out _);
                        using (var compiler = state.NewCompiler())
                        {
                            compiler.LoadString($"{cls.GetClassName()}.new.m");
                        }

                        Ruby.Close(state);
                    }
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }
            })
            {
                IsBackground = true,
                Name = $"armA-{threadId}",
            })
            .ToList();

        threads.ForEach(t => t.Start());
        foreach (var t in threads)
        {
            Assert.True(t.Join(TimeSpan.FromMinutes(3)), "ArmA thread did not finish in time.");
        }

        // If we reach here without a native crash, the lifecycle race produced at most
        // managed exceptions. Surface them so the arm reports cleanly.
        Assert.Empty(errors);
    }

    // ARM B: StateMapper delegate-drop path. VmLifecycleLock stays ON (no concurrent
    // open/close). Run only when MRUBY_V4_ARM=statemapper (CI pairs it with
    // MRUBY_DISABLE_STATEMAPPER_LOCK=1, VmLifecycleLock + DataClassLock left ON).
    [Fact]
    public void ArmB_StateMapperDelegateDropUnderGc()
    {
        if (!ArmEnabled("statemapper"))
        {
            return;
        }

        const int threadCount = 6;
        const int rounds = 300;

        // Pre-open one state per thread SERIALLY (VmLifecycleLock still serializes these).
        // The concurrent section never opens/closes - it only races StateMapper via
        // concurrent DefineMethod (keeper add) while GC runs.
        var pool = Enumerable.Range(0, threadCount).Select(_ => Ruby.Open()).ToArray();
        var errors = new ConcurrentQueue<Exception>();
        using var roundBarrier = new Barrier(threadCount);

        try
        {
            var threads = Enumerable.Range(0, threadCount)
                .Select(threadId => new Thread(() =>
                {
                    var state = pool[threadId];
                    for (var r = 0; r < rounds; r++)
                    {
                        try
                        {
                            var cls = state.DefineClass($"ArmB_{threadId}_{r}", null);

                            // Define a callback whose ONLY strong root is StateMapper: the
                            // helper returns the method name and discards the delegate
                            // (out _ inside), capturing a unique object so the compiler
                            // cannot cache a static delegate. If a concurrent StateMapper
                            // mutation drops this state's keeper, the delegate becomes
                            // unreachable.
                            var methodName = DefineEphemeralMethod(cls, r);

                            // All threads define together, THEN collect, THEN call - the
                            // window where a dropped+collected delegate is invoked natively.
                            roundBarrier.SignalAndWait();
                            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                            GC.WaitForPendingFinalizers();
                            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

                            using (var compiler = state.NewCompiler())
                            {
                                // Calls the ephemeral method -> reverse-P/Invoke into the
                                // (possibly-collected) delegate.
                                compiler.LoadString($"{cls.GetClassName()}.new.{methodName}");
                            }
                        }
                        catch (Exception e)
                        {
                            errors.Enqueue(e);
                        }
                    }
                })
                {
                    IsBackground = true,
                    Name = $"armB-{threadId}",
                })
                .ToList();

            threads.ForEach(t => t.Start());
            foreach (var t in threads)
            {
                Assert.True(t.Join(TimeSpan.FromMinutes(3)), "ArmB thread did not finish in time.");
            }
        }
        finally
        {
            foreach (var state in pool)
            {
                Ruby.Close(state);
            }
        }

        Assert.Empty(errors);
    }

    // Defines a callback method and returns ONLY its name. The NativeMethodFunc is
    // discarded (out _) so StateMapper's keeper is its sole strong root. [NoInlining] +
    // the captured unique array prevent the JIT from caching/hoisting a static delegate
    // that would keep it alive past this frame.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string DefineEphemeralMethod(RbClass cls, int round)
    {
        var captured = new byte[] { (byte)(round & 0xFF) };
        var name = $"ephem{round}";
        cls.DefineMethod(name, (_, self, _) =>
        {
            _ = captured[0];
            return self;
        }, RbHelper.MRB_ARGS_NONE(), out _);
        return name;
    }
}
