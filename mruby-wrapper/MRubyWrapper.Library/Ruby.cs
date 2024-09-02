namespace MRubyWrapper.Library
{
    using System;
    using System.Runtime.InteropServices;
    using Language;

    public static class Ruby
    {
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_open();

        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_close(IntPtr mrb);

        public static RbState Open()
        {
            var ptr = mrb_open();
            var state = new RbState()
            {
                NativeHandler = ptr,
            };
            return state;
        }

        public static void Close(RbState state)
        {
            if (state.NativeHandler != IntPtr.Zero)
            {
                mrb_close(state.NativeHandler);
            }
        }
    }
}