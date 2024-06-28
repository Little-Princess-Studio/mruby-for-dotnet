namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial struct RbState
    {
        // MRB_API struct RClass *mrb_define_class(mrb_state *mrb, const char *name, struct RClass *super);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_class(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            IntPtr @class);

        // MRB_API struct RClass *mrb_define_module(mrb_state *mrb, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_module(
            IntPtr state,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API mrb_bool mrb_class_defined(mrb_state *mrb, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool mrb_class_defined(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass* mrb_class_get(mrb_state *mrb, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_class_get(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass* mrb_exc_get_id(mrb_state *mrb, mrb_sym name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_exc_get_id(
            IntPtr mrb,
            UInt64 sym);

        // MRB_API mrb_bool mrb_class_defined_under(mrb_state *mrb, struct RClass *outer, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool mrb_class_defined_under(
            IntPtr mrb,
            IntPtr outer,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass * mrb_class_get_under(mrb_state *mrb, struct RClass *outer, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_class_get_under(
            IntPtr mrb,
            IntPtr outer,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass * mrb_module_get(mrb_state *mrb, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_module_get(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API void mrb_notimplement(mrb_state*);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_notimplement(IntPtr mrb);

        // MRB_API mrb_value mrb_notimplement_m(mrb_state*, mrb_value);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_notimplement_m(IntPtr mrb, UInt64 value);

        // MRB_API mrb_value mrb_obj_itself(mrb_state*, mrb_value);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_itself(IntPtr mrb, UInt64 value);

        // MRB_API mrb_value mrb_top_self(mrb_state *mrb);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_top_self(IntPtr mrb);

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

        // MRB_API void mrb_p(mrb_state*, mrb_value);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_p(IntPtr mrb, UInt64 value);

        // MRB_API void mrb_full_gc(mrb_state*);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_full_gc(IntPtr mrb);

        // MRB_API void mrb_incremental_gc(mrb_state*);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_incremental_gc(IntPtr mrb);

        // MRB_API void mrb_gc_protect(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_gc_protect(IntPtr mrb, UInt64 obj);

        // MRB_API void mrb_gc_register(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_gc_register(IntPtr mrb, UInt64 obj);

        // MRB_API void mrb_gc_unregister(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_gc_unregister(IntPtr mrb, UInt64 obj);

        // MRB_API void mrb_define_global_const(mrb_state *mrb, const char *name, mrb_value val);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_define_global_const(IntPtr mrb, [MarshalAs(UnmanagedType.LPStr)] string name, UInt64 val);

        // MRB_API mrb_value mrb_fiber_new(mrb_state *mrb, const struct RProc *proc);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_fiber_new(IntPtr mrb, IntPtr proc);

        // MRB_API mrb_value mrb_fiber_resume(mrb_state *mrb, mrb_value fib, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_fiber_resume(IntPtr mrb, UInt64 fib, int argc, UInt64[] argv);

        // MRB_API mrb_value mrb_fiber_yield(mrb_state *mrb, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_fiber_yield(IntPtr mrb, int argc, UInt64[] argv);

        // MRB_API mrb_value mrb_fiber_alive_p(mrb_state *mrb, mrb_value fib);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_fiber_alive_p(IntPtr mrb, UInt64 fib);

        // MRB_API mrb_value mrb_yield(mrb_state *mrb, mrb_value b, mrb_value arg);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_yield(IntPtr mrb, UInt64 b, UInt64 arg);

        // MRB_API mrb_value mrb_yield_argv(mrb_state *mrb, mrb_value b, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_yield_argv(IntPtr mrb, UInt64 b, int argc, UInt64[] argv);

        // MRB_API mrb_value mrb_yield_with_class(mrb_state *mrb, mrb_value b, mrb_int argc, const mrb_value *argv, mrb_value self, struct RClass *c);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_yield_with_class(IntPtr mrb, UInt64 b, int argc, UInt64[] argv, UInt64 self, IntPtr c);
        
        // mrb_value mrb_yield_cont(mrb_state *mrb, mrb_value b, mrb_value self, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_yield_cont(IntPtr mrb, UInt64 b, UInt64 self, int argc, UInt64[] argv);
    }
}