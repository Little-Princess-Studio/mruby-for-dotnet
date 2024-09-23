namespace MRuby.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbState
    {
        // MRB_API mrb_value mrb_exc_new(mrb_state *mrb, struct RClass *c, const char *ptr, mrb_int len);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_exc_new(IntPtr mrb, IntPtr c, [MarshalAs(UnmanagedType.LPStr)] string ptr, int len);

        // MRB_API mrb_noreturn void mrb_exc_raise(mrb_state *mrb, mrb_value exc);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_exc_raise(IntPtr mrb, UInt64 exc);

        // MRB_API mrb_noreturn void mrb_raise(mrb_state *mrb, struct RClass *c, const char *msg);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_raise(IntPtr mrb, IntPtr c, [MarshalAs(UnmanagedType.LPStr)] string msg);

        // MRB_API mrb_noreturn void mrb_name_error_ex(mrb_state *mrb, mrb_sym id, const char *msg);
        // [DllImport(Ruby.MRUBY_LIB, CharSet = CharSet.Ansi, SetLastError = true)]
        // private static extern void mrb_name_error_ex(IntPtr mrb, UInt64 id, [MarshalAs(UnmanagedType.LPStr)] string msg);

        // MRB_API mrb_noreturn void mrb_frozen_error(mrb_state *mrb, void *frozen_obj);
        // [DllImport(Ruby.MRUBY_LIB, CharSet = CharSet.Ansi, SetLastError = true)]
        // private static extern void mrb_frozen_error(IntPtr mrb, IntPtr frozen_obj);

        // MRB_API mrb_noreturn void mrb_argnum_error(mrb_state *mrb, mrb_int argc, int min, int max);
        // [DllImport(Ruby.MRUBY_LIB, CharSet = CharSet.Ansi, SetLastError = true)]
        // private static extern void mrb_argnum_error(IntPtr mrb, int argc, int min, int max);

        // MRB_API void mrb_warn_ex(mrb_state *mrb, const char *msg);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_warn_ex(IntPtr mrb, [MarshalAs(UnmanagedType.LPStr)] string msg);

        // MRB_API mrb_noreturn void mrb_bug(mrb_state *mrb, const char *mesg);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_bug(IntPtr mrb, [MarshalAs(UnmanagedType.LPStr)] string msg);

        // MRB_API void mrb_print_backtrace(mrb_state *mrb);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_print_backtrace(IntPtr mrb);

        // MRB_API void mrb_print_error(mrb_state *mrb);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_print_error(IntPtr mrb);

        // MRB_API mrb_noreturn void mrb_sys_fail(mrb_state *mrb, const char *mesg);
        // [DllImport(Ruby.MRUBY_LIB, CharSet = CharSet.Ansi, SetLastError = true)]
        // private static extern void mrb_sys_fail(IntPtr mrb, [MarshalAs(UnmanagedType.LPStr)] string mesg);

        // MRB_API mrb_value mrb_exc_new_str(mrb_state *mrb, struct RClass* c, mrb_value str);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_exc_new_str(IntPtr mrb, IntPtr c, UInt64 str);

        // MRB_API void mrb_clear_error(mrb_state *mrb);
        // [DllImport(Ruby.MRUBY_LIB, CharSet = CharSet.Ansi, SetLastError = true)]
        // private static extern void mrb_clear_error(IntPtr mrb);

        // MRB_API mrb_bool mrb_check_error(mrb_state *mrb);
        // [DllImport(Ruby.MRUBY_LIB, CharSet = CharSet.Ansi, SetLastError = true)]
        // [return: MarshalAs(UnmanagedType.U1)]
        // private static extern Boolean mrb_check_error(IntPtr mrb);

        // MRB_API mrb_value mrb_protect_error(mrb_state *mrb, mrb_protect_error_func *body, void *userdata, mrb_bool *error);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_protect_error(IntPtr mrb,  [MarshalAs(UnmanagedType.FunctionPtr)] NativeProtectErrorFunc body, IntPtr userdata, [MarshalAs(UnmanagedType.U1)] ref Boolean error);

        // MRB_API mrb_value mrb_protect(mrb_state *mrb, mrb_func_t body, mrb_value data, mrb_bool *State);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_protect(IntPtr mrb, [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc body, UInt64 data, [MarshalAs(UnmanagedType.U1)] ref Boolean state);

        // MRB_API mrb_value mrb_ensure(mrb_state *mrb, mrb_func_t body, mrb_value b_data, mrb_func_t ensure, mrb_value e_data);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_ensure(IntPtr mrb, [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc body, UInt64 b_data, [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc ensure, UInt64 e_data);

        // MRB_API mrb_value mrb_rescue(mrb_state *mrb, mrb_func_t body, mrb_value b_data, mrb_func_t rescue, mrb_value r_data);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_rescue(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc body,
            UInt64 b_data,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc rescue,
            UInt64 r_data);

        // MRB_API mrb_value mrb_rescue_exceptions(mrb_state *mrb, mrb_func_t body, mrb_value b_data, mrb_func_t rescue, mrb_value r_data, mrb_int len, struct RClass **classes);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_rescue_exceptions(IntPtr mrb, [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc body, UInt64 b_data, [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc rescue, UInt64 r_data, int len,
            IntPtr[] classes);
    }
}