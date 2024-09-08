namespace MRuby.Library.Language
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt64 IvForeachFunc(IntPtr state, UInt64 sym, UInt64 val, IntPtr data);

    public delegate RbValue CSharpIvForeachFunc(RbState state, string name, RbValue value);

    public partial class RbValue
    {
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        internal struct RbNativeValue
        {
            public UInt64 Value;
        }

        public readonly RbState RbState;
        private readonly RbNativeValue nativeValue;

        public UInt64 NativeValue => this.nativeValue.Value;

        public RbValue(RbState rbState, UInt64 nativeValue)
        {
            this.RbState = rbState;
            this.nativeValue = new RbNativeValue
            {
                Value = nativeValue,
            };
        }

        public RbClass SingletonClass
        {
            get
            {
                var classPtr = mrb_singleton_class_ptr(this.RbState.NativeHandler, this.NativeValue);
                return new RbClass(classPtr, this.RbState);
            }
        }

        public bool IsInteger => RbHelper.IsInteger(this);

        public long ToIntUnchecked() => this.RbState.UnboxInt(this);
        
        public long ToInt()
        {
            return this.IsInteger ?  this.ToIntUnchecked(): throw new Exception("Value is not an integer");
        }
        
        public bool IsNil => this == this.RbState.RbNil;

        public bool IsTrue => this == this.RbState.RbTrue;

        public bool IsFalse => this == this.RbState.RbFalse;

        public bool IsSymbol => RbHelper.IsSymbol(this);

        public bool IsFloat => RbHelper.IsFloat(this);

        public double ToFloatUnchecked() => this.RbState.UnboxFloat(this);

        public double ToFloat()
        {
            return this.IsFloat ? this.ToFloatUnchecked(): throw new Exception("Value is not a float");
        }

        public bool IsArray => RbHelper.IsArray(this);

        public RbArray ToArrayUnchecked() => this.RbState.NewArrayFromArrayObject(this);

        public RbArray ToArray()
        {
            return this.IsArray ? this.ToArrayUnchecked() : throw new Exception("Value is not an array");
        }

        public bool IsString => RbHelper.IsString(this);

        public string? ToStringUnchecked() => this.RbState.UnboxString(this);
        
        public override string? ToString()
        {
            return this.IsString ? this.ToStringUnchecked() : throw new Exception("Value is not a string");
        }

        public bool IsHash => RbHelper.IsHash(this);

        public RbHash ToHashUnchecked() => this.RbState.NewHashFromHashObject(this);
        
        public RbHash ToHash()
        {
            return this.IsHash ? this.ToHashUnchecked() : throw new Exception("Value is not a hash");
        }
        
        public bool IsException => RbHelper.IsException(this);

        public bool IsObject => RbHelper.IsObject(this);

        public bool IsClass => RbHelper.IsClass(this);

        public RbClass ToClassUnchecked() => RbHelper.GetRbClassFromValue(this.RbState, this);

        public RbClass ToClass()
        {
            return this.IsClass ? this.ToClassUnchecked() : throw new Exception("Value is not a class");
        }
        
        public bool IsModule => RbHelper.IsModule(this);

        public RbClass ToModuleUnchecked() => RbHelper.GetRbClassFromValue(this.RbState, this);
        
        public RbClass ToModule()
        {
            return this.IsModule ? this.ToModuleUnchecked() : throw new Exception("Value is not a module");
        }
        
        public bool IsSingletonClass => RbHelper.IsSClass(this);

        public bool IsProc => RbHelper.IsProc(this);

        public RbProc ToProcUnchecked() => RbProc.FromRbValue(this);
        
        public RbProc ToProc()
        {
            return this.IsProc ? this.ToProcUnchecked() : throw new Exception("Value is not a proc");
        }

        public bool IsRange => RbHelper.IsRange(this);

        public bool IsFiber => RbHelper.IsFiber(this);

        public RbValue CallMethod(string name, params RbValue[] args)
            => RbHelper.CallMethod(this.RbState, this, name, args);

        public RbValue CallMethodWithBlock(string name, RbValue block, params RbValue[] args)
            => RbHelper.CallMethodWithBlock(this.RbState, this, name, block, args);

        public RbValue CallMethodWithBlock(string name, RbProc proc, params RbValue[] args)
        {
            var procObj = proc.ToValue();
            return this.CallMethodWithBlock(name, procObj, args);
        }

        // Wrapper for mrb_obj_dup
        public RbValue Duplicate()
        {
            var result = mrb_obj_dup(this.RbState.NativeHandler, this.nativeValue.Value);
            return new RbValue(this.RbState, result);
        }

        public RbValue Freeze()
        {
            var result = mrb_obj_freeze(this.RbState.NativeHandler, this.nativeValue.Value);
            return new RbValue(this.RbState, result);
        }

        public bool CheckFrozen() => mrb_check_frozen_ex(this.NativeValue);

        public Int64 ObjectId => mrb_obj_id(this.nativeValue.Value);

        public bool StrictEquals(RbValue rbValue)
        {
            var res = mrb_obj_equal(this.RbState.NativeHandler, this.nativeValue.Value, rbValue.nativeValue.Value);
            return res;
        }

        public Int64 Compare(RbValue rbValue) => mrb_cmp(this.RbState.NativeHandler, this.nativeValue.Value, rbValue.nativeValue.Value);

        [ExcludeFromCodeCoverage]
        public RbValue ToRbString()
        {
            var result = mrb_any_to_s(this.RbState.NativeHandler, this.nativeValue.Value);
            return new RbValue(this.RbState, result);
        }

        public string? GetClassName()
        {
            var result = mrb_obj_classname(this.RbState.NativeHandler, this.nativeValue.Value);
            return Marshal.PtrToStringAnsi(result);
        }

        public RbClass GetClass()
        {
            var classPtr = mrb_obj_class(this.RbState.NativeHandler, this.nativeValue.Value);
            return new RbClass(classPtr, this.RbState);
        }

        public bool IsKindOf(RbClass @class)
        {
            return mrb_obj_is_kind_of(this.RbState.NativeHandler, this.nativeValue.Value, @class.NativeHandler);
        }

        [ExcludeFromCodeCoverage]
        public RbValue Inspect()
        {
            var result = mrb_obj_inspect(this.RbState.NativeHandler, this.nativeValue.Value);
            return new RbValue(this.RbState, result);
        }

        public RbValue Clone()
        {
            var result = mrb_obj_clone(this.RbState.NativeHandler, this.nativeValue.Value);
            return new RbValue(this.RbState, result);
        }

        // public RbValue GetAttribute(string name)
        // {
        //     var symId = this.RbState.GetInternSymbol(name);
        //     var result = mrb_attr_get(this.RbState.NativeHandler, this.nativeValue.Value, symId);
        //     return new RbValue(this.RbState, result);
        // }

        public static bool operator ==(RbValue left, RbValue right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(RbValue left, RbValue right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            if (obj is RbValue rbValue)
            {
                var res = mrb_eql(this.RbState.NativeHandler, this.nativeValue.Value, rbValue.nativeValue.Value);
                return res;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = mrb_obj_hash(this.RbState.NativeHandler, this.nativeValue.Value);
            return (int)hashCode;
        }

        // Wrapper methods
        public RbValue GetInstanceVariable(string ivName)
        {
            var sym = this.RbState.GetInternSymbol(ivName);
            var result = mrb_iv_get(this.RbState.NativeHandler, this.nativeValue.Value, sym);
            return new RbValue(this.RbState, result);
        }

        public void SetInstanceVariable(string ivName, RbValue val)
        {
            var sym = this.RbState.GetInternSymbol(ivName);
            mrb_iv_set(this.RbState.NativeHandler, this.nativeValue.Value, sym, val.nativeValue.Value);
        }

        public bool IsInstanceVariableDefined(string ivName)
        {
            var sym = this.RbState.GetInternSymbol(ivName);
            return mrb_iv_defined(this.RbState.NativeHandler, this.nativeValue.Value, sym);
        }

        public RbValue RemoveInstanceVariable(string ivName)
        {
            var sym = this.RbState.GetInternSymbol(ivName);
            var result = mrb_iv_remove(this.RbState.NativeHandler, this.nativeValue.Value, sym);
            return new RbValue(this.RbState, result);
        }

        public void CopyInstanceVariables(RbValue dst) => mrb_iv_copy(this.RbState.NativeHandler, dst.nativeValue.Value, this.nativeValue.Value);

        public void InstanceVariableForeach(CSharpIvForeachFunc func)
        {
            var nativeFunc = new IvForeachFunc((state, sym, val, _) =>
            {
                var nativeState = new RbState { NativeHandler = state };
                var name = nativeState.GetSymbolName(sym);
                var value = new RbValue(this.RbState, val);
                return func(this.RbState, name!, value).nativeValue.Value;
            });

            mrb_iv_foreach(this.RbState.NativeHandler, this.nativeValue.Value, nativeFunc, IntPtr.Zero);
        }

        public T? GetDataObject<T>(string typeName) where T : class
        {
            var type = RbHelper.GetOrCreateNewRbDataStructPtr(typeName);
            var ptr = mrb_data_object_get_ptr(this.RbState.NativeHandler, this.nativeValue.Value, type);
            return (T?)RbHelper.GetObjectFromIntPtr(ptr);
        }

        public RbDataClassType GetDataObjectType()
        {
            var ptr = mrb_data_object_get_type(this.nativeValue.Value);
            return Marshal.PtrToStructure<RbDataClassType>(ptr);
        }

        public void DefineSingletonMethod(string name, CSharpMethodFunc callback, uint parameterAspect, out NativeMethodFunc delegateFunc)
        {
            delegateFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            var objPtr = RbHelper.GetRbObjectPtrFromValue(this);
            mrb_define_singleton_method(this.RbState.NativeHandler, objPtr, name, delegateFunc, parameterAspect);
        }
    }

    public static class CSharpValueExtension
    {
        public static RbValue ToValue(this int value, RbState state) => state.BoxInt(value);

        public static RbValue ToValue(this long value, RbState state) => state.BoxInt(value);

        public static RbValue ToValue(this float value, RbState state) => state.BoxFloat(value);

        public static RbValue ToValue(this double value, RbState state) => state.BoxFloat(value);

        public static RbValue ToValue(this string value, RbState state) => state.BoxString(value);

        public static RbValue ToValue(this bool value, RbState state) => value ? state.RbTrue : state.RbFalse;

        public static RbValue ToValue(this RbProc value) => RbHelper.PtrToRbValue(value.State, value.NativeHandler);

        public static RbValue ToValue(this RbClass value) => value.ClassObject;

        public static RbValue ToValue(this RbArray value) => value.Value;

        public static RbValue ToValue(this RbHash value) => value.Value;
    }
}