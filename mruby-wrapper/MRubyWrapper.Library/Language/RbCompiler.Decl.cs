namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbCompiler
    {
        // MRB_API struct mrb_parser_state* mrb_parser_new(mrb_state*);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_parser_new(IntPtr mrb);
        
        // MRB_API void mrb_parser_free(struct mrb_parser_state*);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        internal static extern void mrb_parser_free(IntPtr parser);
        
        // MRB_API void mrb_parser_parse(struct mrb_parser_state*,mrb_ccontext*);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        internal static extern void mrb_parser_parse(IntPtr parser, IntPtr ccontext);
        
        // MRB_API void mrb_parser_set_filename(struct mrb_parser_state*, char const* filename);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        internal static extern void mrb_parser_set_filename(IntPtr parser, [MarshalAs(UnmanagedType.LPStr)] string filename);
        
        // MRB_API mrb_sym mrb_parser_get_filename(struct mrb_parser_state*, uint16_t idx);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrb_parser_get_filename(IntPtr parser, UInt16 idx);
        
        // MRB_API struct mrb_parser_state* mrb_parse_string(mrb_state*,const char*,mrb_ccontext*);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        internal static extern IntPtr mrb_parse_string(IntPtr mrb, [MarshalAs(UnmanagedType.LPStr)] string code, IntPtr ccontext);
        
        // MRB_API struct RProc* mrb_generate_code(mrb_state*, struct mrb_parser_state*);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        internal static extern IntPtr mrb_generate_code(IntPtr mrb, IntPtr parser);
        
        // MRB_API mrb_value mrb_load_exec(mrb_state *mrb, struct mrb_parser_state *p, mrb_ccontext *c);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrb_load_exec(IntPtr mrb, IntPtr parser, IntPtr ccontext);
        
        // MRB_API mrb_value mrb_load_string(mrb_state *mrb, const char *s);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrb_load_string(IntPtr mrb, [MarshalAs(UnmanagedType.LPStr)] string code);
        
        // MRB_API mrb_value mrb_load_string_cxt(mrb_state *mrb, const char *s, mrb_ccontext *cxt);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrb_load_string_cxt(IntPtr mrb, [MarshalAs(UnmanagedType.LPStr)] string code, IntPtr ccontext);

        // MRB_API mrb_value mrb_top_run(mrb_state *mrb, const struct RProc *proc, mrb_value self, mrb_int stack_keep);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrb_top_run(IntPtr mrb, IntPtr proc, UInt64 self, Int64 stackKeep);
    }
}