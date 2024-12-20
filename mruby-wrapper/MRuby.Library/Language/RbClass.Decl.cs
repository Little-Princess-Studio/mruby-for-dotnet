namespace MRuby.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial struct RbClass
    { 
        // MRB_API void mrb_define_method_id(mrb_state *mrb, struct RClass *cla, mrb_sym name, mrb_func_t func, mrb_aspec aspec);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrb_define_method_id(
            IntPtr mrb,
            IntPtr @class,
            UInt64 name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc nativeMethod,
            uint parameterAspect);

        // MRB_API void mrb_define_class_method_id(mrb_state *mrb, struct RClass *cla, mrb_sym name, mrb_func_t fun, mrb_aspec aspec);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrb_define_class_method_id(
            IntPtr mrb,
            IntPtr @class,
            UInt64 name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc nativeMethod,
            uint parameterAspect);
        
        // MRB_API void mrb_define_module_function_id(mrb_state *mrb, struct RClass *cla, mrb_sym name, mrb_func_t fun, mrb_aspec aspec);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrb_define_module_function_id(
            IntPtr mrb,
            IntPtr @class,
            UInt64 name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc nativeMethod,
            uint parameterAspect);

        // MRB_API void mrb_define_const_id(mrb_state* mrb, struct RClass* cla, mrb_sym name, mrb_value val);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrb_define_const_id(
            IntPtr mrb,
            IntPtr @class,
            UInt64 name,
            UInt64 value);

        // MRB_API mrb_value mrb_obj_new(mrb_state *mrb, struct RClass *c, mrb_int argc, const mrb_value *argv);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_obj_new(IntPtr state, IntPtr @class, int argc, IntPtr argv);

        // MRB_API void mrb_include_module(mrb_state *mrb, struct RClass *cla, struct RClass *included);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_include_module(IntPtr mrb, IntPtr cla, IntPtr included);

        // MRB_API void mrb_prepend_module(mrb_state *mrb, struct RClass *cla, struct RClass *prepended);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_prepend_module(IntPtr mrb, IntPtr cla, IntPtr prepended);

        // MRB_API void mrb_undef_method_id(mrb_state *mrb, struct RClass *cla, mrb_sym *name);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_undef_method_id(IntPtr mrb, IntPtr cla, UInt64 name);

        // MRB_API void mrb_undef_class_method_id(mrb_state *mrb, struct RClass *cls, const char *name);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_undef_class_method_id(IntPtr mrb, IntPtr cla, UInt64 name);
        
        // MRB_API mrb_bool mrb_obj_respond_to(mrb_state *mrb, struct RClass* c, mrb_sym mid);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_obj_respond_to(IntPtr mrb, IntPtr c, UInt64 mid);
        
        // MRB_API void mrb_define_alias_id(mrb_state *mrb, struct RClass *c, mrb_sym a, mrb_sym b);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrb_define_alias_id(IntPtr mrb, IntPtr @class, UInt64 a, UInt64 b);

        // MRB_API const char *mrb_class_name(mrb_state *mrb, struct RClass* klass);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_class_name(IntPtr mrb, IntPtr klass);

        // MRB_API mrb_value mrb_cv_get(mrb_state *mrb, mrb_value mod, mrb_sym sym);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_cv_get(IntPtr mrb, UInt64 mod, UInt64 sym);

        // MRB_API void mrb_cv_set(mrb_state *mrb, mrb_value mod, mrb_sym sym, mrb_value v);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_cv_set(IntPtr mrb, UInt64 mod, UInt64 sym, UInt64 val);
        
        // MRB_API mrb_bool mrb_cv_defined(mrb_state *mrb, mrb_value mod, mrb_sym sym);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_cv_defined(IntPtr mrb, UInt64 mod, UInt64 sym);
        
        // MRB_API mrb_value mrb_new_data_object(mrb_state *mrb, RClass *klass, void *datap, mrb_data_type *type);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_new_data_object(IntPtr mrb, IntPtr klass, IntPtr data, IntPtr type);
        
        // MRB_API mrb_bool mrb_const_defined(mrb_state*, mrb_value, mrb_sym);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_const_defined(IntPtr mrb, IntPtr mod, UInt64 sym);

        // MRB_API void mrb_const_remove(mrb_state*, mrb_value, mrb_sym);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_const_remove(IntPtr mrb, IntPtr mod, UInt64 sym);

        // MRB_API mrb_value mrb_class_path(mrb_state *mrb, struct RClass *c);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_class_path(IntPtr mrb, IntPtr @class);
    }
}