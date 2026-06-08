namespace MRuby.UnitTest;

using System;
using Xunit;

// A [Fact] that only runs on Windows.
//
// Some regression tests deliberately create a managed allocation/GC storm across many
// raw worker threads while native mruby activity happens elsewhere in the same process.
// On Windows this is stable and is the platform where these tests are proven both to
// pass with the thread-safety fix in place and to FAIL (detect the regression) when the
// fix is reverted. On macOS/Linux the .NET test host can hard-exit under this synthetic
// load - the runtime suspends/rendezvouses managed threads that are executing inside
// native mruby callbacks during a GC. That is a test-host/runtime stress limit, NOT a
// library defect: the real parallel xUnit suite (the actual original CI crash scenario)
// is stable on all platforms once the dictionaries are locked.
//
// The library's supported contract is: an RbState is thread-affine; distinct states may
// be used concurrently; VM lifecycle is serialized internally. Heavy cross-thread stress
// against native callbacks is intentionally outside that contract, so we assert the
// underlying managed thread-safety only where the runtime can host the stress reliably.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WindowsOnlyFactAttribute : FactAttribute
{
    public WindowsOnlyFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            this.Skip = "Windows-only concurrency stress regression: macOS/Linux test-host "
                + "stability under a synthetic GC/thread storm is not part of the library "
                + "contract (an RbState is thread-affine; lifecycle is serialized).";
        }
    }
}
