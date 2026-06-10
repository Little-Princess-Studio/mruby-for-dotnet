namespace MRuby.UnitTest;

using System;
using Xunit;

// A [Fact] that runs on ALL platforms including macOS, for the stabilized
// Open/Close storm test (TestStaticMappingsAreStableUnderHeavySequentialOpenCloseStorm).
//
// Unlike [WindowsOnlyFact], this attribute does NOT skip on macOS or Linux.
// It is "stabilized" by running the storm body inside chunked NoGCRegion segments
// (implemented in T10), which suppresses the CLR gen0 churn that drives the crash
// probability on macOS.
//
// Env var contract (CI amplifier sets these):
//   MRUBY_STORM_CYCLES  - number of Open/Close cycles (default: 1 for the normal suite;
//                         CI amplifier/canary override explicitly)
//   MRUBY_STORM_NOGC    - "1" enables chunked NoGCRegion (default: 1=on), "0" disables
//     Setting MRUBY_STORM_NOGC=0 + fix ON is the A3 attribution config: proves the
//     registry/disarm fix closes the crash window independent of GC suppression.
//   MRUBY_ASSERT_MIN_GC - optional minimum gen0 delta assertion for diagnostic canaries.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class StabilizedStormFactAttribute : FactAttribute
{
    // Reads MRUBY_STORM_CYCLES env var; returns parsed int or default 1.
    // Normal platform suites should exercise the path without running the heavy synthetic
    // Open/Close storm. CI's separate amplifier job owns the probabilistic 5000-cycle gate.
    public static int GetCycles()
    {
        var raw = Environment.GetEnvironmentVariable("MRUBY_STORM_CYCLES");
        if (raw != null && int.TryParse(raw, out var n) && n > 0)
            return n;
        return 1;
    }

    // Reads MRUBY_STORM_NOGC env var; returns true unless explicitly "0".
    public static bool GetNoGcEnabled()
    {
        var raw = Environment.GetEnvironmentVariable("MRUBY_STORM_NOGC");
        return raw != "0";  // default on; only "0" disables
    }

    public static int? GetMinimumGen0Collections()
    {
        var raw = Environment.GetEnvironmentVariable("MRUBY_ASSERT_MIN_GC");
        if (raw != null && int.TryParse(raw, out var n) && n >= 0)
            return n;

        return null;
    }
}
