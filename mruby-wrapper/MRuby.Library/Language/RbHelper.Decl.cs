namespace MRuby.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbHelper
    {
        // MRB_API mrb_value mrb_funcall_argv(mrb_state *mrb, mrb_value val, mrb_sym name, mrb_int argc, const mrb_value *argv);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_funcall_argv(
            IntPtr state,
            UInt64 val,
            UInt64 sym,
            Int64 argc,
            UInt64[] argv);

        // MRB_API mrb_value mrb_funcall_with_block(mrb_state *mrb, mrb_value val, mrb_sym name, mrb_int argc, const mrb_value *argv, mrb_value block);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_funcall_with_block(
            IntPtr state,
            UInt64 val,
            UInt64 sym,
            Int64 argc,
            UInt64[] argv,
            UInt64 block);
        
        // MRB_API mrb_sym mrb_intern_cstr(mrb_state *mrb, const char* str);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_intern_cstr(
            IntPtr state,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API mrb_bool mrb_block_given_p(mrb_state *mrb);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_block_given_p(IntPtr mrb);
        
        // MRB_API const char *mrb_sym_name(mrb_state*,mrb_sym);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_sym_name(IntPtr mrb, UInt64 sym);

        // MRB_API const char *mrb_sym_dump(mrb_state*,mrb_sym);
        // [DllImport(Ruby.MRUBY_LIB, CharSet = CharSet.Ansi)]
        // private static extern IntPtr mrb_sym_dump(IntPtr mrb, UInt64 sym);
        
        // MRB_API mrb_value mrb_sym_str(mrb_state*,mrb_sym);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_sym_str(IntPtr mrb, UInt64 sym);

        // MRB_API mrb_value mrb_str_new_cstr(mrb_state*, const char*);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_str_new_cstr(IntPtr mrb, [MarshalAs(UnmanagedType.LPUTF8Str)] string str);
        
        // MRB_API mrb_value mrb_ptr_to_mrb_value(void *p);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_ptr_to_mrb_value(IntPtr p);
        
        // MRB_API RClass *mrb_get_class_ptr(mrb_value value);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_get_class_ptr(UInt64 value);
        
        // MRB_API RObject* mrb_value_to_obj_ptr(mrb_value value);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_value_to_obj_ptr(UInt64 value);
        
        // MRB_API mrb_int mrb_get_argc(mrb_state *mrb);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern Int64 mrb_get_argc(IntPtr mrb);

        // MRB_API const mrb_value *mrb_get_argv(mrb_state *mrb);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_get_argv(IntPtr mrb);
        
        // MRB_API mrb_value mrb_const_get(mrb_state*, mrb_value, mrb_sym);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_const_get(IntPtr mrb, UInt64 mod, UInt64 sym);

        // MRB_API void mrb_const_set(mrb_state*, mrb_value, mrb_sym, mrb_value);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_const_set(IntPtr mrb, UInt64 mod, UInt64 sym, UInt64 val);
        
        // MRB_API mrb_int mrb_get_args_a(mrb_state *mrb, mrb_args_format format, void** ptr);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern Int64 mrb_get_args_a(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string format,
            ref IntPtr[] ptr);
        
        // MRB_API mrb_bool mrb_check_type_integer(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_check_type_integer(UInt64 obj);
        
        // MRB_API mrb_bool mrb_check_type_symbol(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_symbol(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_float(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_float(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_array(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_array(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_string(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_string(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_hash(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_hash(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_exception(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_exception(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_object(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_object(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_class(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_class(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_moudle(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_moudle(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_sclass(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_sclass(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_proc(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_proc(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_range(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_range(UInt64 obj);

        // MRB_API mrb_bool mrb_check_type_fiber(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_check_type_fiber(UInt64 obj);
        
        // MRB_API void mrb_get_raw_bytes_from_string(mrb_value value, const char **bytes, size_t *len);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrb_get_raw_bytes_from_string(UInt64 value, ref IntPtr bytes, ref UInt64 len);
        
        // MRB_API mrb_value mrb_str_new(mrb_state *mrb, const char *p, mrb_int len);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_str_new(IntPtr mrb, byte[] p, Int64 len);
    }
}