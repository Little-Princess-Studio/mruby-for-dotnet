namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt64 NativeMethodSignature(IntPtr state, UInt64 self);

    public delegate RbValue CSharpMethodSignature(RbValue self, params RbValue[] args);

    public struct RbClass
    {
        public IntPtr NativeHandler { get; set; }
        public RbState RbState { get; set; }

        public RbClass(IntPtr nativeHandler, RbState rbState)
        {
            this.NativeHandler = nativeHandler;
            this.RbState = rbState;
        }
        
        // MRB_API void mrb_define_method(mrb_state *mrb, struct RClass *cla, const char *name, mrb_func_t func, mrb_aspec aspec);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_method(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodSignature nativeMethod,
            uint parameterAspect);

        // MRB_API void mrb_define_class_method(mrb_state *mrb, struct RClass *cla, const char *name, mrb_func_t fun, mrb_aspec aspec);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_class_method(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodSignature nativeMethod,
            uint parameterAspect);

        // MRB_API void mrb_define_singleton_method(mrb_state *mrb, struct RObject *cla, const char *name, mrb_func_t fun, mrb_aspec aspec);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_singleton_method(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodSignature nativeMethod,
            uint parameterAspect);

        // MRB_API void mrb_define_module_function(mrb_state *mrb, struct RClass *cla, const char *name, mrb_func_t fun, mrb_aspec aspec);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_module_function(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodSignature nativeMethod,
            uint parameterAspect);

        // MRB_API void mrb_define_const(mrb_state* mrb, struct RClass* cla, const char *name, mrb_value val);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_const(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            UInt64 value);

        // MRB_API mrb_value mrb_obj_new(mrb_state *mrb, struct RClass *c, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_obj_new(IntPtr state, IntPtr @class, int argc, IntPtr argv);

        // MRB_API void mrb_include_module(mrb_state *mrb, struct RClass *cla, struct RClass *included);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_include_module(IntPtr mrb, IntPtr cla, IntPtr included);

        // MRB_API void mrb_prepend_module(mrb_state *mrb, struct RClass *cla, struct RClass *prepended);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_prepend_module(IntPtr mrb, IntPtr cla, IntPtr prepended);

        // MRB_API void mrb_undef_method(mrb_state *mrb, struct RClass *cla, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_undef_method(IntPtr mrb, IntPtr cla, [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API void mrb_undef_class_method(mrb_state *mrb, struct RClass *cls, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_undef_class_method(IntPtr mrb, IntPtr cla, [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass * mrb_class_new(mrb_state *mrb, struct RClass *super);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_class_new(IntPtr mrb, IntPtr super);

        // MRB_API struct RClass * mrb_module_new(mrb_state *mrb);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_module_new(IntPtr mrb);

        // MRB_API mrb_bool mrb_obj_respond_to(mrb_state *mrb, struct RClass* c, mrb_sym mid);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool mrb_obj_respond_to(IntPtr mrb, IntPtr c, UInt64 mid);

        // MRB_API struct RClass* mrb_define_class_under(mrb_state *mrb, struct RClass *outer, const char *name, struct RClass *super);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_class_under(
            IntPtr mrb, IntPtr outer, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr super);

        // MRB_API struct RClass* mrb_define_module_under(mrb_state *mrb, struct RClass *outer, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_module_under(
            IntPtr mrb, IntPtr outer, [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API mrb_int mrb_get_argc(mrb_state *mrb);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern Int64 mrb_get_argc(IntPtr mrb);

        // MRB_API const mrb_value *mrb_get_argv(mrb_state *mrb);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_get_argv(IntPtr mrb);

        public void DefineMethod(RbClass @class, string name, CSharpMethodSignature callback, uint parameterAspect)
        {
            var lambda = BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_method(this.RbState.MrbState, @class.NativeHandler, name, lambda, parameterAspect);
        }

        private static unsafe NativeMethodSignature BuildCSharpCallbackToNativeCallbackBridgeMethod(CSharpMethodSignature callback)
        {
            NativeMethodSignature lambda = new NativeMethodSignature((state, self) =>
            {
                var argc = mrb_get_argc(state);
                var argv = mrb_get_argv(state);
                unsafe
                {
                    var args = new RbValue[(int)argc];
                    for (int i = 0; i < argc; i++)
                    {
                        args[i] = new RbValue(new RbState() { MrbState = state}, *(UInt64*)(argv + i));
                    }

                    var csharpSelf = new RbValue(new RbState() { MrbState = state }, self);
                    var csharpRes = callback(csharpSelf, args);
                    return csharpRes.NativeValue.Value;
                }
            });
            return lambda;
        }

        public void DefineClassMethod(RbClass @class, string name, CSharpMethodSignature callback, uint parameterAspect)
        {
            var lambda = BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_class_method(this.RbState.MrbState, @class.NativeHandler, name, lambda, parameterAspect);
        }

        public void DefineSingletonMethod(string name, CSharpMethodSignature callback, uint parameterAspect)
        {
            var lambda = BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_singleton_method(this.RbState.MrbState, this.NativeHandler, name, lambda, parameterAspect);
        }

        public void DefineModuleMethod(string name, CSharpMethodSignature callback, uint parameterAspect)
        {
            var lambda = BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_module_function(this.RbState.MrbState, this.NativeHandler, name, lambda, parameterAspect);
        }

        public void DefineConstant(string name, RbValue value)
            => mrb_define_const(this.RbState.MrbState, this.NativeHandler, name, value.NativeValue.Value);

        public void UndefMethod(string name)
            => mrb_undef_method(this.RbState.MrbState, this.NativeHandler, name);

        public void UndefClassMethod(string name)
            => mrb_undef_class_method(this.RbState.MrbState, this.NativeHandler, name);

        public RbValue NewObject(params RbValue[] args)
        {
            int length = args.Length;
            UInt64 value;

            if (length > 0)
            {
                unsafe
                {
                    var fixedArgs = args.Select(v => v.NativeValue).ToArray();

                    fixed (RbValue.RbNativeValue* p = &fixedArgs[0])
                    {
                        value = mrb_obj_new(this.RbState.MrbState, this.NativeHandler, length, new IntPtr(p));
                    }
                }
            }
            else
            {
                value = mrb_obj_new(this.RbState.MrbState, this.NativeHandler, length, IntPtr.Zero);
            }

            return new RbValue(this.RbState, value);
        }

        public void IncludeModule(RbClass included)
            => mrb_include_module(this.RbState.MrbState, this.NativeHandler, included.NativeHandler);

        public void PrependModule(RbClass prepended)
            => mrb_prepend_module(this.RbState.MrbState, this.NativeHandler, prepended.NativeHandler);

        public RbClass ClassNew(RbClass super)
        {
            var classPtr = mrb_class_new(this.RbState.MrbState, super.NativeHandler);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this.RbState,
            };
        }

        public RbClass ModuleNew()
        {
            var modulePtr = mrb_module_new(this.RbState.MrbState);
            return new RbClass
            {
                NativeHandler = modulePtr,
                RbState = this.RbState,
            };
        }

        public bool ObjRespondTo(string name) => mrb_obj_respond_to(this.RbState.MrbState, this.NativeHandler, RbHelper.GetInternSymbol(this.RbState, name));

        public RbClass DefineClassUnder(RbClass outer, string name, RbClass super)
        {
            var classPtr = mrb_define_class_under(this.RbState.MrbState, outer.NativeHandler, name, super.NativeHandler);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this.RbState,
            };
        }

        public RbClass DefineModuleUnder(RbClass outer, string name)
        {
            var modulePtr = mrb_define_module_under(this.RbState.MrbState, outer.NativeHandler, name);
            return new RbClass
            {
                NativeHandler = modulePtr,
                RbState = this.RbState,
            };
        }
    }
}