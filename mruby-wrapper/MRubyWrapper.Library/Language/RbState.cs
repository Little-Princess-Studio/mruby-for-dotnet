namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    public partial class RbState
    {
        public IntPtr NativeHandler { get; set; } = IntPtr.Zero;

        public RbValue RbTrue { get; private set; }
        public RbValue RbFalse { get; private set; }
        public RbValue RbNil { get; private set; }
        public RbValue RbUndef { get; private set; }

        public RbState()
        {
            this.RbTrue = new RbValue(this, mrb_true_value_boxing());
            this.RbFalse = new RbValue(this, mrb_false_value_boxing());
            this.RbNil = new RbValue(this, mrb_nil_value_boxing());
            this.RbUndef = new RbValue(this, mrb_undef_value_boxing());
        }

        public RbValue BoxFloat(float value)
        {
            var result = mrb_float_value_boxing(this.NativeHandler, value);
            return new RbValue(this, result);
        }

        public RbValue BoxInt(Int64 value)
        {
            var result = mrb_int_value_boxing(value);
            return new RbValue(this, result);
        }

        public RbValue BoxSymbol(UInt64 value)
        {
            var result = mrb_symbol_value_boxing(value);
            return new RbValue(this, result);
        }

        public RbValue BoxString(string str)
        {
            var result = RbHelper.NewRubyString(this, str);
            return result;
        }

        public string? GetSymbolName(UInt64 sym) => RbHelper.GetSymbolName(this, sym);
        
        public RbValue GetSymbolStr(UInt64 sym) => RbHelper.GetSymbolStr(this, sym);

        public RbClass DefineClass(string name, RbClass? @class)
        {
            var classPtr = mrb_define_class(this.NativeHandler, name, @class?.NativeHandler ?? IntPtr.Zero);
            return new RbClass(classPtr, this);
        }

        public RbClass DefineModule(string name)
        {
            var modulePtr = mrb_define_module(this.NativeHandler, name);
            return new RbClass(modulePtr, this);
        }

        public bool ClassDefined(string name) => mrb_class_defined(this.NativeHandler, name);

        public RbClass GetClass(string name)
        {
            var classPtr = mrb_class_get(this.NativeHandler, name);
            return new RbClass(classPtr, this);
        }

        public RbClass GetExceptionClass(string name)
        {
            var classPtr = mrb_exc_get_id(this.NativeHandler, RbHelper.GetInternSymbol(this, name));
            return new RbClass(classPtr, this);
        }

        public RbValue GetTopSelf() => new RbValue(this, mrb_top_self(this.NativeHandler));

        public bool ClassDefinedUnder(RbClass outer, string name) => mrb_class_defined_under(this.NativeHandler, outer.NativeHandler, name);

        public RbClass GetClassUnder(RbClass outer, string name)
        {
            var classPtr = mrb_class_get_under(this.NativeHandler, outer.NativeHandler, name);
            return new RbClass(classPtr, this);
        }

        public RbClass GetModule(string name)
        {
            var modulePtr = mrb_module_get(this.NativeHandler, name);
            return new RbClass(modulePtr, this);
        }

        public void NotImplement() => mrb_notimplement(this.NativeHandler);

        public RbValue NotImplementM(RbValue value)
        {
            var result = mrb_notimplement_m(this.NativeHandler, value.NativeValue);
            return new RbValue(this, result);
        }

        public RbValue ObjItself(RbValue value)
        {
            var result = mrb_obj_itself(this.NativeHandler, value.NativeValue);
            return new RbValue(this, result);
        }

        public void FullGc() => mrb_full_gc(this.NativeHandler);

        public void IncrementalGc() => mrb_incremental_gc(this.NativeHandler);

        public void GcProtect(RbValue value)
        {
            mrb_gc_protect(this.NativeHandler, value.NativeValue);
        }

        public void GcRegister(RbValue value)
        {
            mrb_gc_register(this.NativeHandler, value.NativeValue);
        }

        public void GcUnregister(RbValue value)
        {
            mrb_gc_unregister(this.NativeHandler, value.NativeValue);
        }

        public void DefineGlobalConst(string name, RbValue value)
        {
            mrb_define_global_const(this.NativeHandler, name, value.NativeValue);
        }

        public RbValue FiberNew(RbProc proc)
        {
            var result = mrb_fiber_new(this.NativeHandler, proc.NativeHandler);
            return new RbValue(this, result);
        }

        public RbValue FiberResume(RbValue fib, params RbValue[] argv)
        {
            var result = mrb_fiber_resume(this.NativeHandler, fib.NativeValue, argv.Length, argv.Select(a => a.NativeValue).ToArray());
            return new RbValue(this, result);
        }

        public RbValue FiberYield(params RbValue[] argv)
        {
            var result = mrb_fiber_yield(this.NativeHandler, argv.Length, argv.Select(a => a.NativeValue).ToArray());
            return new RbValue(this, result);
        }

        public RbValue FiberAliveP(RbValue fib)
        {
            var result = mrb_fiber_alive_p(this.NativeHandler, fib.NativeValue);
            return new RbValue(this, result);
        }

        public RbValue Yield(RbValue b, RbValue arg)
        {
            var result = mrb_yield(this.NativeHandler, b.NativeValue, arg.NativeValue);
            return new RbValue(this, result);
        }

        public RbValue YieldArgv(RbValue b, params RbValue[] argv)
        {
            var result = mrb_yield_argv(this.NativeHandler, b.NativeValue, argv.Length, argv.Select(a => a.NativeValue).ToArray());
            return new RbValue(this, result);
        }
        
        public RbValue P(RbValue value)
        {
            mrb_p(this.NativeHandler, value.NativeValue);
            return value;
        }
        
        public RbValue YieldWithClass(RbValue b, RbValue c, RbValue[] args, RbValue self, RbClass @class)
        {
            var result = mrb_yield_with_class(
                this.NativeHandler,
                b.NativeValue,
                args.Length,
                args.Select(a => a.NativeValue).ToArray(),
                self.NativeValue, @class.NativeHandler);
            return new RbValue(this, result);
        }
        
        public UInt64 YieldCont(RbValue b, RbValue self, params RbValue[] args)
        {
            return mrb_yield_cont(this.NativeHandler, b.NativeValue, self.NativeValue, args.Length, args.Select(a => a.NativeValue).ToArray());
        }
        
        public Int64 UnboxInt(RbValue value) => mrb_int_value_unboxing(value.NativeValue);
        
        public double UnboxFloat(RbValue value) => mrb_float_value_unboxing(value.NativeValue);
        
        public UInt64 UnboxSymbol(RbValue value) => mrb_symbol_value_unboxing(value.NativeValue);
        
        public string? UnboxString(RbValue value) => Marshal.PtrToStringAnsi(mrb_string_value_unboxing(this.NativeHandler, value.NativeValue));
        
        public RbValue ConstGet(UInt64 mod, UInt64 sym)
        {
            var result = mrb_const_get(this.NativeHandler, mod, sym);
            return new RbValue(this, result);
        }

        public void ConstSet(UInt64 mod, UInt64 sym, RbValue val)
            => mrb_const_set(this.NativeHandler, mod, sym, val.NativeValue);

        public bool ConstDefined(UInt64 mod, UInt64 sym) => mrb_const_defined(this.NativeHandler, mod, sym);

        public void ConstRemove(UInt64 mod, UInt64 sym) => mrb_const_remove(this.NativeHandler, mod, sym);

        public bool IvNameSymP(UInt64 sym) => mrb_iv_name_sym_p(this.NativeHandler, sym);

        public void IvNameSymCheck(UInt64 sym) => mrb_iv_name_sym_check(this.NativeHandler, sym);

        public RbValue GvGet(UInt64 sym)
        {
            var result = mrb_gv_get(this.NativeHandler, sym);
            return new RbValue(this, result);
        }

        public void GvSet(UInt64 sym, RbValue val) => mrb_gv_set(this.NativeHandler, sym, val.NativeValue);

        public void GvRemove(UInt64 sym) => mrb_gv_remove(this.NativeHandler, sym);
    }
}