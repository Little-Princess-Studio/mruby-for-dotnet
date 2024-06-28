namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    public static partial class RbHelper
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
                    null);
            }
            else
            {
                resVal = mrb_funcall_argv(
                    state.MrbState,
                    value.NativeValue.Value,
                    sym,
                    length,
                    args.Select(v => v.NativeValue.Value).ToArray());
            }

            return new RbValue(state, resVal);
        }

        public static RbValue CallMethodWithBlock(RbState state, RbValue value, string name, RbValue block, params RbValue[] args)
        {
            int length = args.Length;

            UInt64 resVal;
            var sym = mrb_intern_cstr(state.MrbState, name);

            if (length == 0)
            {
                resVal = mrb_funcall_with_block(
                    state.MrbState,
                    value.NativeValue.Value,
                    sym,
                    length,
                    null,
                    block.NativeValue.Value);
            }
            else
            {
                resVal = mrb_funcall_with_block(
                    state.MrbState,
                    value.NativeValue.Value,
                    sym,
                    length,
                    args.Select(v => v.NativeValue.Value).ToArray(),
                    block.NativeValue.Value);
            }

            return new RbValue(state, resVal);
        }
        
        public static bool BlockGivenP(RbState state) => mrb_block_given_p(state.MrbState);

        public static string? GetSymbolName(RbState state, UInt64 sym)
        {
            var ptr = mrb_sym_name(state.MrbState, sym);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static string? GetSymbolDump(RbState state, UInt64 sym)
        {
            var ptr = mrb_sym_dump(state.MrbState, sym);
            return Marshal.PtrToStringAnsi(ptr);
        }
        
        public static RbValue GetSymbolStr(RbState state, UInt64 sym)
        {
            var result = mrb_sym_str(state.MrbState, sym);
            return new RbValue(state, result);
        }
        
        public static RbValue NewRubyString(RbState state, string str)
        {
            var result = mrb_str_new_cstr(state.MrbState, str);
            return new RbValue(state, result);
        }
    }
}