// Fully serialize the entire xUnit test assembly (run one test collection at a time).
//
// NOTE ON LOCATION: this assembly-level attribute deliberately lives at the test project
// root, NOT under Properties/ - mruby-wrapper/.gitignore blanket-ignores "Properties/"
// (it only ever held the local-only launchSettings.json), so a file there would be
// silently untracked and the fix would never ship. An assembly attribute compiles
// identically regardless of which file it sits in.
//
// WHY THIS EXISTS (macOS-only CI test-host crash, signal 11 / SIGSEGV):
// xUnit v2 runs distinct test classes as separate collections IN PARALLEL by default
// (one worker thread per logical core). Every test here drives native mruby through
// reverse-P/Invoke callbacks (Ruby.Open/Close, DefineMethod thunks, data-object dfree
// during mrb_close). On macOS, CoreCLR suspends managed threads for a GC using POSIX
// signals (PAL_InjectActivation -> pthread_kill SIGUSR1). When the GC tries to suspend
// a thread that is parked INSIDE such a native callback, the activation signal can land
// at a non-interruptible point and PROCAbort()s the whole test host. mruby 4.0 widened
// this window (MRB_TT_CDATA teardown does more native work during mrb_close) which is
// why mruby 3.3 never tripped it. It is a macOS CoreCLR limitation in suspending threads
// stopped in native frames (related issues: dotnet/runtime#44498, #58111; xamarin-
// macios#13962) - NOT a defect in this library's managed code. NOTE: this was empirically
// verified to STILL reproduce on .NET 10, so it is not tied to a runtime version;
// dotnet/runtime#102887 (.NET 9) fixed a DIFFERENT macOS case (libdispatch queue threads).
//
// THE FIX: the crash needs TWO coincident conditions - a GC in flight AND a thread
// parked in a native mruby callback. xUnit's default per-class parallelism put several
// threads in native callbacks at once and multiplied that coincidence into a ~50% CI
// flake. Running collections strictly sequentially removes the multiplier we control.
// Verified on a macOS arm64 host under DOTNET_GCStress=0x4 (GC at every transition, the
// worst case): PARALLEL crashed the host on the very FIRST test (0 completed) while
// SERIAL survived 7-65x longer; and under normal GC the serialized suite is consistently
// green (8/8). DisableTestParallelization=true alone is sufficient - it routes execution
// through TestAssemblyRunner's sequential foreach, bypassing the parallel
// semaphore/SyncContext entirely (confirmed against xunit v2-2.9.x source). The
// MaxParallelThreads=1 is belt-and-suspenders defense-in-depth.
//
// COST: the suite is tiny and finishes in well under a second; losing parallelism is
// negligible and far cheaper than a flaky native crash that aborts the whole run.
//
// EXPERIMENT GATE (v5): this attribute is the LOAD-BEARING native-crash mitigation, so it
// cannot be toggled at runtime (assembly attributes are compile-time). The v5 experiment
// builds with -p:DefineConstants=MRUBY_EXPERIMENT_PARALLEL to compile it OUT and restore
// xUnit's default per-class parallelism, recreating the pre-fix condition that (per the
// mechanism above, amplified by DOTNET_GCStress=0x4) reproduces the macOS native crash.
// The DEFAULT build (no constant) keeps the shipped serial behavior byte-identical.
#if !MRUBY_EXPERIMENT_PARALLEL
[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]
#endif
