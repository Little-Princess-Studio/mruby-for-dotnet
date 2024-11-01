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

        public readonly RbState State;
        private readonly RbNativeValue nativeValue;

        public UInt64 NativeValue => this.nativeValue.Value;

        public RbValue(RbState rbState, UInt64 nativeValue)
        {
            this.State = rbState;
            this.nativeValue = new RbNativeValue
            {
                Value = nativeValue,
            };
        }

        public RbClass SingletonClass
        {
            get
            {
                var classPtr = mrb_singleton_class_ptr(this.State.NativeHandler, this.NativeValue);
                return new RbClass(classPtr, this.State);
            }
        }

        public bool IsInt => RbHelper.IsInteger(this);

        public long ToIntUnchecked() => this.State.UnboxInt(this);

        public long ToInt()
        {
            return this.IsInt ? this.ToIntUnchecked() : throw new Exception("Value is not an integer");
        }

        public bool IsNil => this == this.State.RbNil;

        public bool IsTrue => this == this.State.RbTrue;

        public bool IsFalse => this == this.State.RbFalse;

        public bool IsSymbol => RbHelper.IsSymbol(this);

        public bool IsFloat => RbHelper.IsFloat(this);

        public double ToFloatUnchecked() => this.State.UnboxFloat(this);

        public double ToFloat()
        {
            return this.IsFloat ? this.ToFloatUnchecked() : throw new Exception("Value is not a float");
        }

        public bool IsArray => RbHelper.IsArray(this);

        public RbArray ToArrayUnchecked() => this.State.NewArrayFromArrayObject(this);

        public RbArray ToArray()
        {
            return this.IsArray ? this.ToArrayUnchecked() : throw new Exception("Value is not an array");
        }

        public bool IsString => RbHelper.IsString(this);

        public string? ToStringUnchecked() => this.State.UnboxString(this);

        public override string? ToString()
        {
            return this.IsString ? this.ToStringUnchecked() : throw new Exception("Value is not a string");
        }

        public bool IsHash => RbHelper.IsHash(this);

        public RbHash ToHashUnchecked() => this.State.NewHashFromHashObject(this);

        public RbHash ToHash()
        {
            return this.IsHash ? this.ToHashUnchecked() : throw new Exception("Value is not a hash");
        }

        public bool IsException => RbHelper.IsException(this);

        public bool IsObject => RbHelper.IsObject(this);

        public bool IsClass => RbHelper.IsClass(this);

        public RbClass ToClassUnchecked() => RbHelper.GetRbClassFromValue(this.State, this);

        public RbClass ToClass()
        {
            return this.IsClass ? this.ToClassUnchecked() : throw new Exception("Value is not a class");
        }

        public bool IsModule => RbHelper.IsModule(this);

        public RbClass ToModuleUnchecked() => RbHelper.GetRbClassFromValue(this.State, this);

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
            => RbHelper.CallMethod(this.State, this, name, args);

        public RbValue CallMethodWithBlock(string name, RbValue block, params RbValue[] args)
            => RbHelper.CallMethodWithBlock(this.State, this, name, block, args);

        public RbValue CallMethodWithBlock(string name, RbProc proc, params RbValue[] args)
        {
            var procObj = proc.ToValue();
            return this.CallMethodWithBlock(name, procObj, args);
        }

        // Wrapper for mrb_obj_dup
        public RbValue Duplicate()
        {
            var result = mrb_obj_dup(this.State.NativeHandler, this.nativeValue.Value);
            return new RbValue(this.State, result);
        }

        public RbValue Freeze()
        {
            var result = mrb_obj_freeze(this.State.NativeHandler, this.nativeValue.Value);
            return new RbValue(this.State, result);
        }

        public bool CheckFrozen() => mrb_check_frozen_ex(this.NativeValue);

        public Int64 ObjectId => mrb_obj_id(this.nativeValue.Value);

        public bool StrictEquals(RbValue rbValue)
        {
            var res = mrb_obj_equal(this.State.NativeHandler, this.nativeValue.Value, rbValue.nativeValue.Value);
            return res;
        }

        public Int64 Compare(RbValue rbValue) => mrb_cmp(this.State.NativeHandler, this.nativeValue.Value, rbValue.nativeValue.Value);

        [ExcludeFromCodeCoverage]
        public RbValue ToRbString()
        {
            var result = mrb_any_to_s(this.State.NativeHandler, this.nativeValue.Value);
            return new RbValue(this.State, result);
        }

        public string? GetClassName()
        {
            var result = mrb_obj_classname(this.State.NativeHandler, this.nativeValue.Value);
            return Marshal.PtrToStringAnsi(result);
        }

        public RbClass GetClass()
        {
            var classPtr = mrb_obj_class(this.State.NativeHandler, this.nativeValue.Value);
            return new RbClass(classPtr, this.State);
        }

        public bool IsKindOf(RbClass @class)
        {
            return mrb_obj_is_kind_of(this.State.NativeHandler, this.nativeValue.Value, @class.NativeHandler);
        }

        [ExcludeFromCodeCoverage]
        public RbValue Inspect()
        {
            var result = mrb_obj_inspect(this.State.NativeHandler, this.nativeValue.Value);
            return new RbValue(this.State, result);
        }

        public RbValue Clone()
        {
            var result = mrb_obj_clone(this.State.NativeHandler, this.nativeValue.Value);
            return new RbValue(this.State, result);
        }

        // public RbValue GetAttribute(string name)
        // {
        //     var symId = this.State.GetInternSymbol(name);
        //     var result = mrb_attr_get(this.State.NativeHandler, this.nativeValue.Value, symId);
        //     return new RbValue(this.State, result);
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
                var res = mrb_eql(this.State.NativeHandler, this.nativeValue.Value, rbValue.nativeValue.Value);
                return res;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = mrb_obj_hash(this.State.NativeHandler, this.nativeValue.Value);
            return (int)hashCode;
        }

        // Wrapper methods
        public RbValue GetInstanceVariable(string ivName)
        {
            var sym = this.State.GetInternSymbol(ivName);
            var result = mrb_iv_get(this.State.NativeHandler, this.nativeValue.Value, sym);
            return new RbValue(this.State, result);
        }

        public void SetInstanceVariable(string ivName, RbValue val)
        {
            var sym = this.State.GetInternSymbol(ivName);
            mrb_iv_set(this.State.NativeHandler, this.nativeValue.Value, sym, val.nativeValue.Value);
        }

        public bool IsInstanceVariableDefined(string ivName)
        {
            var sym = this.State.GetInternSymbol(ivName);
            return mrb_iv_defined(this.State.NativeHandler, this.nativeValue.Value, sym);
        }

        public RbValue RemoveInstanceVariable(string ivName)
        {
            var sym = this.State.GetInternSymbol(ivName);
            var result = mrb_iv_remove(this.State.NativeHandler, this.nativeValue.Value, sym);
            return new RbValue(this.State, result);
        }

        public void CopyInstanceVariables(RbValue dst) => mrb_iv_copy(this.State.NativeHandler, dst.nativeValue.Value, this.nativeValue.Value);

        public void InstanceVariableForeach(CSharpIvForeachFunc func)
        {
            var nativeFunc = new IvForeachFunc((state, sym, val, _) =>
            {
                var nativeState = new RbState { NativeHandler = state };
                var name = nativeState.GetSymbolName(sym);
                var value = new RbValue(this.State, val);
                return func(this.State, name!, value).nativeValue.Value;
            });

            mrb_iv_foreach(this.State.NativeHandler, this.nativeValue.Value, nativeFunc, IntPtr.Zero);
        }

        public T? GetDataObject<T>(string typeName) where T : class
        {
            var type = RbHelper.GetOrCreateNewRbDataStructPtr(typeName);
            var ptr = mrb_data_object_get_ptr(this.State.NativeHandler, this.nativeValue.Value, type);
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
            mrb_define_singleton_method(this.State.NativeHandler, objPtr, name, delegateFunc, parameterAspect);
        }

        public RbValue this[string name]
        {
            get => this.GetInstanceVariable(name);
            set => this.SetInstanceVariable(name, value);
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