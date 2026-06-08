namespace MRuby.Library
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Language;

    public static class Ruby
    {
        internal const string MrubyLib = "libmruby_x64";

        [DllImport(MrubyLib, CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_open();

        [DllImport(MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrb_close(IntPtr mrb);

        [DllImport(MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_open_failure_p(IntPtr mrb);

        public static RbState Open()
        {
            var ptr = mrb_open();
            ThrowIfOpenFailed(ptr);

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
            if (state.NativeHandler != IntPtr.Zero)
            {
                mrb_close(state.NativeHandler);
            }

            RbNativeObjectLiveKeeper.ReleaseKeeper(state);
        }
    }
}
