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

        public UInt64 GetInternSymbol(string name) => RbHelper.GetInternSymbol(this, name);

        public RbValue BoxFloat(double value)
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

        public RbValue NewRubyString(string str)
        {
            var result = RbHelper.NewRubyString(this, str);
            return result;
        }
        
        
        public RbClass DefineClassUnder(RbClass outer, string name, RbClass? super)
        {
            var classPtr = mrb_define_class_under(this.NativeHandler, outer.NativeHandler, name, super?.NativeHandler?? IntPtr.Zero);
            return new RbClass(classPtr, this);
        }

        public RbClass DefineModuleUnder(RbClass outer, string name)
        {
            var modulePtr = mrb_define_module_under(this.NativeHandler, outer.NativeHandler, name);
            return new RbClass(modulePtr, this);
        }
        
        public RbValue PtrToRbValue(IntPtr p) => RbHelper.PtrToRbValue(this, p);
        
        public RbClass GetRbClass(RbValue value) => RbHelper.GetRbClassFromValue(this, value);

        public string? GetSymbolName(UInt64 sym) => RbHelper.GetSymbolName(this, sym);
        
        public RbValue GetSymbolStr(UInt64 sym) => RbHelper.GetSymbolStr(this, sym);
        
        public string? GetSymbolDump(UInt64 sym) => RbHelper.GetSymbolDump(this, sym);

        public RbClass NewClass(RbClass? super)
        {
            var classPtr = mrb_class_new(this.NativeHandler, super?.NativeHandler ?? IntPtr.Zero);
            return new RbClass(classPtr, this);
        }

        public RbClass NewModule()
        {
            var modulePtr = mrb_module_new(this.NativeHandler);
            return new RbClass(modulePtr, this);
        }
        
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
            var classPtr = mrb_exc_get_id(this.NativeHandler, this.GetInternSymbol(name));
            return new RbClass(classPtr, this);
        }

        public RbValue GetTopSelf() => new RbValue(this, mrb_top_self(this.NativeHandler));

        public Boolean ClassDefinedUnder(RbClass outer, string name) => mrb_class_defined_under(this.NativeHandler, outer.NativeHandler, name);

        public RbClass GetClassUnder(RbClass outer, string name)
        {
            var classPtr = mrb_class_get_under(this.NativeHandler, outer.NativeHandler, name);
            return new RbClass(classPtr, this);
        }

        public RbClass GetModuleUnder(RbClass outer, string name)
        {
            var classPtr = mrb_module_get_under(this.NativeHandler, outer.NativeHandler, name);
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
        
        public RbValue GetGlobalConst(string name)
        {
            var objClass = this.GetClass("Object");
            return RbHelper.GetConst(this, objClass.ClassObject, name);
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
        
        // public Boolean IsInstanceVariableNameSymP(UInt64 sym) => mrb_iv_name_sym_p(this.NativeHandler, sym);
        //
        // public void IsInstanceVariableNameSymCheck(UInt64 sym) => mrb_iv_name_sym_check(this.NativeHandler, sym);

        public RbValue GetGlobalVariable(UInt64 sym)
        {
            var result = mrb_gv_get(this.NativeHandler, sym);
            return new RbValue(this, result);
        }

        public RbValue GetGlobalVariable(string name)
        {
            var sym = this.GetInternSymbol(name);
            return this.GetGlobalVariable(sym);
        }

        public void SetGlobalVariable(UInt64 sym, RbValue val) => mrb_gv_set(this.NativeHandler, sym, val.NativeValue);

        public void SetGlobalVariable(string name, RbValue val)
        {
            var sym = this.GetInternSymbol(name);
            this.SetGlobalVariable(sym, val);
        }

        public void RemoveGlobalVariable(string name)
        {
            var sym = this.GetInternSymbol(name);
            this.RemoveGlobalVariable(sym);
        }
        
        public void RemoveGlobalVariable(UInt64 sym) => mrb_gv_remove(this.NativeHandler, sym);
    }
}