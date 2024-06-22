namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbState
    {
        public IntPtr MrbState { get; set; }
    
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_class(
            IntPtr mrb, 
            [MarshalAs(UnmanagedType.LPStr)]
            string name, IntPtr @class);
    
        public RbClass DefineClass(string name, RbClass? @class)
        {
            var classPtr = mrb_define_class(this.MrbState, name, @class?.NativeHandler ?? IntPtr.Zero);
            return new RbClass()
            {
                NativeHandler = classPtr,
                RbState = this,
            };
        }
    }
}
