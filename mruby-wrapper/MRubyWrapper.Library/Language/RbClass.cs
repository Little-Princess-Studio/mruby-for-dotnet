namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt64 NativeMethodSignature(IntPtr state, UInt64 self);

    public delegate RbValue CSharpMethodSignature(RbValue self, params RbValue[] args);

    public partial struct RbClass
    {
        public IntPtr NativeHandler { get; set; }
        public RbState RbState { get; set; }

        public RbClass(IntPtr nativeHandler, RbState rbState)
        {
            this.NativeHandler = nativeHandler;
            this.RbState = rbState;
        }
        
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

        public void DefineAlias(string a, string b)
        {
            mrb_define_alias(this.RbState.MrbState, this.NativeHandler, a, b);
        }

        public void DefineAliasId(UInt64 a, UInt64 b)
        {
            mrb_define_alias_id(this.RbState.MrbState, this.NativeHandler, a, b);
        }

        public string? GetClassName()
        {
            var result = mrb_class_name(this.RbState.MrbState, this.NativeHandler);
            return Marshal.PtrToStringAnsi(result);
        }
    }
}