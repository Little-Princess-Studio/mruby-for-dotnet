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
        public readonly RbState State;
        
        public RbClass(IntPtr nativeHandler, RbState rbState)
        {
            this.NativeHandler = nativeHandler;
            this.State = rbState;
        }

        public RbValue ClassObject => RbHelper.PtrToRbValue(this.State, this.NativeHandler);

        public RbValue CallMethod(string methodName, params RbValue[] args)
        {
            var classObj = this.ClassObject;
            return RbHelper.CallMethod(this.State, classObj, methodName, args);
        }

        public RbValue CallMethodWithBlock(string methodName, RbValue block, params RbValue[] args)
        {
            var classObj = this.ClassObject;
            return RbHelper.CallMethodWithBlock(this.State, classObj, methodName, block, args);
        }

        public RbValue CallMethodWithBlock(string methodName, RbProc block, params RbValue[] args)
        {
            return CallMethodWithBlock(methodName, block.ToValue(), args);
        }

        public void DefineMethod(string name, CSharpMethodFunc callback, uint parameterAspect, out NativeMethodFunc delegateFunc)
        {
            var sym = this.State.GetInternSymbol(name);
            this.DefineMethod(sym, callback, parameterAspect, out delegateFunc);
        }
        
        public void DefineMethod(UInt64 name, CSharpMethodFunc callback, uint parameterAspect, out NativeMethodFunc delegateFunc)
        {
            delegateFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_method_id(this.State.NativeHandler, this.NativeHandler, name, delegateFunc, parameterAspect);
        }

        public void DefineClassMethod(string name, CSharpMethodFunc callback, uint parameterAspect, out NativeMethodFunc delegateFunc)
        {
            var sym = this.State.GetInternSymbol(name);
            this.DefineClassMethod(sym, callback, parameterAspect, out delegateFunc);
        }
        
        public void DefineClassMethod(UInt64 name, CSharpMethodFunc callback, uint parameterAspect, out NativeMethodFunc delegateFunc)
        {
            delegateFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_class_method_id(this.State.NativeHandler, this.NativeHandler, name, delegateFunc, parameterAspect);
        }

        public void DefineModuleMethod(string name, CSharpMethodFunc callback, uint parameterAspect, out NativeMethodFunc delegateFunc)
        {
            var sym = this.State.GetInternSymbol(name);
            this.DefineModuleMethod(sym, callback, parameterAspect, out delegateFunc);
        }
        
        public void DefineModuleMethod(UInt64 name, CSharpMethodFunc callback, uint parameterAspect, out NativeMethodFunc delegateFunc)
        {
            delegateFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            mrb_define_module_function_id(this.State.NativeHandler, this.NativeHandler, name, delegateFunc, parameterAspect);
        }

        public void DefineConstant(string name, RbValue value)
        {
            var sym = this.State.GetInternSymbol(name);
            this.DefineConstant(sym, value);
        }
        
        public void DefineConstant(UInt64 name, RbValue value)
            => mrb_define_const_id(this.State.NativeHandler, this.NativeHandler, name, value.NativeValue);

        public void UndefMethod(string name)
        {
            var sym = this.State.GetInternSymbol(name);
            this.UndefMethod(sym);
        }

        public void UndefMethod(UInt64 name)
            => mrb_undef_method_id(this.State.NativeHandler, this.NativeHandler, name);
        
        public void UndefClassMethod(string name)
        {
            var sym = this.State.GetInternSymbol(name);
            this.UndefClassMethod(sym);
        }

        public void UndefClassMethod(UInt64 name)
            => mrb_undef_method_id(this.State.NativeHandler, this.NativeHandler, name);
        
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
                        value = mrb_obj_new(this.State.NativeHandler, this.NativeHandler, length, new IntPtr(p));
                    }
                }
            }
            else
            {
                value = mrb_obj_new(this.State.NativeHandler, this.NativeHandler, length, IntPtr.Zero);
            }

            return new RbValue(this.State, value);
        }

        public RbValue NewObjectWithCSharpDataObject<T>(string dataName, T data, params RbValue[] args) where T : class
        {
            var dataType = RbHelper.GetOrCreateNewRbDataStructPtr(dataName);
            var dataPtr = RbHelper.GetIntPtrOfCSharpObject(data);
            var obj = mrb_new_data_object(this.State.NativeHandler, this.NativeHandler, dataPtr, dataType);
            var ret = new RbValue(this.State, obj);
            ret.CallMethod("initialize", args);
            return ret;
        }

        public RbValue NewObjectWithCSharpDataObject<T>(string dataName, T data, Action<RbState, object?> releaseFn, params RbValue[] args) where T : class
        {
            var dataType = RbHelper.GetOrCreateNewRbDataStructPtr(dataName, releaseFn);
            var dataPtr = RbHelper.GetIntPtrOfCSharpObject(data);
            var obj = mrb_new_data_object(this.State.NativeHandler, this.NativeHandler, dataPtr, dataType);
            var ret = new RbValue(this.State, obj);
            ret.CallMethod("initialize", args);
            return ret;
        }

        public void IncludeModule(RbClass included)
            => mrb_include_module(this.State.NativeHandler, this.NativeHandler, included.NativeHandler);

        public void PrependModule(RbClass prepended)
            => mrb_prepend_module(this.State.NativeHandler, this.NativeHandler, prepended.NativeHandler);

        public bool ObjRespondTo(string name) => mrb_obj_respond_to(this.State.NativeHandler, this.NativeHandler, this.State.GetInternSymbol(name));

        public void DefineAlias(string a, string b)
        {
            var aSym = this.State.GetInternSymbol(a);
            var bSym = this.State.GetInternSymbol(b);
            mrb_define_alias_id(this.State.NativeHandler, this.NativeHandler, aSym, bSym);
        }

        public void DefineAliasId(UInt64 a, UInt64 b)
        {
            mrb_define_alias_id(this.State.NativeHandler, this.NativeHandler, a, b);
        }

        public string? GetClassName()
        {
            var result = mrb_class_name(this.State.NativeHandler, this.NativeHandler);
            return Marshal.PtrToStringAnsi(result);
        }

        public RbValue GetClassVariable(string name)
        {
            var mod = this.State.PtrToRbValue(this.NativeHandler);
            var sym = this.State.GetInternSymbol(name);
            var result = mrb_cv_get(this.NativeHandler, mod.NativeValue, sym);
            return new RbValue(this.State, result);
        }

        public void SetClassVariable(string cvName, RbValue val)
        {
            var mod = this.State.PtrToRbValue(this.NativeHandler);
            var sym = this.State.GetInternSymbol(cvName);
            mrb_cv_set(this.State.NativeHandler, mod.NativeValue, sym, val.NativeValue);
        }

        public bool ClassVariableDefined(string cvName)
        {
            var mod = this.State.PtrToRbValue(this.NativeHandler);
            var sym = this.State.GetInternSymbol(cvName);
            var defined = mrb_cv_defined(this.State.NativeHandler, mod.NativeValue, sym);
            return defined;
        }

        public RbValue GetConst(string name) => RbHelper.GetConst(this.State, this.ClassObject, name);

        public void SetConst(string name, RbValue val) => RbHelper.SetConst(this.State, this.ClassObject, name, val);

        public bool ConstDefined(string name)
        {
            var sym = this.State.GetInternSymbol(name);
            return mrb_const_defined(this.State.NativeHandler, this.NativeHandler, sym);
        }

        public void RemoveConst(string name)
        {
            var sym = this.State.GetInternSymbol(name);
            mrb_const_remove(this.State.NativeHandler, this.NativeHandler, sym);
        }

        public RbValue GetClassPath()
        {
            var result = mrb_class_path(this.State.NativeHandler, this.NativeHandler);
            return new RbValue(this.State, result);
        }

        public RbValue this[string name]
        {
            get => this.GetClassVariable(name);
            set => this.SetClassVariable(name, value);
        }
    }
}