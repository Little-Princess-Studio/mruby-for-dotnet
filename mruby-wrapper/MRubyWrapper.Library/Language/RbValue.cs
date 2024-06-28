namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

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
            var classPtr = mrb_singleton_class_ptr(this.rbState.MrbState, this.NativeValue.Value);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this.rbState,
            };
        }

        // Wrapper for mrb_obj_dup
        public RbValue Duplicate()
        {
            var result = mrb_obj_dup(this.rbState.MrbState, this.NativeValue.Value);
            return new RbValue(this.rbState, result);
        }

        public RbValue Freeze()
        {
            var result = mrb_obj_freeze(this.rbState.MrbState, this.NativeValue.Value);
            return new RbValue(this.rbState, result);
        }

        public Int64 ObjectId() => mrb_obj_id(this.NativeValue.Value);

        public bool DeepEquals(RbValue rbValue) => mrb_equal(this.rbState.MrbState, this.NativeValue.Value, rbValue.NativeValue.Value);

        public Int64 Compare(RbValue rbValue) => mrb_cmp(this.rbState.MrbState, this.NativeValue.Value, rbValue.NativeValue.Value);

        public RbValue ToRbString()
        {
            var result = mrb_any_to_s(this.rbState.MrbState, this.NativeValue.Value);
            return new RbValue(this.rbState, result);
        }

        public string? GetClassName()
        {
            var result = mrb_obj_classname(this.rbState.MrbState, this.NativeValue.Value);
            return Marshal.PtrToStringAnsi(result);
        }

        public RbClass GetClass()
        {
            var classPtr = mrb_obj_class(this.rbState.MrbState, this.NativeValue.Value);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this.rbState,
            };
        }

        public RbValue GetClassPath(RbClass @class)
        {
            var result = mrb_class_path(this.rbState.MrbState, @class.NativeHandler);
            return new RbValue(this.rbState, result);
        }

        public bool IsKindOf(RbClass @class)
        {
            return mrb_obj_is_kind_of(this.rbState.MrbState, this.NativeValue.Value, @class.NativeHandler);
        }

        public RbValue Inspect()
        {
            var result = mrb_obj_inspect(this.rbState.MrbState, this.NativeValue.Value);
            return new RbValue(this.rbState, result);
        }

        public RbValue Clone()
        {
            var result = mrb_obj_clone(this.rbState.MrbState, this.NativeValue.Value);
            return new RbValue(this.rbState, result);
        }
        
        public RbValue GetAttribute(string name)
        {
            var symId = RbHelper.GetInternSymbol(this.rbState, name);
            var result = mrb_attr_get(this.rbState.MrbState, this.NativeValue.Value, symId);
            return new RbValue(this.rbState, result);
        }
        
        // override object.Equals
        public override bool Equals(object? obj)
        {
            if (obj is RbValue rbValue)
            {
                return mrb_obj_eq(this.rbState.MrbState, this.NativeValue.Value, rbValue.NativeValue.Value);
            }

            return false;
        }

        // override object.GetHashCode
        public override int GetHashCode() => this.NativeValue.Value.GetHashCode();
    }
}