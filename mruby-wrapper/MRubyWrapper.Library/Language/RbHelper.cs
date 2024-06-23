namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

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

        // MRB_API mrb_value mrb_funcall_argv(mrb_state *mrb, mrb_value val, mrb_sym name, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern uint mrb_funcall_argv(
            IntPtr state,
            UInt64 val,
            UInt64 sym,
            Int64 argc,
            IntPtr argv);

        // MRB_API mrb_sym mrb_intern_cstr(mrb_state *mrb, const char* str);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern UInt32 mrb_intern_cstr(
            IntPtr state,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API mrb_bool mrb_block_given_p(mrb_state *mrb);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern bool mrb_block_given_p(IntPtr mrb);

        public static UInt64 GetInternSymbol(RbState state, string str) => mrb_intern_cstr(state.MrbState, str);

        public static RbValue CallMethod(RbState state, RbValue value, string name, params RbValue[] args)
        {
            int length = args.Length;

            UInt64 resVal;
            var sym = mrb_intern_cstr(state.MrbState, name);

            if (length == 0)
            { 
                resVal = mrb_funcall_argv(
                    state.MrbState,
                    value.NativeValue.Value,
                    sym,
                    length,
                    IntPtr.Zero);
            }
            else
            {
                unsafe
                {
                    var fixedArgs = args.Select(v => v.NativeValue).ToArray();
                    fixed (RbValue.RbNativeValue* p = &fixedArgs[0])
                    {
                        resVal = mrb_funcall_argv(
                            state.MrbState,
                            value.NativeValue.Value,
                            sym,
                            length,
                            new IntPtr(p));
                    }
                }
            }

            return new RbValue(state, resVal);
        }

        public static bool BlockGivenP(RbState state) => mrb_block_given_p(state.MrbState);

        public static void Init(RbState state)
        {
            RbValue.Init(state);
        }
    }
}