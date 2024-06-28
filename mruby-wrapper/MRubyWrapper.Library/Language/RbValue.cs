namespace MRubyWrapper.Library.Language
{
    using System;
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
    }
}