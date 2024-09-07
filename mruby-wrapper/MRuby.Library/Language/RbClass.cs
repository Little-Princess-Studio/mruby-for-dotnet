namespace MRuby.Library.Language
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt64 NativeMethodFunc(IntPtr state, UInt64 self);

    public delegate RbValue CSharpMethodFunc(RbState state, RbValue self, params RbValue[] args);

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

        public RbValue ClassObject => RbHelper.PtrToRbValue(this.RbState, this.NativeHandler);

        public RbValue CallMethod(string methodName, params RbValue[] args)
        {
            var classObj = this.ClassObject;
            return RbHelper.CallMethod(this.RbState, classObj, methodName, args);
        }

        public RbValue CallMethodWithBlock(string methodName, RbValue block, params RbValue[] args)
        {
            var classObj = this.ClassObject;
            return RbHelper.CallMethodWithBlock(this.RbState, classObj, methodName, block, args);
        }

        public RbValue CallMethodWithBlock(string methodName, RbProc block, params RbValue[] args)
        {
            return CallMethodWithBlock(methodName, block.ToRbValue(), args);
        }

        public void DefineMethod(string name, CSharpMethodFunc callback, uint parameterAspect)
        {
            var lambda = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_method(this.RbState.NativeHandler, this.NativeHandler, name, lambda, parameterAspect);
        }

        public void DefineClassMethod(string name, CSharpMethodFunc callback, uint parameterAspect)
        {
            var lambda = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_class_method(this.RbState.NativeHandler, this.NativeHandler, name, lambda, parameterAspect);
        }

        public void DefineModuleMethod(string name, CSharpMethodFunc callback, uint parameterAspect)
        {
            var lambda = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
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

        public RbValue NewObjectWithCSharpDataObject<T>(string dataName, T data, params RbValue[] args) where T : class
        {
            var dataType = RbHelper.GetOrCreateNewRbDataStructPtr(dataName);
            var dataPtr = RbHelper.GetIntPtrOfCSharpObject(data);
            var obj = mrb_new_data_object(this.RbState.NativeHandler, this.NativeHandler, dataPtr, dataType);
            var ret = new RbValue(this.RbState, obj);
            ret.CallMethod("initialize", args);
            return ret;
        }

        public RbValue NewObjectWithCSharpDataObject<T>(string dataName, T data, Action<RbState, object?> releaseFn, params RbValue[] args) where T : class
        {
            var dataType = RbHelper.GetOrCreateNewRbDataStructPtr(dataName, releaseFn);
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

        public bool ObjRespondTo(string name) => mrb_obj_respond_to(this.RbState.NativeHandler, this.NativeHandler, this.RbState.GetInternSymbol(name));

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

        public RbValue GetClassVariable(string name)
        {
            var mod = this.RbState.PtrToRbValue(this.NativeHandler);
            var sym = this.RbState.GetInternSymbol(name);
            var result = mrb_cv_get(this.NativeHandler, mod.NativeValue, sym);
            return new RbValue(this.RbState, result);
        }

        public void SetClassVariable(string cvName, RbValue val)
        {
            var mod = this.RbState.PtrToRbValue(this.NativeHandler);
            var sym = this.RbState.GetInternSymbol(cvName);
            mrb_cv_set(this.RbState.NativeHandler, mod.NativeValue, sym, val.NativeValue);
        }

        public bool ClassVariableDefined(string cvName)
        {
            var mod = this.RbState.PtrToRbValue(this.NativeHandler);
            var sym = this.RbState.GetInternSymbol(cvName);
            var defined = mrb_cv_defined(this.RbState.NativeHandler, mod.NativeValue, sym);
            return defined;
        }

        public RbValue GetConst(string name) => RbHelper.GetConst(this.RbState, this.ClassObject, name);

        public void SetConst(string name, RbValue val) => RbHelper.SetConst(this.RbState, this.ClassObject, name, val);

        public bool ConstDefined(string name)
        {
            var sym = this.RbState.GetInternSymbol(name);
            return mrb_const_defined(this.RbState.NativeHandler, this.NativeHandler, sym);
        }

        public void RemoveConst(string name)
        {
            var sym = this.RbState.GetInternSymbol(name);
            mrb_const_remove(this.RbState.NativeHandler, this.NativeHandler, sym);
        }

        public RbValue GetClassPath()
        {
            var result = mrb_class_path(this.RbState.NativeHandler, this.NativeHandler);
            return new RbValue(this.RbState, result);
        }
    }
}