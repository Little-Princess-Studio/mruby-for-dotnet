namespace MRuby.Library
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Language;

    public static class Ruby
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
            lock (VmLifecycleLock)
            {
                if (state.NativeHandler != IntPtr.Zero)
                {
                    RbHelper.DrainStateDataObjects(state);
                    mrb_close(state.NativeHandler);
                }
            }

            RbNativeObjectLiveKeeper.ReleaseKeeper(state);
        }
    }
}
