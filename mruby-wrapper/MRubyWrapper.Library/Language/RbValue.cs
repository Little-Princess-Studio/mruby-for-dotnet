namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt64 IvForeachFunc(IntPtr state, UInt64 sym, UInt64 val, IntPtr data);
    
    public delegate RbValue CSharpIvForeachFunc(RbState state, string name, RbValue self, IntPtr data);
    
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

        public RbValue SingletonClassObject
        {
            get {
                var classPtr = mrb_singleton_class_ptr(this.RbState.NativeHandler, this.NativeValue);
                return this.RbState.PtrToRbValue(classPtr);
            }
        }
        
        public RbClass SingletonClass
        {
            get {
                var classPtr = mrb_singleton_class_ptr(this.RbState.NativeHandler, this.NativeValue);
                return new RbClass(classPtr, this.RbState);
            }
        }
        
        public RbValue CallMethod(string name, params RbValue[] args)
            => RbHelper.CallMethod(this.RbState, this, name, args);

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

        public Int64 ObjectId => mrb_obj_id(this.nativeValue.Value);

        public bool StrictEquals(RbValue rbValue)
        {
            var res = mrb_obj_equal(this.RbState.NativeHandler, this.nativeValue.Value, rbValue.nativeValue.Value);
            return res;
        }

        public Int64 Compare(RbValue rbValue) => mrb_cmp(this.RbState.NativeHandler, this.nativeValue.Value, rbValue.nativeValue.Value);

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

        public RbValue GetClassPath(RbClass @class)
        {
            var result = mrb_class_path(this.RbState.NativeHandler, @class.NativeHandler);
            return new RbValue(this.RbState, result);
        }

        public bool IsKindOf(RbClass @class)
        {
            return mrb_obj_is_kind_of(this.RbState.NativeHandler, this.nativeValue.Value, @class.NativeHandler);
        }

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
        
        public RbValue GetAttribute(string name)
        {
            var symId = this.RbState.GetInternSymbol(name);
            var result = mrb_attr_get(this.RbState.NativeHandler, this.nativeValue.Value, symId);
            return new RbValue(this.RbState, result);
        }
        
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

        public override int GetHashCode() => this.nativeValue.Value.GetHashCode();

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
        
        public void IvForeach(CSharpIvForeachFunc func, IntPtr data)
        {
            var nativeFunc = new IvForeachFunc((state, sym, val, nativeData) =>
            {
                var nativeState = new RbState() { NativeHandler = state };
                var name = nativeState.GetSymbolName(sym);
                var self = new RbValue(this.RbState, val);
                return func(this.RbState, name!, self, nativeData).nativeValue.Value;
            });
            mrb_iv_foreach(this.RbState.NativeHandler, this.nativeValue.Value, nativeFunc, data);
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
        
        public void DefineSingletonMethod(string name, CSharpMethodSignature callback, uint parameterAspect)
        {
            var lambda = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            var objPtr = RbHelper.GetRbObjectPtrFromValue(this);
            mrb_define_singleton_method(this.RbState.NativeHandler, objPtr, name, lambda, parameterAspect);
        }
    }
}