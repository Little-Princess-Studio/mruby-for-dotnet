namespace MRubyWrapper.Library
{
    using System;
    using System.Runtime.InteropServices;
    using Language;

    public static partial class Ruby
    {
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_open();

        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_close(IntPtr mrb); 

        public static RbState Open()
        {
            var ptr = mrb_open();
            var state = new RbState()
            {
                MrbState = ptr,
            };
            return state;
        }

        public static void Close(RbState state)
        {
            mrb_close(state.MrbState);
        }
    }
}
