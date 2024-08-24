namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbState
    {
        // MRB_API struct RClass * mrb_class_new(mrb_state *mrb, struct RClass *super);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_class_new(IntPtr mrb, IntPtr super);

        // MRB_API struct RClass * mrb_module_new(mrb_state *mrb);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_module_new(IntPtr mrb);

        // MRB_API struct RClass *mrb_define_class(mrb_state *mrb, const char *name, struct RClass *super);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_class(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            IntPtr @class);

        // MRB_API struct RClass *mrb_define_module(mrb_state *mrb, const char *name);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_module(
            IntPtr state,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API mrb_bool mrb_class_defined(mrb_state *mrb, const char *name);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_class_defined(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass* mrb_class_get(mrb_state *mrb, const char *name);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_class_get(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass* mrb_exc_get_id(mrb_state *mrb, mrb_sym name);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_exc_get_id(
            IntPtr mrb,
            UInt64 sym);

        // MRB_API mrb_bool mrb_class_defined_under(mrb_state *mrb, struct RClass *outer, const char *name);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_class_defined_under(
            IntPtr mrb,
            IntPtr outer,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass * mrb_class_get_under(mrb_state *mrb, struct RClass *outer, const char *name);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_class_get_under(
            IntPtr mrb,
            IntPtr outer,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass * mrb_module_get_under(mrb_state *mrb, struct RClass *outer, const char *name);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_module_get_under(
            IntPtr mrb,
            IntPtr outer,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass * mrb_module_get(mrb_state *mrb, const char *name);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_module_get(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API void mrb_notimplement(mrb_state*);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_notimplement(IntPtr mrb);

        // MRB_API mrb_value mrb_notimplement_m(mrb_state*, mrb_value);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_notimplement_m(IntPtr mrb, UInt64 value);

        // // MRB_API mrb_value mrb_obj_itself(mrb_state*, mrb_value);
        // [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        // private static extern UInt64 mrb_obj_itself(IntPtr mrb, UInt64 value);

        // MRB_API mrb_value mrb_top_self(mrb_state *mrb);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_top_self(IntPtr mrb);

        // MRB_API mrb_value mrb_float_value_boxing(struct mrb_state *mrb, mrb_float f);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_float_value_boxing(IntPtr mrb, double f);

        // MRB_API mrb_value mrb_int_value_boxing(struct mrb_state *mrb, mrb_int i);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_int_value_boxing(Int64 i);

        // MRB_API mrb_value mrb_symbol_value_boxing(mrb_sym i);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_symbol_value_boxing(UInt64 i);

        // MRB_API mrb_value mrb_nil_value_boxing();
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_nil_value_boxing();

        // MRB_API mrb_value mrb_true_value_boxing();
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_true_value_boxing();

        // MRB_API mrb_value mrb_false_value_boxing();
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_false_value_boxing();

        // MRB_API mrb_value mrb_undef_value_boxing();
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_undef_value_boxing();

        // MRB_API void mrb_p(mrb_state*, mrb_value);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_p(IntPtr mrb, UInt64 value);

        // MRB_API void mrb_full_gc(mrb_state*);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_full_gc(IntPtr mrb);

        // MRB_API void mrb_incremental_gc(mrb_state*);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_incremental_gc(IntPtr mrb);

        // MRB_API void mrb_gc_protect(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_gc_protect(IntPtr mrb, UInt64 obj);

        // MRB_API void mrb_gc_register(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_gc_register(IntPtr mrb, UInt64 obj);

        // MRB_API void mrb_gc_unregister(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_gc_unregister(IntPtr mrb, UInt64 obj);

        // MRB_API void mrb_define_global_const(mrb_state *mrb, const char *name, mrb_value val);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_define_global_const(IntPtr mrb, [MarshalAs(UnmanagedType.LPStr)] string name, UInt64 val);

        // MRB_API mrb_value mrb_fiber_new(mrb_state *mrb, const struct RProc *proc);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_fiber_new(IntPtr mrb, IntPtr proc);

        // MRB_API mrb_value mrb_fiber_resume(mrb_state *mrb, mrb_value fib, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_fiber_resume(IntPtr mrb, UInt64 fib, int argc, UInt64[] argv);

        // MRB_API mrb_value mrb_fiber_yield(mrb_state *mrb, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_fiber_yield(IntPtr mrb, int argc, UInt64[] argv);

        // MRB_API mrb_value mrb_fiber_alive_p(mrb_state *mrb, mrb_value fib);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_fiber_alive_p(IntPtr mrb, UInt64 fib);

        // // MRB_API mrb_value mrb_yield(mrb_state *mrb, mrb_value b, mrb_value arg);
        // [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        // private static extern UInt64 mrb_yield(IntPtr mrb, UInt64 b, UInt64 arg);

        // // MRB_API mrb_value mrb_yield_argv(mrb_state *mrb, mrb_value b, mrb_int argc, const mrb_value *argv);
        // [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        // private static extern UInt64 mrb_yield_argv(IntPtr mrb, UInt64 b, int argc, UInt64[] argv);

        // MRB_API mrb_value mrb_yield_with_class(mrb_state *mrb, mrb_value b, mrb_int argc, const mrb_value *argv, mrb_value self, struct RClass *c);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_yield_with_class(IntPtr mrb, UInt64 b, int argc, UInt64[] argv, UInt64 self, IntPtr c);

        // MRB_API mrb_int mrb_int_value_unboxing(mrb_value value);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern Int64 mrb_int_value_unboxing(UInt64 value);

        // MRB_API mrb_float mrb_float_value_unboxing(mrb_value value);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern double mrb_float_value_unboxing(UInt64 value);

        // MRB_API mrb_sym mrb_symbol_value_unboxing(mrb_value value);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_symbol_value_unboxing(UInt64 value);

        // MRB_API const char* mrb_string_value_unboxing(struct mrb_state* mrb, mrb_value value);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_string_value_unboxing(IntPtr mrb, UInt64 value);

        // // MRB_API mrb_bool mrb_iv_name_sym_p(mrb_state *mrb, mrb_sym sym);
        // [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        // [return: MarshalAs(UnmanagedType.U1)]
        // private static extern Boolean mrb_iv_name_sym_p(IntPtr mrb, UInt64 sym);
        //
        // // MRB_API void mrb_iv_name_sym_check(mrb_state *mrb, mrb_sym sym);
        // [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        // private static extern void mrb_iv_name_sym_check(IntPtr mrb, UInt64 sym);

        // MRB_API mrb_value mrb_gv_get(mrb_state *mrb, mrb_sym sym);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_gv_get(IntPtr mrb, UInt64 sym);

        // MRB_API void mrb_gv_set(mrb_state *mrb, mrb_sym sym, mrb_value val);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_gv_set(IntPtr mrb, UInt64 sym, UInt64 val);

        // MRB_API void mrb_gv_remove(mrb_state *mrb, mrb_sym sym);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_gv_remove(IntPtr mrb, UInt64 sym);

        // MRB_API struct RClass* mrb_define_class_under(mrb_state *mrb, struct RClass *outer, const char *name, struct RClass *super);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_class_under(
            IntPtr mrb, IntPtr outer, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr super);

        // MRB_API struct RClass* mrb_define_module_under(mrb_state *mrb, struct RClass *outer, const char *name);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_module_under(
            IntPtr mrb, IntPtr outer, [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RProc *mrb_proc_new_cfunc_with_env(mrb_state *mrb, mrb_func_t func, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_proc_new_cfunc_with_env(
            IntPtr mrb, [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc func, int argc, UInt64[]? argv);

        // mrb_value mrb_get_block(struct mrb_state *mrb) {
        [DllImport("mruby_x64.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_get_block(IntPtr mrb);
    }
}