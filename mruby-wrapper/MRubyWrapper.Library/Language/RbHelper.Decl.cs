namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbHelper
    {
        // MRB_API mrb_value mrb_funcall_argv(mrb_state *mrb, mrb_value val, mrb_sym name, mrb_int argc, const mrb_value *argv);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern uint mrb_funcall_argv(
            IntPtr state,
            UInt64 val,
            UInt64 sym,
            Int64 argc,
            UInt64[] argv);

        // MRB_API mrb_value mrb_funcall_with_block(mrb_state *mrb, mrb_value val, mrb_sym name, mrb_int argc, const mrb_value *argv, mrb_value block);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern uint mrb_funcall_with_block(
            IntPtr state,
            UInt64 val,
            UInt64 sym,
            Int64 argc,
            UInt64[] argv,
            UInt64 block);
        
        // MRB_API mrb_sym mrb_intern_cstr(mrb_state *mrb, const char* str);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern UInt32 mrb_intern_cstr(
            IntPtr state,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API mrb_bool mrb_block_given_p(mrb_state *mrb);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern bool mrb_block_given_p(IntPtr mrb);
        
        // MRB_API const char *mrb_sym_name(mrb_state*,mrb_sym);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_sym_name(IntPtr mrb, UInt64 sym);

        // MRB_API const char *mrb_sym_dump(mrb_state*,mrb_sym);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_sym_dump(IntPtr mrb, UInt64 sym);
        
        // MRB_API mrb_value mrb_sym_str(mrb_state*,mrb_sym);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_sym_str(IntPtr mrb, UInt64 sym);

        // MRB_API mrb_value mrb_str_new_cstr(mrb_state*, const char*);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern UInt64 mrb_str_new_cstr(IntPtr mrb, [MarshalAs(UnmanagedType.LPStr)] string str);
    }
}