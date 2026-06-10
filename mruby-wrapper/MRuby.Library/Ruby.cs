namespace MRuby.Library
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Language;

    public static partial class Ruby
    {
        internal const string MrubyLib = "libmruby_x64";

        // Serializes mruby VM creation/teardown across managed threads.
        //
        // An mrb_state* is single-threaded by mruby's design (no GIL). Independent
        // states isolate VM data, but mrb_open()/mrb_close() still touch
        // process-global initialization paths, and mrb_close() drives a final GC
        // sweep that calls managed data-object free callbacks back across the native
        // boundary. Allowing many threads to open/close VMs simultaneously (e.g. xUnit
        // runs test classes in parallel) races those global/teardown paths and can hard
        // -crash the host process. Lifecycle is therefore serialized; concurrent *use*
        // of two already-open, independent states on their own threads is still allowed.
        private static readonly object VmLifecycleLock = new object();

        [DllImport(MrubyLib, CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_open();

        [DllImport(MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrb_close(IntPtr mrb);

        [DllImport(MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_open_failure_p(IntPtr mrb);

        public static RbState Open()
        {
            IntPtr ptr;
            lock (VmLifecycleLock)
            {
                ptr = mrb_open();
                ThrowIfOpenFailed(ptr);
            }

            var state = new RbState()
            {
                NativeHandler = ptr,
            };
            RbHelper.RegisterCanonicalState(state);
            return state;
        }

        [ExcludeFromCodeCoverage]
        private static void ThrowIfOpenFailed(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero || mrb_open_failure_p(ptr))
            {
                if (ptr != IntPtr.Zero)
                {
                    mrb_close(ptr);
                }

                throw new InvalidOperationException("mruby initialization failed (mrb_open returned a failed state).");
            }
        }

        public static void Close(RbState state)
        {
            var releaseCallbacks = new List<(Action<RbState, object?> ReleaseFn, object? Obj)>();
            var exceptions = new List<Exception>();

            try
            {
                // Phase 1: drain data-object registry outside VmLifecycleLock. Managed
                // GCHandles are freed before mrb_close; release callbacks are only collected
                // here and run after native teardown so user code cannot reenter close or
                // throw before the VM handle is closed and zeroed. Native RData is disarmed
                // inside the lifecycle lock immediately before mrb_close so disarm+close are
                // one native lifecycle-critical section.
                var nativeHandler = state.NativeHandler;
                IReadOnlyDictionary<IComparable, RbDataObjectRegistration>? entries = null;
                if (nativeHandler != IntPtr.Zero)
                {
                    var registry = RbNativeObjectLiveKeeper<RbDataObjectKeeper, RbDataObjectRegistration>
                        .GetOrCreateKeeper(state);
                    entries = registry.Drain();

                    foreach (var kv in entries)
                    {
                        var entry = kv.Value;
                        object? obj = null;
                        if (entry.Handle != IntPtr.Zero)
                        {
                            try
                            {
                                obj = RbHelper.GetObjectFromIntPtr(entry.Handle);
                                RbHelper.FreeIntPtrOfCSharpObject(entry.Handle);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }

                        if (entry.ReleaseFn != null)
                        {
                            releaseCallbacks.Add((entry.ReleaseFn, obj));
                        }
                    }
                }

                // Phase 2: serialize native disarm + teardown and zero the handle as the idempotency gate.
                lock (VmLifecycleLock)
                {
                    if (state.NativeHandler != IntPtr.Zero)
                    {
                        if (entries != null)
                        {
                            foreach (var kv in entries)
                            {
                                try
                                {
                                    // Disarm native RData so mrb_close's final sweep skips dfree entirely.
                                    mrb_data_disarm(state.NativeHandler, kv.Value.MrbValue);
                                }
                                catch (Exception ex)
                                {
                                    exceptions.Add(ex);
                                }
                            }
                        }

                        RbHelper.UnregisterCanonicalState(state);
                        mrb_close(state.NativeHandler);
                        state.NativeHandler = IntPtr.Zero;
                    }
                }
            }
            finally
            {
                // Phase 3: release delegate roots and any remaining per-state keepers after close.
                RbNativeObjectLiveKeeper.ReleaseKeeper(state);
            }

            // Phase 4: invoke user release callbacks after native teardown. A callback may throw
            // or call Ruby.Close/Dispose again, but the VM handle is already closed and zeroed.
            foreach (var (releaseFn, obj) in releaseCallbacks)
            {
                try
                {
                    releaseFn(state, obj);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("One or more errors occurred while closing the mruby state.", exceptions);
            }
        }
    }
}
