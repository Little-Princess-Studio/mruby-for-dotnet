namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Linq;

    public partial struct RbState
    {
        public IntPtr MrbState { get; set; } = 0;

        public RbValue RbTrue { get; private set; } = null!;
        public RbValue RbFalse { get; private set; } = null!;
        public RbValue RbNil { get; private set; } = null!;
        public RbValue RbUndef { get; private set; } = null!;

        public RbState()
        {
            this.Init();
        }

        private void Init()
        {
            RbTrue = new RbValue(this, mrb_true_value_boxing());
            RbFalse = new RbValue(this, mrb_false_value_boxing());
            RbNil = new RbValue(this, mrb_nil_value_boxing());
            RbUndef = new RbValue(this, mrb_undef_value_boxing());
        }

        // Wrapper for mrb_float_value_boxing
        public RbValue BoxFloat(float value)
        {
            var result = mrb_float_value_boxing(this.MrbState, value);
            return new RbValue(this, result);
        }

        // Wrapper for mrb_int_value_boxing
        public RbValue BoxInt(int value)
        {
            var result = mrb_int_value_boxing(this.MrbState, value);
            return new RbValue(this, result);
        }

        // Wrapper for mrb_symbol_value_boxing
        public RbValue BoxSymbol(UInt64 value)
        {
            var result = mrb_symbol_value_boxing(value);
            return new RbValue(this, result);
        }

        public RbClass DefineClass(string name, RbClass? @class)
        {
            var classPtr = mrb_define_class(this.MrbState, name, @class?.NativeHandler ?? IntPtr.Zero);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this,
            };
        }

        public RbClass DefineModule(string name)
        {
            var modulePtr = mrb_define_module(this.MrbState, name);
            return new RbClass
            {
                NativeHandler = modulePtr,
                RbState = this,
            };
        }

        public bool ClassDefined(string name) => mrb_class_defined(this.MrbState, name);

        public RbClass GetClass(string name)
        {
            var classPtr = mrb_class_get(this.MrbState, name);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this,
            };
        }

        public RbClass GetExceptionClass(string name)
        {
            var classPtr = mrb_exc_get_id(this.MrbState, RbHelper.GetInternSymbol(this, name));
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this,
            };
        }

        public RbValue GetTopSelf() => new RbValue(this, mrb_top_self(this.MrbState));

        public bool ClassDefinedUnder(RbClass outer, string name) => mrb_class_defined_under(this.MrbState, outer.NativeHandler, name);

        public RbClass GetClassUnder(RbClass outer, string name)
        {
            var classPtr = mrb_class_get_under(this.MrbState, outer.NativeHandler, name);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this,
            };
        }

        public RbClass GetModule(string name)
        {
            var modulePtr = mrb_module_get(this.MrbState, name);
            return new RbClass
            {
                NativeHandler = modulePtr,
                RbState = this,
            };
        }

        public void NotImplement() => mrb_notimplement(this.MrbState);

        public RbValue NotImplementM(RbValue value)
        {
            var result = mrb_notimplement_m(this.MrbState, value.NativeValue.Value);
            return new RbValue(this, result);
        }

        public RbValue ObjItself(RbValue value)
        {
            var result = mrb_obj_itself(this.MrbState, value.NativeValue.Value);
            return new RbValue(this, result);
        }

        public void FullGc() => mrb_full_gc(this.MrbState);

        public void IncrementalGc() => mrb_incremental_gc(this.MrbState);

        public void GcProtect(RbValue value)
        {
            mrb_gc_protect(this.MrbState, value.NativeValue.Value);
        }

        public void GcRegister(RbValue value)
        {
            mrb_gc_register(this.MrbState, value.NativeValue.Value);
        }

        public void GcUnregister(RbValue value)
        {
            mrb_gc_unregister(this.MrbState, value.NativeValue.Value);
        }

        public void DefineGlobalConst(string name, RbValue value)
        {
            mrb_define_global_const(this.MrbState, name, value.NativeValue.Value);
        }

        public RbValue FiberNew(RbProc proc)
        {
            var result = mrb_fiber_new(this.MrbState, proc.NativeHandler);
            return new RbValue(this, result);
        }

        public RbValue FiberResume(RbValue fib, params RbValue[] argv)
        {
            var result = mrb_fiber_resume(this.MrbState, fib.NativeValue.Value, argv.Length, argv.Select(a => a.NativeValue.Value).ToArray());
            return new RbValue(this, result);
        }

        public RbValue FiberYield(params RbValue[] argv)
        {
            var result = mrb_fiber_yield(this.MrbState, argv.Length, argv.Select(a => a.NativeValue.Value).ToArray());
            return new RbValue(this, result);
        }

        public RbValue FiberAliveP(RbValue fib)
        {
            var result = mrb_fiber_alive_p(this.MrbState, fib.NativeValue.Value);
            return new RbValue(this, result);
        }

        public RbValue Yield(RbValue b, RbValue arg)
        {
            var result = mrb_yield(this.MrbState, b.NativeValue.Value, arg.NativeValue.Value);
            return new RbValue(this, result);
        }

        public RbValue YieldArgv(RbValue b, params RbValue[] argv)
        {
            var result = mrb_yield_argv(this.MrbState, b.NativeValue.Value, argv.Length, argv.Select(a => a.NativeValue.Value).ToArray());
            return new RbValue(this, result);
        }
    }
}