namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    public struct RbDataClassType
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public readonly string Name;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public readonly NativeDataObjectFreeFunc FreeFunc;

        public RbDataClassType(string name, NativeDataObjectFreeFunc freeFunc)
        {
            this.Name = name;
            this.FreeFunc = freeFunc;
        }
    }

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

        private static Dictionary<string, IntPtr> RbDataClassMapping { get; } = new Dictionary<string, IntPtr>();

        private static bool RbDataStructExist(string name) => RbDataClassMapping.ContainsKey(name);

        private static void RbDataStructAdd(string name)  {
            var typeStruct = Marshal.AllocHGlobal(Marshal.SizeOf<RbDataClassType>());
            var type = new RbDataClassType(name, NativeDataObjectFreeFunc);
            Marshal.StructureToPtr(type, typeStruct, false);
            RbDataClassMapping.Add(name, typeStruct);
        }
        
        internal static IntPtr GetOrCreateNewRbDataStructPtr(string name) {
            if (RbDataStructExist(name))
            {
                return RbDataClassMapping[name];
            }
            
            RbDataStructAdd(name);
            return RbDataClassMapping[name];
        }

        internal static unsafe NativeMethodSignature BuildCSharpCallbackToNativeCallbackBridgeMethod(CSharpMethodSignature callback)
        {
            UInt64 Lambda(IntPtr state, ulong self)
            {
                var argc = mrb_get_argc(state);
                var argv = mrb_get_argv(state);
                var args = new RbValue[(int)argc];
                for (int i = 0; i < argc; i++)
                {
                    var arg = *(((UInt64*)argv) + i);
                    args[i] = new RbValue(new RbState
                    {
                        NativeHandler = state
                    }, arg);
                }

                var csharpState = new RbState
                {
                    NativeHandler = state
                };
                var csharpSelf = new RbValue(csharpState, self);
                var csharpRes = callback(csharpState, csharpSelf, args);
                return csharpRes.NativeValue;
            }
            return Lambda;
        }
        
        public static IntPtr GetIntPtrOfCSharpObject(object obj) => GCHandle.ToIntPtr(GCHandle.Alloc(obj, GCHandleType.Pinned));

        public static object? GetObjectFromIntPtr(IntPtr ptr) => GCHandle.FromIntPtr(ptr).Target;
        
        private static void NativeDataObjectFreeFunc(IntPtr state, IntPtr data)
        {
            GCHandle handle = GCHandle.FromIntPtr(data);
            handle.Free();
        }

        internal static UInt64 GetInternSymbol(RbState state, string str) => mrb_intern_cstr(state.NativeHandler, str);

        internal static RbValue CallMethod(RbState state, RbValue value, string name, params RbValue[] args)
        {
            int length = args.Length;

            UInt64 resVal;
            var sym = mrb_intern_cstr(state.NativeHandler, name);

            if (length == 0)
            { 
                resVal = mrb_funcall_argv(
                    state.NativeHandler,
                    value.NativeValue,
                    sym,
                    length,
                    null!);
            }
            else
            {
                resVal = mrb_funcall_argv(
                    state.NativeHandler,
                    value.NativeValue,
                    sym,
                    length,
                    args.Select(v => v.NativeValue).ToArray());
            }

            return new RbValue(state, resVal);
        }

        internal static RbValue CallMethodWithBlock(RbState state, RbValue value, string name, RbValue block, params RbValue[] args)
        {
            int length = args.Length;

            UInt64 resVal;
            var sym = mrb_intern_cstr(state.NativeHandler, name);

            if (length == 0)
            {
                resVal = mrb_funcall_with_block(
                    state.NativeHandler,
                    value.NativeValue,
                    sym,
                    length,
                    null!,
                    block.NativeValue);
            }
            else
            {
                resVal = mrb_funcall_with_block(
                    state.NativeHandler,
                    value.NativeValue,
                    sym,
                    length,
                    args.Select(v => v.NativeValue).ToArray(),
                    block.NativeValue);
            }

            return new RbValue(state, resVal);
        }
        
        internal static bool BlockGivenP(RbState state) => mrb_block_given_p(state.NativeHandler);

        internal static string? GetSymbolName(RbState state, UInt64 sym)
        {
            var ptr = mrb_sym_name(state.NativeHandler, sym);
            return Marshal.PtrToStringAnsi(ptr);
        }

        internal static string? GetSymbolDump(RbState state, UInt64 sym)
        {
            var ptr = mrb_sym_dump(state.NativeHandler, sym);
            return Marshal.PtrToStringAnsi(ptr);
        }
        
        internal static RbValue GetSymbolStr(RbState state, UInt64 sym)
        {
            var result = mrb_sym_str(state.NativeHandler, sym);
            return new RbValue(state, result);
        }
        
        internal static RbValue NewRubyString(RbState state, string str)
        {
            var result = mrb_str_new_cstr(state.NativeHandler, str);
            return new RbValue(state, result);
        }

        internal static RbValue PtrToRbValue(RbState state, IntPtr p)
        {
            var result = mrb_ptr_to_mrb_value(p);
            return new RbValue(state, result);
        }
        
        internal static RbClass GetRbClassFromValue(RbState state, RbValue value)
        {
            var ptr = mrb_get_class_ptr(value.NativeValue);
            return new RbClass(ptr, state);
        }
        
        public static IntPtr GetRbObjectPtrFromValue(RbValue value) => mrb_value_to_obj_ptr(value.NativeValue);
        
        public static RbValue GetConst(RbState state, RbValue scope, string name)
        {
            var sym = state.GetInternSymbol(name);
            var result = mrb_const_get(state.NativeHandler, scope.NativeValue, sym);
            return new RbValue(state, result);
        }
        
        public static void SetConst(RbState state, RbValue scope, string name, RbValue val)
        {
            var sym = state.GetInternSymbol(name);
            mrb_const_set(state.NativeHandler, scope.NativeValue, sym, val.NativeValue);
        }
    }
}