namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint NativeMethodSignature(IntPtr mrb, uint self);

    public static class RbHelper
    {
        public static uint MRB_ARGS_REQ(uint n) => (n & 0x1fU) << 18;
        public static uint MRB_ARGS_OPT(uint n) => (n & 0x1fU) << 13;
        public static uint MRB_ARGS_ARG(uint n1, uint n2) => MRB_ARGS_REQ(n1) | MRB_ARGS_OPT(n2);
        public static uint MRB_ARGS_REST() => 1U << 12;
        public static uint MRB_ARGS_POST(uint n) => (n & 0x1fU) << 7;
        public static uint MRB_ARGS_KEY(uint n1, uint n2) => ((n1 & 0x1fU) << 2) | (n2 != 0 ? 1U : 0);
        public static uint MRB_ARGS_BLOCK() => 1U;
        public static uint MRB_ARGS_ANY() => MRB_ARGS_REST();
        public static uint MRB_ARGS_NONE() => 0U;
    }

    public partial class RbClass
    {
        public IntPtr NativeHandler { get; set; }
        public RbState RbState { get; set; } = null!;

        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_method(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)]
            string name,
            [MarshalAs(UnmanagedType.FunctionPtr)]
            NativeMethodSignature nativeMethod,
            uint parameterAspect);
    
        public void DefineMethod( RbClass @class, string name, NativeMethodSignature nativeMethod, uint parameterAspect)
        {
            mrb_define_method(this.RbState.MrbState, @class.NativeHandler, name, nativeMethod, parameterAspect);
        }
    }
}