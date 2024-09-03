namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    public partial class RbArray
    {
        public readonly RbValue Value;
        public readonly RbState RbState;

        public Int64 Size => mrb_array_len(this.Value.NativeValue);
        
        private RbArray(RbValue value)
        {
            this.Value = value;
            this.RbState = value.RbState;
        }

        internal static RbArray FromArrayObject(RbValue value)
        {
            // todo: check if value is an array of ruby
            return new RbArray(value);
        }
        
        internal static RbArray New(RbState state)
        {
            var arrayPtr = mrb_ary_new(state.NativeHandler);
            return FromArrayObject(new RbValue(state, arrayPtr));
        }

        internal static RbArray FromValues(RbState state, IEnumerable<RbValue> values)
        {
            var enumerable = values as RbValue[] ?? values.ToArray();
            var size = enumerable.Count();
            var valArr = enumerable.Select(v => v.NativeValue).ToArray();
            unsafe
            {
                fixed (UInt64* valuePtr = valArr)
                {
                    var converted = (IntPtr)valuePtr;
                    var arrayPtr = mrb_ary_new_from_values(state.NativeHandler, size, converted);
                    return FromArrayObject(new RbValue(state, arrayPtr));
                }
            }
        }

        public static RbArray AssocNew(RbState state, RbValue car, RbValue cdr)
        {
            var assocPtr = mrb_assoc_new(state.NativeHandler, car.NativeValue, cdr.NativeValue);
            return FromArrayObject(new RbValue(state, assocPtr));
        }

        public void Concat(RbArray other) => mrb_ary_concat(this.RbState.NativeHandler, this.Value.NativeValue, other.Value.NativeValue);

        public void Push(RbValue value) => mrb_ary_push(this.RbState.NativeHandler, this.Value.NativeValue, value.NativeValue);

        public RbValue Pop()
        {
            var valuePtr = mrb_ary_pop(this.RbState.NativeHandler, this.Value.NativeValue);
            return new RbValue(this.RbState, valuePtr);
        }

        public void Set(int index, RbValue value) => mrb_ary_set(this.RbState.NativeHandler, this.Value.NativeValue, index, value.NativeValue);

        public void Replace(RbArray other) => mrb_ary_replace(this.RbState.NativeHandler, this.Value.NativeValue, other.Value.NativeValue);

        public void Unshift(RbValue item) => mrb_ary_unshift(this.RbState.NativeHandler, this.Value.NativeValue, item.NativeValue);

        public RbValue Get(int offset)
        {
            var valuePtr = mrb_ary_entry(this.Value.NativeValue, offset);
            return new RbValue(this.RbState, valuePtr);
        }

        public RbArray Splice(int head, int len, RbArray? rpl = null)
        {
            var rplValue = rpl?.Value ?? this.RbState.RbUndef;
            var valuePtr = mrb_ary_splice(this.RbState.NativeHandler, this.Value.NativeValue, head, len, rplValue.NativeValue);
            return FromArrayObject(new RbValue(this.RbState, valuePtr));
        }

        public RbValue Shift()
        {
            var valuePtr = mrb_ary_shift(this.RbState.NativeHandler, this.Value.NativeValue);
            return new RbValue(this.RbState, valuePtr);
        }

        public void Clear() => mrb_ary_clear(this.RbState.NativeHandler, this.Value.NativeValue);

        public RbValue Join(RbValue sep)
        {
            // todo: check if sep is a string
            var valuePtr = mrb_ary_join(this.RbState.NativeHandler, this.Value.NativeValue, sep.NativeValue);
            return new RbValue(this.RbState, valuePtr);
        }

        public void Resize(int newLen) => mrb_ary_resize(this.RbState.NativeHandler, this.Value.NativeValue, newLen);
        
        public RbValue this[int index]
        {
            get => this.Get(index);
            set => this.Set(index, value);
        }
    }
}