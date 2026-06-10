namespace MRuby.UnitTest;

using System;
using Xunit;

// EXPERIMENT-aware variant of [WindowsOnlyFact] for the two concurrent map-race
// regression tests (TestConcurrentKeeperMappingIsThreadSafe,
// TestConcurrentDataObjectRegistrationIsThreadSafe).
//
// Default behavior is IDENTICAL to [WindowsOnlyFact]: the test runs on Windows and is
// skipped on macOS/Linux, because the shipped stance is that heavy cross-thread stress
// against native callbacks is outside the library's thread-affinity contract.
//
// When MRUBY_EXPERIMENT_UNSKIP=1, the test ALSO runs on macOS/Linux. This is used by the
// root-cause experiment to test, with REAL mruby, whether reverting the process-global
// map locks (via MRUBY_DISABLE_STATEMAPPER_LOCK / MRUBY_DISABLE_DATACLASS_LOCK) actually
// reproduces the original macOS hard-crash - and, importantly, whether these tests are
// stable on macOS WITH the locks in place (the [WindowsOnlyFact] comment claims they
// hard-exit on macOS even with the fix; that claim is being verified, not assumed).
//
// This attribute changes NO production code and, with the env unset, changes NO test
// behavior on any platform.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ConcurrencyExperimentFactAttribute : FactAttribute
{
    public ConcurrencyExperimentFactAttribute()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        if (Environment.GetEnvironmentVariable("MRUBY_EXPERIMENT_UNSKIP") == "1")
        {
            return;
        }

        this.Skip = "Windows-only concurrency stress regression (set MRUBY_EXPERIMENT_UNSKIP=1 "
            + "to run on macOS/Linux for the root-cause experiment).";
    }
}
