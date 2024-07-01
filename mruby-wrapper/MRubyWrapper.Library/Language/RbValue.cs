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

        private readonly RbState rbState;
        internal readonly RbNativeValue NativeValue;

        public RbValue(RbState rbState, UInt64 nativeValue)
        {
            this.rbState = rbState;
            this.NativeValue = new RbNativeValue
            {
                Value = nativeValue,
            };
        }

        public RbValue CallMethod(string name, params RbValue[] args)
            => RbHelper.CallMethod(this.rbState, this, name, args);

        public RbClass SingletonClass()
        {
            var classPtr = mrb_singleton_class_ptr(this.rbState.NativeHandler, this.NativeValue.Value);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this.rbState,
            };
        }

        // Wrapper for mrb_obj_dup
        public RbValue Duplicate()
        {
            var result = mrb_obj_dup(this.rbState.NativeHandler, this.NativeValue.Value);
            return new RbValue(this.rbState, result);
        }

        public RbValue Freeze()
        {
            var result = mrb_obj_freeze(this.rbState.NativeHandler, this.NativeValue.Value);
            return new RbValue(this.rbState, result);
        }

        public Int64 ObjectId() => mrb_obj_id(this.NativeValue.Value);

        public bool StrictEquals(RbValue rbValue)
        {
            var res = mrb_obj_eq(this.rbState.NativeHandler, this.NativeValue.Value, rbValue.NativeValue.Value);
            return res;
        }

        public Int64 Compare(RbValue rbValue) => mrb_cmp(this.rbState.NativeHandler, this.NativeValue.Value, rbValue.NativeValue.Value);

        public RbValue ToRbString()
        {
            var result = mrb_any_to_s(this.rbState.NativeHandler, this.NativeValue.Value);
            return new RbValue(this.rbState, result);
        }

        public string? GetClassName()
        {
            var result = mrb_obj_classname(this.rbState.NativeHandler, this.NativeValue.Value);
            return Marshal.PtrToStringAnsi(result);
        }

        public RbClass GetClass()
        {
            var classPtr = mrb_obj_class(this.rbState.NativeHandler, this.NativeValue.Value);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this.rbState,
            };
        }

        public RbValue GetClassPath(RbClass @class)
        {
            var result = mrb_class_path(this.rbState.NativeHandler, @class.NativeHandler);
            return new RbValue(this.rbState, result);
        }

        public bool IsKindOf(RbClass @class)
        {
            return mrb_obj_is_kind_of(this.rbState.NativeHandler, this.NativeValue.Value, @class.NativeHandler);
        }

        public RbValue Inspect()
        {
            var result = mrb_obj_inspect(this.rbState.NativeHandler, this.NativeValue.Value);
            return new RbValue(this.rbState, result);
        }

        public RbValue Clone()
        {
            var result = mrb_obj_clone(this.rbState.NativeHandler, this.NativeValue.Value);
            return new RbValue(this.rbState, result);
        }
        
        public RbValue GetAttribute(string name)
        {
            var symId = RbHelper.GetInternSymbol(this.rbState, name);
            var result = mrb_attr_get(this.rbState.NativeHandler, this.NativeValue.Value, symId);
            return new RbValue(this.rbState, result);
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
                var res = mrb_eql(this.rbState.NativeHandler, this.NativeValue.Value, rbValue.NativeValue.Value);
                return res;
            }

            return false;
        }

        public override int GetHashCode() => this.NativeValue.Value.GetHashCode();

        // Wrapper methods
        public RbValue GetInstanceVariable(string ivName)
        {
            var sym = RbHelper.GetInternSymbol(this.rbState, ivName);
            var result = mrb_iv_get(this.rbState.NativeHandler, this.NativeValue.Value, sym);
            return new RbValue(this.rbState, result);
        }

        public void SetInstanceVariable(string ivName, RbValue val)
        {
            var sym = RbHelper.GetInternSymbol(this.rbState, ivName);
            mrb_iv_set(this.rbState.NativeHandler, this.NativeValue.Value, sym, val.NativeValue.Value);
        }

        public bool IsInstanceVariableDefined(string ivName)
        {
            var sym = RbHelper.GetInternSymbol(this.rbState, ivName);
            return mrb_iv_defined(this.rbState.NativeHandler, this.NativeValue.Value, sym);
        }

        public RbValue RemoveInstanceVariable(string ivName)
        {
            var sym = RbHelper.GetInternSymbol(this.rbState, ivName);
            var result = mrb_iv_remove(this.rbState.NativeHandler, this.NativeValue.Value, sym);
            return new RbValue(this.rbState, result);
        }

        public void CopyInstanceVariables(RbValue dst) => mrb_iv_copy(this.rbState.NativeHandler, dst.NativeValue.Value, this.NativeValue.Value);
        
        public void IvForeach(CSharpIvForeachFunc func, IntPtr data)
        {
            var nativeFunc = new IvForeachFunc((state, sym, val, nativeData) =>
            {
                var nativeState = new RbState() { NativeHandler = state };
                var name = RbHelper.GetSymbolName(nativeState, sym);
                var self = new RbValue(this.rbState, val);
                return func(this.rbState, name!, self, nativeData).NativeValue.Value;
            });
            mrb_iv_foreach(this.rbState.NativeHandler, this.NativeValue.Value, nativeFunc, data);
        }
        
        public T? GetDataObject<T>(string typeName) where T : class
        {
            var type = RbHelper.GetOrCreateNewRbDataStructPtr(typeName);
            var ptr = mrb_data_object_get_ptr(this.rbState.NativeHandler, this.NativeValue.Value, type);
            return (T?)RbHelper.GetObjectFromIntPtr(ptr);
        }
        
        public RbDataClassType GetDataObjectType()
        {
            var ptr = mrb_data_object_get_type(this.NativeValue.Value);
            return Marshal.PtrToStructure<RbDataClassType>(ptr);
        }
    }
}