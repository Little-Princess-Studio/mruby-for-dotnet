// Fully serialize the entire xUnit test assembly (run one test collection at a time).
//
// NOTE ON LOCATION: this assembly-level attribute deliberately lives at the test project
// root, NOT under Properties/ - mruby-wrapper/.gitignore blanket-ignores "Properties/"
// (it only ever held the local-only launchSettings.json), so a file there would be
// silently untracked and the fix would never ship. An assembly attribute compiles
// identically regardless of which file it sits in.
//
// WHY THIS EXISTS (macOS CI test-host crash, native abort):
// xUnit v2 runs distinct test classes as separate collections IN PARALLEL by default
// (one worker thread per logical core). Every test here drives native mruby through
// reverse-P/Invoke callbacks (Ruby.Open/Close, DefineMethod thunks, data-object dfree
// during mrb_close), and some exercise deeper native control flow (mruby exception
// longjmp in RbExceptionTest, fiber swapcontext in RbProcTest, the parser in
// RbCompilerTest). The crash is a NATIVE test-host abort (caught by `dotnet test
// --blame-crash` as "Test host process crashed", surfacing as exit 1 - NOT a managed
// xUnit assertion failure).
//
// LEADING HYPOTHESIS (consistent with the data; NOT yet crash-dump-confirmed):
// on macOS CoreCLR suspends managed threads for a GC using POSIX signals
// (PAL_InjectActivation -> pthread_kill). When a GC on one thread tries to suspend
// another thread that is parked INSIDE a native mruby callback / mrb_close teardown,
// the activation signal can land at a point the runtime cannot safely resume, and the
// host aborts. mruby 4.0 shifted GC timing/teardown work vs 3.3, changing crash
// probability. Confirming the exact mechanism requires analysing the crash-dump thread
// stacks (the GC/suspender thread + the victim thread's native frames); that has not
// been done here, so treat the mechanism as the best-supported explanation, not proof.
//
// THE FIX (empirically measured, this repo): the driver is xUnit test-COLLECTION
// PARALLELISM, not GC intensity. A 2x2 control on macos-14 (full real suite, Server GC
// pressure, --blame-crash, 20 repeats/arm, crashes counted by the host-crash signature):
//
//     arm                         host crashes / 20
//     serial   + coverlet off            0 / 20   (shipped config: clean)
//     serial   + coverlet on             2 / 20
//     parallel + coverlet on             6 / 20
//     parallel + coverlet off            7 / 20
//
// serial(0/20) vs parallel(7/20) is a robust signal (Fisher two-sided p~=0.008);
// coverage collection (coverlet) is a minor secondary pressure, NOT the driver
// (parallel crashes ~equally with or without it). So DisableTestParallelization=true is
// a LOAD-BEARING mitigation that substantially reduces (does not provably eliminate -
// 0/20 is "0 observed", not "0% risk") the crash. It routes execution through
// TestAssemblyRunner's sequential foreach, bypassing the parallel semaphore/SyncContext
// (confirmed against xunit v2-2.9.x source); MaxParallelThreads=1 is belt-and-suspenders.
//
// CAVEAT ON A PRIOR NOTE: an earlier version of this comment cited DOTNET_GCStress=0x4
// as evidence ("PARALLEL crashed on the first test"). That citation is technically
// invalid on the RELEASE CoreCLR that setup-dotnet installs: GCStress modes 0x4/0x8 are
// compiled out of retail builds (they need _DEBUG / HAVE_GCCOVER; see dotnet/coreclr
// #25445). Only 0x1/0x2/0x3 function on release. The empirical conclusion above
// (serialization reduces the parallel-suite crash) stands on the 2x2 data, independent
// of GCStress. Related (adjacent, not exact) runtime issues: dotnet/runtime#44498
// (macOS activation race, fixed ~.NET 6), #102887 (.NET 9 libdispatch case).
//
// COST: the suite is tiny and finishes in well under a second; losing parallelism is
// negligible and far cheaper than a flaky native crash that aborts the whole run.
[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]
