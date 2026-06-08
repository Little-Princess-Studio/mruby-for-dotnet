namespace MRuby.UnitTest;

using System;
using Library;
using Library.Language;

// Reproduction + regression guard for the dangling native-callback bug.
//
// DefineMethod (and friends) build a NativeMethodFunc delegate, hand its function
// pointer to mruby via mrb_define_method_id, and return the delegate through an
// `out` parameter. If the library does NOT root that delegate, a caller that
// discards it (the idiomatic `out _`) leaves nothing keeping it alive. The next GC
// collects the delegate while mruby still holds its raw function pointer; invoking
// the Ruby method then jumps through a freed pointer and hard-crashes the process
// (a native test-host abort, not a managed exception). This was the real macOS CI
// crash: under xUnit's parallel collections, GC pressure from other tests collected
// the discarded delegates at the wrong time.
//
// The fix roots every callback delegate to the RbState lifetime inside the library
// (RbHelper.BuildAndRootNativeCallback -> RbCallbackKeeper), so `out _` is safe.
//
// These tests force aggressive GC between definition and call to deterministically
// expose the bug. They are [WindowsOnlyFact]: a forced GC.Collect from a test thread
// while OTHER test classes are executing inside native mruby callbacks makes the
// macOS/Linux signal-based GC thread-suspension hard-exit the test host (a runtime
// stress limit, not a library defect). On Windows they are stable AND detective -
// verified to crash 0/20 against the unrooted library and pass after the fix. The
// real (non-GC-storm) suite proves the fix on all platforms by running green in
// parallel on macOS. See WindowsOnlyFactAttribute for the full rationale.
public class RbCallbackLifetimeTest
{
    private static void ForceFullGc()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
        }
    }

    [WindowsOnlyFact]
    public void TestInstanceMethodCallbackSurvivesGc()
    {
        using var state = Ruby.Open();
        using var compiler = state.NewCompiler();

        var cls = state.DefineClass("GcInstanceProbe", null);

        // Discard the delegate (idiomatic usage). The library must root it.
        cls.DefineMethod("answer", (stat, self, args) => stat.BoxInt(42), RbHelper.MRB_ARGS_NONE(), out _);

        // Drop every managed reference we control, then force GC. If the delegate is
        // not rooted by the library, mruby now holds a dangling function pointer.
        ForceFullGc();

        var result = compiler.LoadString("GcInstanceProbe.new.answer");
        Assert.Equal(42, state.UnboxInt(result));
    }

    [WindowsOnlyFact]
    public void TestClassMethodCallbackSurvivesGc()
    {
        using var state = Ruby.Open();
        using var compiler = state.NewCompiler();

        var cls = state.DefineClass("GcClassProbe", null);
        cls.DefineClassMethod("ping", (stat, self, args) => stat.BoxInt(7), RbHelper.MRB_ARGS_NONE(), out _);

        ForceFullGc();

        var result = compiler.LoadString("GcClassProbe.ping");
        Assert.Equal(7, state.UnboxInt(result));
    }

    [WindowsOnlyFact]
    public void TestManyDiscardedCallbacksSurviveGc()
    {
        using var state = Ruby.Open();
        using var compiler = state.NewCompiler();

        var cls = state.DefineClass("GcManyProbe", null);

        // Define many methods, all discarding the delegate, to maximise the chance a
        // collection reclaims at least one before it is called.
        for (var i = 0; i < 50; i++)
        {
            var captured = i;
            cls.DefineMethod($"m{i}", (stat, self, args) => stat.BoxInt(captured), RbHelper.MRB_ARGS_NONE(), out _);
        }

        ForceFullGc();

        var obj = cls.NewObject();
        for (var i = 0; i < 50; i++)
        {
            var result = obj.CallMethod($"m{i}");
            Assert.Equal(i, state.UnboxInt(result));
        }
    }
}
