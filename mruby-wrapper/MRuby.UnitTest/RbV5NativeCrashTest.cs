namespace MRuby.UnitTest;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Library;
using Library.Language;

// v5 NATIVE-CRASH REPRODUCTION (tight vehicle) - the faithful recreation my prior
// experiments lacked. Per MRuby.UnitTest/XunitAssemblyInfo.cs, the original macOS CI
// hard-crash is the reverse-P/Invoke GC-suspension window: CoreCLR suspends a managed
// thread for GC via a POSIX signal while that thread is parked INSIDE a native mruby
// callback (Ruby.Open/Close, a DefineMethod thunk, or mrb_close's CDATA/dfree teardown),
// and the signal lands at a non-interruptible point -> PROCAbort.
//
// Why v1-v4 never reproduced it (all clean): they lacked the THREE ingredients this
// vehicle supplies together:
//   1. REAL mruby native callback parking (not a pure C# loop) - threads sit inside
//      mrb_close teardown and inside DefineMethod-dispatched C# callbacks.
//   2. CROSS-THREAD LIFECYCLE SKEW - distinct mrb_state's in DIFFERENT phases at once
//      (one opening, one calling a callback, one closing), so one thread's GC can suspend
//      another parked in native code. (v1/v2 had all threads in the SAME tight loop.)
//   3. GC FREQUENCY - reproduced under DOTNET_GCStress=0x4 (GC at every transition),
//      which the CI v5 arm sets. A loose forced-GC thread (v4) was far weaker.
//
// GATING: every test here is a no-op unless MRUBY_V5=1, AND real parallelism only exists
// when the assembly is built with MRUBY_EXPERIMENT_PARALLEL (see XunitAssemblyInfo.cs).
// Default build + unset env => these tests do nothing => zero normal-suite impact.
//
// Each Worker* class is a SEPARATE xUnit collection (distinct class = distinct collection),
// so under a parallel build they run CONCURRENTLY, producing the cross-thread skew. Their
// shared churn body lives in V5Churn.
internal static class V5Churn
{
    internal static bool Enabled => Environment.GetEnvironmentVariable("MRUBY_V5") == "1";

    // A background thread hammering compacting gen2 GCs as fast as possible. This is the
    // release-runtime-compatible amplifier (DOTNET_GCStress=0x4 is dead code on the release
    // CoreCLR that setup-dotnet installs - it needs HAVE_GCCOVER/_DEBUG; only 0x1/0x2/0x3
    // work on release). A compacting blocking gen2 forces object relocation + a full STW
    // suspension, maximizing the chance of suspending a thread parked in a native mruby
    // callback. Gated on MRUBY_V5_HAMMER so it can be toggled per CI arm.
    internal static bool HammerEnabled => Environment.GetEnvironmentVariable("MRUBY_V5_HAMMER") == "1";

    // One worker = a tight Open -> define real callback -> call it (reverse-P/Invoke) ->
    // make a data object (mrb_close will run CDATA teardown) -> Close loop. Spawns a few
    // raw threads so even within one collection there is cross-thread lifecycle skew; the
    // separate xUnit collections multiply it further under a parallel build.
    internal static void Run(string tag, int threads, int iterations)
    {
        if (!Enabled)
        {
            return; // skipped unless the v5 experiment is selected
        }

        // ENGAGEMENT SELF-CHECK: record GC counts before/after so the harness can PROVE the
        // GC amplifier actually fired. v5 round 1 was a false negative - DOTNET_GCStress=0x4
        // was silently ignored on the release runtime and we nearly reported "clean". Never
        // again: if the gen2 delta is ~0, the amplifier did not engage and any "no crash"
        // result is meaningless.
        var gen0Before = GC.CollectionCount(0);
        var gen2Before = GC.CollectionCount(2);

        using var hammerCts = new CancellationTokenSource();
        Thread? hammer = null;
        if (HammerEnabled)
        {
            hammer = new Thread(() =>
            {
                while (!hammerCts.IsCancellationRequested)
                {
                    GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                    // No sleep: a blocking compacting gen2 on the small test heap is
                    // microseconds, so this yields hundreds-to-thousands of STW suspensions
                    // per second WITHOUT DOTNET_GCStress (which instrumented every process
                    // allocation and made the run non-completing). This is the bounded,
                    // targeted amplifier; the gen2Delta>50 self-check confirms it engaged.
                    Thread.Yield();
                }
            }) { IsBackground = true, Name = $"v5-hammer-{tag}" };
            hammer.Start();
        }

        var errors = new ConcurrentQueue<Exception>();
        var workers = Enumerable.Range(0, threads)
            .Select(t => new Thread(() =>
            {
                try
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        var state = Ruby.Open();
                        try
                        {
                            var cls = state.DefineClass($"V5_{tag}_{t}_{i}", null);

                            // Real C# callback dispatched FROM native mruby (reverse-P/Invoke).
                            cls.DefineMethod("cb", (_, self, _) => self, RbHelper.MRB_ARGS_NONE(), out _);

                            // A data object so mrb_close runs CDATA/dfree teardown (the
                            // native work the assembly comment blames for the widened
                            // mruby-4.0 suspension window).
                            cls.DefineMethod("initialize", (_, self, _) => self, RbHelper.MRB_ARGS_NONE(), out _);
                            var obj = cls.NewObjectWithCSharpDataObject($"V5Data_{tag}_{t}_{i}", new object());

                            // Park inside native dispatch repeatedly: each call is a
                            // native->managed transition where a GC suspension signal can land.
                            using (var compiler = state.NewCompiler())
                            {
                                compiler.LoadString($"o = {cls.GetClassName()}.new; 8.times {{ o.cb }}");
                            }

                            GC.KeepAlive(obj);
                        }
                        finally
                        {
                            // mrb_close teardown - the other half of the cross-thread skew.
                            Ruby.Close(state);
                        }
                    }
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }
            }) { IsBackground = true, Name = $"v5-{tag}-{t}" })
            .ToList();

        workers.ForEach(w => w.Start());
        foreach (var w in workers)
        {
            Assert.True(w.Join(TimeSpan.FromSeconds(90)), $"v5 worker {tag} did not finish in time.");
        }

        hammerCts.Cancel();
        hammer?.Join(TimeSpan.FromSeconds(10));

        var gen0Delta = GC.CollectionCount(0) - gen0Before;
        var gen2Delta = GC.CollectionCount(2) - gen2Before;
        Console.WriteLine($"[v5-engagement] tag={tag} gen0Delta={gen0Delta} gen2Delta={gen2Delta} hammer={HammerEnabled}");

        // Proof-of-engagement gate: when the hammer is on, gen2 GCs MUST have fired in bulk.
        // If they didn't, the amplifier was a no-op and a "no crash" outcome is a false
        // negative - fail loudly instead of reporting a misleading green.
        if (HammerEnabled)
        {
            Assert.True(
                gen2Delta > 50,
                $"GC amplifier did not engage (gen2Delta={gen2Delta}); 'no crash' would be a false negative.");
        }

        Assert.Empty(errors);
    }
}

// Four distinct collections so a parallel build runs them concurrently (cross-thread
// lifecycle skew across independent mrb_state's). Names are intentionally varied.
public class RbV5WorkerAlphaTest
{
    [Fact]
    public void Churn() => V5Churn.Run("alpha", threads: 3, iterations: 120);
}

public class RbV5WorkerBravoTest
{
    [Fact]
    public void Churn() => V5Churn.Run("bravo", threads: 3, iterations: 120);
}

public class RbV5WorkerCharlieTest
{
    [Fact]
    public void Churn() => V5Churn.Run("charlie", threads: 3, iterations: 120);
}

public class RbV5WorkerDeltaTest
{
    [Fact]
    public void Churn() => V5Churn.Run("delta", threads: 3, iterations: 120);
}
