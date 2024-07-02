namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt64 NativeMethodSignature(IntPtr state, UInt64 self);

    public delegate RbValue CSharpMethodSignature(RbState state, RbValue self, params RbValue[] args);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NativeDataObjectFreeFunc(IntPtr mrb, IntPtr data);
    
    public partial struct RbClass
    {
        public readonly IntPtr NativeHandler;
        public readonly RbState RbState;

        public RbClass(IntPtr nativeHandler, RbState rbState)
        {
            this.NativeHandler = nativeHandler;
            this.RbState = rbState;
        }

        public RbValue RbObject => RbHelper.PtrToRbValue(this.RbState, this.NativeHandler); 
        
        public RbValue CallMethod(string methodName, params RbValue[] args)
        {
            var classObj = this.RbObject;
            return RbHelper.CallMethod(this.RbState, classObj, methodName, args);
        }
        
        public void DefineMethod(string name, CSharpMethodSignature callback, uint parameterAspect)
        {
            var lambda = BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_method(this.RbState.NativeHandler, this.NativeHandler, name, lambda, parameterAspect);
        }

        private static unsafe NativeMethodSignature BuildCSharpCallbackToNativeCallbackBridgeMethod(CSharpMethodSignature callback)
        {
            NativeMethodSignature lambda = (state, self) =>
            {
                var argc = mrb_get_argc(state);
                var argv = mrb_get_argv(state);
                var args = new RbValue[(int)argc];
                for (int i = 0; i < argc; i++)
                {
                    var arg = *(((UInt64*)argv)+ i);
                    args[i] = new RbValue(new RbState { NativeHandler = state }, arg);
                }

                var csharpState = new RbState { NativeHandler = state };
                var csharpSelf = new RbValue(csharpState, self);
                var csharpRes = callback(csharpState, csharpSelf, args);
                return csharpRes.NativeValue;
            };
            return lambda;
        }

        public void DefineClassMethod(string name, CSharpMethodSignature callback, uint parameterAspect)
        {
            var lambda = BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_class_method(this.RbState.NativeHandler, this.NativeHandler, name, lambda, parameterAspect);
        }

        public void DefineSingletonMethod(string name, CSharpMethodSignature callback, uint parameterAspect)
        {
            var lambda = BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_singleton_method(this.RbState.NativeHandler, this.NativeHandler, name, lambda, parameterAspect);
        }

        public void DefineModuleMethod(string name, CSharpMethodSignature callback, uint parameterAspect)
        {
            var lambda = BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_module_function(this.RbState.NativeHandler, this.NativeHandler, name, lambda, parameterAspect);
        }

        public void DefineConstant(string name, RbValue value)
            => mrb_define_const(this.RbState.NativeHandler, this.NativeHandler, name, value.NativeValue);

        public void UndefMethod(string name)
            => mrb_undef_method(this.RbState.NativeHandler, this.NativeHandler, name);

        public void UndefClassMethod(string name)
            => mrb_undef_class_method(this.RbState.NativeHandler, this.NativeHandler, name);

        public RbValue NewObject(params RbValue[] args)
        {
            int length = args.Length;
            UInt64 value;

            if (length > 0)
            {
                unsafe
                {
                    var fixedArgs = args.Select(v => v.NativeValue).ToArray();

                    fixed (UInt64* p = &fixedArgs[0])
                    {
                        value = mrb_obj_new(this.RbState.NativeHandler, this.NativeHandler, length, new IntPtr(p));
                    }
                }
            }
            else
            {
                value = mrb_obj_new(this.RbState.NativeHandler, this.NativeHandler, length, IntPtr.Zero);
            }

            return new RbValue(this.RbState, value);
        }

        public RbValue NewObjectWithCSharpDataObject(string dataName, object data, params RbValue[] args)
        {
            var dataType = RbHelper.GetOrCreateNewRbDataStructPtr(dataName);
            var dataPtr = RbHelper.GetIntPtrOfCSharpObject(data);
            var obj = mrb_new_data_object(this.RbState.NativeHandler, this.NativeHandler, dataPtr, dataType);
            var ret = new RbValue(this.RbState, obj);
            ret.CallMethod("initialize", args);
            return ret;
        }
        
        public void IncludeModule(RbClass included)
            => mrb_include_module(this.RbState.NativeHandler, this.NativeHandler, included.NativeHandler);

        public void PrependModule(RbClass prepended)
            => mrb_prepend_module(this.RbState.NativeHandler, this.NativeHandler, prepended.NativeHandler);

        public RbClass ClassNew(RbClass super)
        {
            var classPtr = mrb_class_new(this.RbState.NativeHandler, super.NativeHandler);
            return new RbClass(classPtr, this.RbState);
        }

        public RbClass ModuleNew()
        {
            var modulePtr = mrb_module_new(this.RbState.NativeHandler);
            return new RbClass(modulePtr, this.RbState);
        }

        public bool ObjRespondTo(string name) => mrb_obj_respond_to(this.RbState.NativeHandler, this.NativeHandler, RbHelper.GetInternSymbol(this.RbState, name));

        public RbClass DefineClassUnder(RbClass outer, string name, RbClass super)
        {
            var classPtr = mrb_define_class_under(this.RbState.NativeHandler, outer.NativeHandler, name, super.NativeHandler);
            return new RbClass(classPtr, this.RbState);
        }

        public RbClass DefineModuleUnder(RbClass outer, string name)
        {
            var modulePtr = mrb_define_module_under(this.RbState.NativeHandler, outer.NativeHandler, name);
            return new RbClass(modulePtr, this.RbState);
        }

        public void DefineAlias(string a, string b)
        {
            mrb_define_alias(this.RbState.NativeHandler, this.NativeHandler, a, b);
        }

        public void DefineAliasId(UInt64 a, UInt64 b)
        {
            mrb_define_alias_id(this.RbState.NativeHandler, this.NativeHandler, a, b);
        }

        public string? GetClassName()
        {
            var result = mrb_class_name(this.RbState.NativeHandler, this.NativeHandler);
            return Marshal.PtrToStringAnsi(result);
        }

        public RbValue CvGet(string name)
        {
            var mod = RbHelper.PtrToRbValue(this.RbState, this.NativeHandler);
            var sym = RbHelper.GetInternSymbol(this.RbState, name);
            var result = mrb_cv_get(this.NativeHandler, mod.NativeValue, sym);
            return new RbValue(this.RbState, result);
        }

        public void CvSet(string cvName, RbValue val)
        {
            var mod = RbHelper.PtrToRbValue(this.RbState, this.NativeHandler);
            var sym = RbHelper.GetInternSymbol(this.RbState, cvName);
            mrb_cv_set(this.NativeHandler, mod.NativeValue, sym, val.NativeValue);
        }

        public bool CvDefined(string cvName)
        {
            var mod = RbHelper.PtrToRbValue(this.RbState, this.NativeHandler);
            var sym = RbHelper.GetInternSymbol(this.RbState, cvName);
            return mrb_cv_defined(this.NativeHandler, mod.NativeValue, sym);
        }

        public bool IsConstantDefinedAt(string constName)
        {
            var mod = RbHelper.PtrToRbValue(this.RbState, this.NativeHandler);
            var sym = RbHelper.GetInternSymbol(this.RbState, constName);
            return mrb_const_defined_at(this.RbState.NativeHandler, mod.NativeValue, sym);
        }
    }
}