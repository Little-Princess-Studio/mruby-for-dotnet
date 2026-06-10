using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;

namespace ReversePinvokeGcCrash
{
    /// <summary>
    /// Self-contained reproduction of the macOS-specific CoreCLR crash, with
    /// ZERO mruby involvement. A rooted CLR delegate is handed to a trivial
    /// native shim that calls it in a tight loop (reverse-P/Invoke). The
    /// callback allocates managed memory that promotes to gen2; a dedicated
    /// driver thread forces blocking gen2 GCs. The CoreCLR GC suspends the
    /// worker threads via POSIX signals (SIGUSR1/SIGRTMIN on Unix). If the
    /// signal lands while a worker is mid-transition at the reverse-P/Invoke
    /// boundary, the process crashes (SIGSEGV / SIGABRT).
    ///
    /// On Windows the GC suspends threads with SuspendThread (a different
    /// mechanism, no async signal at the boundary) -> it should NOT crash.
    /// That cross-platform contrast is the proof the bug is CLR + Unix/macOS
    /// specific, not a property of mruby.
    /// </summary>
    internal static class Program
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativeCallback();

        private const string NativeLib = "nativeshim";

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void run_callback_loop(IntPtr cb, long iterations);

        // ---- CLASS A ISOLATION ----
        // The delegate is rooted (static field + GCHandle) for the whole process
        // lifetime, so it can NEVER be GC-collected. Any crash that remains is
        // therefore the CLASS B GC-suspension-at-reverse-P/Invoke-boundary window,
        // NOT a dangling/collected delegate pointer.
        private static readonly NativeCallback s_callback = ManagedCallback;
        private static GCHandle s_callbackHandle;

        // Per-worker rolling survivor pool: forces promotion gen0 -> gen1 -> gen2,
        // which is what makes the background GC actually run (and thus suspend).
        [ThreadStatic] private static List<byte[]>? t_survivors;
        [ThreadStatic] private static int t_workerId;

        private static long[] s_counts = Array.Empty<long>();
        private static int s_garbageBytes = 16 * 1024;
        private static int s_survCap = 2048;
        private static volatile bool s_deadlineHit;

        private static int Main()
        {
            int threads   = EnvInt("REPRO_THREADS", 4);
            int seconds   = EnvInt("REPRO_SECONDS", 20);
            long iters    = EnvLong("REPRO_ITERS", 100_000_000L);
            int garbageKb = EnvInt("REPRO_GARBAGE_KB", 16);
            bool forceGc  = EnvInt("REPRO_FORCE_GC", 1) != 0;
            int forceGcUs = EnvInt("REPRO_FORCE_GC_US", 20);
            int survCap   = EnvInt("REPRO_SURVIVORS", 2048);

            s_garbageBytes = garbageKb * 1024;
            s_survCap = survCap;
            s_counts = new long[threads];

            Console.WriteLine("==================================================================");
            Console.WriteLine(" reverse-P/Invoke GC-suspension crash repro  (ZERO mruby involved)");
            Console.WriteLine("==================================================================");
            Console.WriteLine($"OS          : {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Arch        : {RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine($"Runtime     : {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"ServerGC    : {GCSettings.IsServerGC}");
            Console.WriteLine($"LatencyMode : {GCSettings.LatencyMode}");
            Console.WriteLine($"config      : threads={threads} seconds={seconds} garbageKb={garbageKb} " +
                              $"forceGc={forceGc} forceGcUs={forceGcUs} survivors={survCap}");
            Console.WriteLine("------------------------------------------------------------------");

            // Root the delegate (eliminates class A) and grab the raw function pointer.
            s_callbackHandle = GCHandle.Alloc(s_callback);
            IntPtr fp = Marshal.GetFunctionPointerForDelegate(s_callback);

            var sw = Stopwatch.StartNew();
            var deadline = TimeSpan.FromSeconds(seconds);

            // Watchdog: after the deadline, callbacks become no-ops so the native
            // loops drain quickly and the process exits 0 (a "clean" attempt).
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
                                      $"gen2={GC.CollectionCount(2),5}  callbacks={total:N0}");
                }
            }) { IsBackground = true, Name = "gc-monitor" };
            monitor.Start();

            // Forced-GC driver: THE key amplifier. A separate thread doing blocking
            // gen2 collects => constant thread-suspension of the worker threads while
            // they oscillate across the reverse-P/Invoke boundary. (egonelbre's repro
            // for dotnet/runtime#127320 reached SIGSEGV 5/5 with this driver.)
            Thread? gcDriver = null;
            if (forceGc)
            {
                gcDriver = new Thread(() =>
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

            // Worker threads: each enters native ONCE, and native then calls the
            // rooted delegate `iters` times -> millions of boundary crossings/thread.
            var workers = new Thread[threads];
            for (int i = 0; i < threads; i++)
            {
                int id = i;
                workers[i] = new Thread(() =>
                {
                    t_workerId = id;
                    t_survivors = new List<byte[]>(s_survCap + 8);
                    run_callback_loop(fp, iters);
                }) { IsBackground = false, Name = $"worker-{i}" };
                workers[i].Start();
            }

            foreach (var w in workers) w.Join();
            sw.Stop();

            GC.KeepAlive(s_callback);
            s_callbackHandle.Free();

            long grand = 0;
            foreach (var c in s_counts) grand += c;
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine($"COMPLETED CLEANLY in {sw.Elapsed.TotalSeconds:F1}s | " +
                              $"callbacks={grand:N0} | gen2={GC.CollectionCount(2)}");
            Console.WriteLine("exit 0 (no crash on this attempt)");
            return 0;
        }

        private static void ManagedCallback()
        {
            // After the deadline: cheap no-op so the native loop drains fast.
            if (s_deadlineHit) return;

            // Short-lived allocation that we actually touch (so it isn't elided),
            // then retained in a rolling window to drive promotion to gen2.
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

            // Single-writer-per-slot: no atomics in the hot path (keeps the
            // boundary-crossing frequency high).
            s_counts[t_workerId]++;
        }

        private static int EnvInt(string key, int def) =>
            int.TryParse(Environment.GetEnvironmentVariable(key), out var v) ? v : def;

        private static long EnvLong(string key, long def) =>
            long.TryParse(Environment.GetEnvironmentVariable(key), out var v) ? v : def;
    }
}
