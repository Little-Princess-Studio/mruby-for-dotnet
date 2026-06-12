namespace MRuby.UnitTest;

using System;
using System.Diagnostics;
using Library;
using Library.Language;

// Callback-invocation microbenchmark (PART A of the trampoline perf investigation).
//
// Measures the per-call cost of the native-trampoline -> managed-dispatcher path that
// every mruby->C# method callback now takes, vs the pre-trampoline direct-delegate
// design. Reports ns/call and gen0 GC delta so before/after optimization runs are
// comparable. Gated behind MRUBY_BENCH=1 so it never runs in the normal suite (it is a
// measurement, not a pass/fail assertion).
//
// Shape: define ONE trivial C# instance-method callback, then run a tight Ruby loop
// `obj.cb` N times via a single LoadString (so the loop itself is in the mruby VM and
// each iteration is one real method dispatch through the trampoline). Warm up first to
// exclude JIT/first-call costs, then time the measured run.
public class RbCallbackBenchmark
{
    private static bool Enabled => Environment.GetEnvironmentVariable("MRUBY_BENCH") == "1";

    private static int Iterations =>
        int.TryParse(Environment.GetEnvironmentVariable("MRUBY_BENCH_ITERS"), out var n) && n > 0
            ? n
            : 2_000_000;

    [Fact]
    public void BenchmarkCallbackInvocation()
    {
        if (!Enabled)
        {
            return; // measurement only; skipped unless MRUBY_BENCH=1
        }

        using var state = Ruby.Open();
        var cls = state.DefineClass("BenchTarget", null);
        cls.DefineMethod("initialize", (_, self, _) => self, RbHelper.MRB_ARGS_NONE(), out _);
        // Trivial no-arg callback: returns self. Isolates the dispatch overhead, not user work.
        cls.DefineMethod("cb", (_, self, _) => self, RbHelper.MRB_ARGS_NONE(), out _);

        int iters = Iterations;

        // Warm up: JIT the dispatcher, populate the canonical-state cache, prime the method
        // cache, so the timed run measures steady-state per-call cost only.
        using (var warm = state.NewCompiler())
        {
            warm.LoadString($"o = {cls.GetClassName()}.new; 100000.times {{ o.cb }}; nil");
        }

        var gen0Before = GC.CollectionCount(0);
        var gen1Before = GC.CollectionCount(1);
        var sw = Stopwatch.StartNew();

        using (var run = state.NewCompiler())
        {
            run.LoadString($"o = {cls.GetClassName()}.new; {iters}.times {{ o.cb }}; nil");
        }

        sw.Stop();
        var gen0 = GC.CollectionCount(0) - gen0Before;
        var gen1 = GC.CollectionCount(1) - gen1Before;

        double nsPerCall = sw.Elapsed.TotalMilliseconds * 1_000_000.0 / iters;
        Console.WriteLine("==================== CALLBACK BENCHMARK ====================");
        Console.WriteLine($"iterations   : {iters:N0}");
        Console.WriteLine($"total time   : {sw.Elapsed.TotalMilliseconds:F1} ms");
        Console.WriteLine($"ns per call  : {nsPerCall:F1}");
        Console.WriteLine($"gen0 delta   : {gen0}");
        Console.WriteLine($"gen1 delta   : {gen1}");
        Console.WriteLine("============================================================");
    }
}
