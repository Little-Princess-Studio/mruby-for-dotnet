namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial struct RbClass
    { 
        // MRB_API void mrb_define_method(mrb_state *mrb, struct RClass *cla, const char *name, mrb_func_t func, mrb_aspec aspec);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_method(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodSignature nativeMethod,
            uint parameterAspect);

        // MRB_API void mrb_define_class_method(mrb_state *mrb, struct RClass *cla, const char *name, mrb_func_t fun, mrb_aspec aspec);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_class_method(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodSignature nativeMethod,
            uint parameterAspect);

        // MRB_API void mrb_define_singleton_method(mrb_state *mrb, struct RObject *cla, const char *name, mrb_func_t fun, mrb_aspec aspec);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_singleton_method(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodSignature nativeMethod,
            uint parameterAspect);

        // MRB_API void mrb_define_module_function(mrb_state *mrb, struct RClass *cla, const char *name, mrb_func_t fun, mrb_aspec aspec);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_module_function(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodSignature nativeMethod,
            uint parameterAspect);

        // MRB_API void mrb_define_const(mrb_state* mrb, struct RClass* cla, const char *name, mrb_value val);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_const(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            UInt64 value);

        // MRB_API mrb_value mrb_obj_new(mrb_state *mrb, struct RClass *c, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_obj_new(IntPtr state, IntPtr @class, int argc, IntPtr argv);

        // MRB_API void mrb_include_module(mrb_state *mrb, struct RClass *cla, struct RClass *included);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_include_module(IntPtr mrb, IntPtr cla, IntPtr included);

        // MRB_API void mrb_prepend_module(mrb_state *mrb, struct RClass *cla, struct RClass *prepended);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_prepend_module(IntPtr mrb, IntPtr cla, IntPtr prepended);

        // MRB_API void mrb_undef_method(mrb_state *mrb, struct RClass *cla, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_undef_method(IntPtr mrb, IntPtr cla, [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API void mrb_undef_class_method(mrb_state *mrb, struct RClass *cls, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_undef_class_method(IntPtr mrb, IntPtr cla, [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass * mrb_class_new(mrb_state *mrb, struct RClass *super);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_class_new(IntPtr mrb, IntPtr super);

        // MRB_API struct RClass * mrb_module_new(mrb_state *mrb);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_module_new(IntPtr mrb);

        // MRB_API mrb_bool mrb_obj_respond_to(mrb_state *mrb, struct RClass* c, mrb_sym mid);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool mrb_obj_respond_to(IntPtr mrb, IntPtr c, UInt64 mid);

        // MRB_API struct RClass* mrb_define_class_under(mrb_state *mrb, struct RClass *outer, const char *name, struct RClass *super);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_class_under(
            IntPtr mrb, IntPtr outer, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr super);

        // MRB_API struct RClass* mrb_define_module_under(mrb_state *mrb, struct RClass *outer, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_module_under(
            IntPtr mrb, IntPtr outer, [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API mrb_int mrb_get_argc(mrb_state *mrb);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern Int64 mrb_get_argc(IntPtr mrb);

        // MRB_API const mrb_value *mrb_get_argv(mrb_state *mrb);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_get_argv(IntPtr mrb);
        
        // MRB_API void mrb_define_alias(mrb_state *mrb, struct RClass *c, const char *a, const char *b);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_alias(
            IntPtr mrb,
            IntPtr @class,
            [MarshalAs(UnmanagedType.LPStr)] string a,
            [MarshalAs(UnmanagedType.LPStr)] string b);

        // MRB_API void mrb_define_alias_id(mrb_state *mrb, struct RClass *c, mrb_sym a, mrb_sym b);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern void mrb_define_alias_id(IntPtr mrb, IntPtr @class, UInt64 a, UInt64 b);

        // MRB_API const char *mrb_class_name(mrb_state *mrb, struct RClass* klass);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_class_name(IntPtr mrb, IntPtr klass);
    }
}