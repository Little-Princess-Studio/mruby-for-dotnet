using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;

namespace ReversePinvokeGcCrash
{
    /// <summary>
    /// Self-contained reproduction harness for the macOS-specific CoreCLR crash,
    /// with ZERO mruby involvement. Two modes, selected by REPRO_MODE:
    ///
    ///   loop  (v1) - a rooted delegate is called from a native tight loop. Tests
    ///                the PURE reverse-P/Invoke transition window under GC
    ///                suspension. RESULT: 180/180 clean on macOS/Linux/Windows -
    ///                the pure window does NOT crash on .NET 8 by itself.
    ///
    ///   churn (v2) - a fake VM lifecycle (fake_open / fake_register / fake_close).
    ///                Worker threads tight-loop: open a state, register K data
    ///                objects each carrying a managed payload (GCHandle) + a rooted
    ///                dfree callback, then close. fake_close fires every managed
    ///                dfree NESTED DEEP inside native teardown, mimicking mruby's
    ///                mrb_close -> GC sweep -> dfree reverse-callback, under tight
    ///                Open/Close churn. This adds the structural ingredients the
    ///                pure-loop test lacked and that the real storm crash has.
    ///
    /// On Windows the GC suspends threads with SuspendThread (no async signal at
    /// the boundary) -> expected clean. The macOS-vs-Windows contrast is the proof
    /// of where any crash actually lives.
    /// </summary>
    internal static class Program
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativeCallback();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DfreeCallback(IntPtr obj);

        private const string NativeLib = "nativeshim";

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void run_callback_loop(IntPtr cb, long iterations);

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr fake_open();

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void fake_register(IntPtr state, IntPtr dfree, IntPtr obj);

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void fake_close(IntPtr state);

        // ---- CLASS A ISOLATION ----
        // Both callbacks are rooted (static field + GCHandle) for the whole process
        // lifetime, so they can NEVER be GC-collected. Any crash that remains is the
        // CLASS B GC-suspension-at-reverse-P/Invoke-boundary window, NOT a dangling
        // delegate pointer.
        private static readonly NativeCallback s_callback = ManagedCallback;
        private static readonly DfreeCallback s_dfree = ManagedDfree;
        private static GCHandle s_callbackHandle;
        private static GCHandle s_dfreeHandle;

        // Per-worker rolling survivor pool: forces promotion gen0 -> gen1 -> gen2,
        // which is what makes the background GC actually run (and thus suspend).
        [ThreadStatic] private static List<byte[]>? t_survivors;
        [ThreadStatic] private static int t_workerId;

        private static long[] s_counts = Array.Empty<long>();
        private static int s_garbageBytes = 16 * 1024;
        private static int s_survCap = 2048;
        private static int s_objectsPerState = 32;
        private static volatile bool s_deadlineHit;

        private static int Main()
        {
            string mode   = (Environment.GetEnvironmentVariable("REPRO_MODE") ?? "loop").Trim().ToLowerInvariant();
            int threads   = EnvInt("REPRO_THREADS", 4);
            int seconds   = EnvInt("REPRO_SECONDS", 20);
            long iters    = EnvLong("REPRO_ITERS", 100_000_000L);
            int garbageKb = EnvInt("REPRO_GARBAGE_KB", 16);
            bool forceGc  = EnvInt("REPRO_FORCE_GC", 1) != 0;
            int forceGcUs = EnvInt("REPRO_FORCE_GC_US", 20);
            int survCap   = EnvInt("REPRO_SURVIVORS", 2048);
            int objsState = EnvInt("REPRO_OBJECTS_PER_STATE", 32);

            s_garbageBytes = garbageKb * 1024;
            s_survCap = survCap;
            s_objectsPerState = objsState;
            s_counts = new long[threads];

            Console.WriteLine("==================================================================");
            Console.WriteLine(" reverse-P/Invoke GC-suspension crash repro  (ZERO mruby involved)");
            Console.WriteLine("==================================================================");
            Console.WriteLine($"OS          : {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Arch        : {RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine($"Runtime     : {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"ServerGC    : {GCSettings.IsServerGC}");
            Console.WriteLine($"LatencyMode : {GCSettings.LatencyMode}");
            Console.WriteLine($"mode        : {mode}");
            Console.WriteLine($"config      : threads={threads} seconds={seconds} garbageKb={garbageKb} " +
                              $"forceGc={forceGc} forceGcUs={forceGcUs} survivors={survCap} objsPerState={objsState}");
            Console.WriteLine("------------------------------------------------------------------");

            // Root both delegates (eliminates class A) and grab raw function pointers.
            s_callbackHandle = GCHandle.Alloc(s_callback);
            s_dfreeHandle = GCHandle.Alloc(s_dfree);
            IntPtr cbPtr = Marshal.GetFunctionPointerForDelegate(s_callback);
            IntPtr dfreePtr = Marshal.GetFunctionPointerForDelegate(s_dfree);

            var sw = Stopwatch.StartNew();
            var deadline = TimeSpan.FromSeconds(seconds);

            // Watchdog: after the deadline, work drains quickly and the process exits 0.
            var watchdog = new Thread(() =>
            {
                while (sw.Elapsed < deadline) Thread.Sleep(5);
                s_deadlineHit = true;
            }) { IsBackground = true, Name = "watchdog" };
            watchdog.Start();

            // Monitor: prints gen0/1/2 movement so a false-negative (no background
            // GC -> no suspension -> no chance of crash) is visible, not silent.
            var monitor = new Thread(() =>
            {
                while (!s_deadlineHit)
                {
                    Thread.Sleep(1000);
                    long total = 0;
                    foreach (var c in s_counts) total += c;
                    Console.WriteLine($"[gc] t={sw.Elapsed.TotalSeconds,4:F0}s  " +
                                      $"gen0={GC.CollectionCount(0),5}  gen1={GC.CollectionCount(1),5}  " +
                                      $"gen2={GC.CollectionCount(2),5}  ops={total:N0}");
                }
            }) { IsBackground = true, Name = "gc-monitor" };
            monitor.Start();

            // Forced-GC driver: a separate thread doing blocking gen2 collects =>
            // constant thread-suspension of the worker threads while they oscillate
            // across the reverse-P/Invoke boundary. (egonelbre's repro for
            // dotnet/runtime#127320 reached SIGSEGV 5/5 with this driver.)
            if (forceGc)
            {
                var gcDriver = new Thread(() =>
                {
                    while (!s_deadlineHit)
                    {
                        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
                        if (forceGcUs > 0) Thread.Sleep(TimeSpan.FromMicroseconds(forceGcUs));
                        else Thread.Yield();
                    }
                }) { IsBackground = true, Name = "gc-driver" };
                gcDriver.Start();
            }

            var workers = new Thread[threads];
            for (int i = 0; i < threads; i++)
            {
                int id = i;
                workers[i] = new Thread(() =>
                {
                    t_workerId = id;
                    t_survivors = new List<byte[]>(s_survCap + 8);
                    if (mode == "churn")
                        ChurnWorker(dfreePtr);
                    else
                        run_callback_loop(cbPtr, iters);
                }) { IsBackground = false, Name = $"worker-{i}" };
                workers[i].Start();
            }

            foreach (var w in workers) w.Join();
            sw.Stop();

            GC.KeepAlive(s_callback);
            GC.KeepAlive(s_dfree);
            s_callbackHandle.Free();
            s_dfreeHandle.Free();

            long grand = 0;
            foreach (var c in s_counts) grand += c;
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine($"COMPLETED CLEANLY in {sw.Elapsed.TotalSeconds:F1}s | " +
                              $"ops={grand:N0} | gen2={GC.CollectionCount(2)}");
            Console.WriteLine("exit 0 (no crash on this attempt)");
            return 0;
        }

        // ---- Mode: churn ----
        // Tight Open/Close churn. Each iteration: open a fake state, register K data
        // objects (each pins a managed payload via GCHandle and hands native the
        // handle + the rooted dfree), then close -> native fake_close walks the
        // objects and calls the managed dfree for each, NESTED inside native
        // teardown. A GC suspension landing while parked in fake_close hits a
        // managed thread deep in native code -> the real storm's crash shape.
        private static void ChurnWorker(IntPtr dfreePtr)
        {
            int k = s_objectsPerState;
            while (!s_deadlineHit)
            {
                IntPtr state = fake_open();
                for (int j = 0; j < k; j++)
                {
                    // Managed payload that the data object "owns"; pinned by a Normal
                    // GCHandle exactly like the binding's data-object mapping.
                    var payload = new byte[256];
                    payload[0] = (byte)j;
                    GCHandle h = GCHandle.Alloc(payload, GCHandleType.Normal);
                    fake_register(state, dfreePtr, GCHandle.ToIntPtr(h));
                }
                // dfree callbacks fire here, inside native teardown.
                fake_close(state);
                s_counts[t_workerId]++;
            }
        }

        // dfree: runs nested inside native fake_close. Frees the GCHandle and
        // allocates a little (so the close path itself produces GC pressure, like
        // mruby's teardown). Survivors retained to keep promotion to gen2 alive.
        private static void ManagedDfree(IntPtr obj)
        {
            if (obj != IntPtr.Zero)
            {
                GCHandle h = GCHandle.FromIntPtr(obj);
                if (h.IsAllocated) h.Free();
            }

            byte[] bytes = new byte[s_garbageBytes];
            bytes[0] = 7;
            bytes[bytes.Length - 1] = 9;
            List<byte[]>? survivors = t_survivors;
            if (survivors != null)
            {
                survivors.Add(bytes);
                if (survivors.Count > s_survCap)
                    survivors.RemoveRange(0, s_survCap / 2);
            }
        }

        // ---- Mode: loop ----
        private static void ManagedCallback()
        {
            if (s_deadlineHit) return;

            byte[] bytes = new byte[s_garbageBytes];
            bytes[0] = 1;
            bytes[bytes.Length - 1] = 2;

            List<byte[]>? survivors = t_survivors;
            if (survivors != null)
            {
                survivors.Add(bytes);
                if (survivors.Count > s_survCap)
                    survivors.RemoveRange(0, s_survCap / 2);
            }

            s_counts[t_workerId]++;
        }

        private static int EnvInt(string key, int def) =>
            int.TryParse(Environment.GetEnvironmentVariable(key), out var v) ? v : def;

        private static long EnvLong(string key, long def) =>
            long.TryParse(Environment.GetEnvironmentVariable(key), out var v) ? v : def;
    }
}
