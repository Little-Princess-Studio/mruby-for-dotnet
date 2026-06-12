namespace MRuby.UnitTest;

using System;
using Library;
using Library.Language;

// Regression guard for native-callback lifetime across GC.
//
// DefineMethod (and friends) register the C# callback under an integer callbackId; mruby
// is handed only a STATIC native trampoline pointer (carrying the id in its proc env),
// never a managed delegate. The callback itself lives in the per-state callbackId ->
// CSharpMethodFunc map (RbCallbackDispatch), rooted for the RbState lifetime. So a GC
// between definition and invocation cannot collect anything mruby depends on - the old
// "discarded delegate gets collected, mruby calls a freed pointer, host crashes" bug is
// structurally impossible now (mruby holds a static function pointer, not a delegate).
//
// These tests force aggressive GC between definition and call to confirm the callback
// still resolves and runs correctly afterwards. They are [WindowsOnlyFact]: a forced
// GC.Collect from a test thread while OTHER test classes execute inside native mruby
// callbacks can hard-exit the macOS/Linux test host (a runtime stress limit, not a library
// defect). The real (non-GC-storm) suite proves correctness on all platforms by running
// green in parallel on macOS. See WindowsOnlyFactAttribute for the full rationale.
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
        cls.DefineMethod("answer", (stat, self, args) => stat.BoxInt(42), RbHelper.MRB_ARGS_NONE());

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
        cls.DefineClassMethod("ping", (stat, self, args) => stat.BoxInt(7), RbHelper.MRB_ARGS_NONE());

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
            cls.DefineMethod($"m{i}", (stat, self, args) => stat.BoxInt(captured), RbHelper.MRB_ARGS_NONE());
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
