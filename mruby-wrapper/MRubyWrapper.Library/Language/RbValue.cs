namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class RbValue
    {
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        internal struct RbNativeValue
        {
            public UInt64 Value;
        }

        public RbState RbState { get; private set; }
        internal RbNativeValue NativeValue { get; private set; }

        // MRB_API struct RClass *mrb_singleton_class_ptr(mrb_state *mrb, mrb_value val);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_singleton_class_ptr(IntPtr state, UInt64 val);

        // MRB_API mrb_value mrb_obj_dup(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_dup(IntPtr state, UInt64 obj);

        // MRB_API mrb_value mrb_float_value_boxing(struct mrb_state *mrb, mrb_float f);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_float_value_boxing(IntPtr mrb, double f);

        // MRB_API mrb_value mrb_int_value_boxing(struct mrb_state *mrb, mrb_int i);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_int_value_boxing(IntPtr mrb, int i);

        // MRB_API mrb_value mrb_symbol_value_boxing(mrb_sym i);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_symbol_value_boxing(UInt64 i);

        // MRB_API mrb_value mrb_nil_value_boxing();
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_nil_value_boxing();

        // MRB_API mrb_value mrb_true_value_boxing();
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_true_value_boxing();

        // MRB_API mrb_value mrb_false_value_boxing();
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_false_value_boxing();

        // MRB_API mrb_value mrb_undef_value_boxing();
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_undef_value_boxing();

        public static RbValue RbTrue { get; private set; } = null!;
        public static RbValue RbFalse { get; private set; } = null!;
        public static RbValue RbNil { get; private set; } = null!;
        public static RbValue RbUndef { get; private set; } = null!;

        public static void Init(RbState state)
        {
            RbTrue = new RbValue(state, mrb_true_value_boxing());
            RbFalse = new RbValue(state, mrb_false_value_boxing());
            RbNil = new RbValue(state, mrb_nil_value_boxing());
            RbUndef = new RbValue(state, mrb_undef_value_boxing());
        }

        public RbValue(RbState rbState, UInt64 nativeValue)
        {
            this.RbState = rbState;
            this.NativeValue = new RbNativeValue
            {
                Value = nativeValue,
            };
        }

        public RbValue CallMethod(string name, params RbValue[] args)
            => RbHelper.CallMethod(this.RbState, this, name, args);

        public RbClass SingletonClass()
        {
            var classPtr = mrb_singleton_class_ptr(this.RbState.MrbState, this.NativeValue.Value);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this.RbState,
            };
        }
        
        // Wrapper for mrb_obj_dup
        public RbValue Duplicate()
        {
            var result = mrb_obj_dup(this.RbState.MrbState, this.NativeValue.Value);
            return new RbValue(this.RbState, result);
        }

        // Wrapper for mrb_float_value_boxing
        public RbValue BoxFloat(float value)
        {
            var result = mrb_float_value_boxing(this.RbState.MrbState, value);
            return new RbValue(this.RbState, result);
        }

        // Wrapper for mrb_int_value_boxing
        public RbValue BoxInt(int value)
        {
            var result = mrb_int_value_boxing(this.RbState.MrbState, value);
            return new RbValue(this.RbState, result);
        }

        // Wrapper for mrb_symbol_value_boxing
        public RbValue BoxSymbol(UInt64 value)
        {
            var result = mrb_symbol_value_boxing(value);
            return new RbValue(this.RbState, result);
        }
    }
}