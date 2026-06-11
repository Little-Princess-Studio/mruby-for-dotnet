namespace MRuby.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbCompiler
    {
        // MRB_API struct mrb_parser_state* mrb_parser_new(mrb_state*);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern IntPtr mrb_parser_new(IntPtr mrb);

        // MRB_API void mrb_parser_free(struct mrb_parser_state*);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern void mrb_parser_free(IntPtr parser);

        // MRB_API void mrb_parser_parse(struct mrb_parser_state*,mrb_ccontext*);
        // [DllImport(Ruby.MRUBY_LIB, CharSet = CharSet.Ansi)]
        // internal static extern void mrb_parser_parse(IntPtr parser, IntPtr ccontext);

        // MRB_API const char *mrb_ccontext_filename(mrb_state *mrb, mrb_ccontext *c, const char *s);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern IntPtr mrb_ccontext_filename(IntPtr mrb, IntPtr ccontext, [MarshalAs(UnmanagedType.LPStr)] string filename);

        // MRB_API void mrb_parser_set_filename(struct mrb_parser_state*, char const* filename);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern void mrb_parser_set_filename(IntPtr parser, [MarshalAs(UnmanagedType.LPStr)] string filename);

        // MRB_API mrb_sym mrb_parser_get_filename(struct mrb_parser_state*, uint16_t idx);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrb_parser_get_filename(IntPtr parser, UInt16 idx);

        // MRB_API struct mrb_parser_state* mrb_parse_string(mrb_state*,const char*,mrb_ccontext*);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern IntPtr mrb_parse_string(IntPtr mrb, [MarshalAs(UnmanagedType.LPUTF8Str)] string code, IntPtr ccontext);

        // MRB_API struct RProc* mrb_generate_code(mrb_state*, struct mrb_parser_state*);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern IntPtr mrb_generate_code(IntPtr mrb, IntPtr parser);

        // MRB_API mrb_value mrb_load_exec(mrb_state *mrb, struct mrb_parser_state *p, mrb_ccontext *c);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrb_load_exec(IntPtr mrb, IntPtr parser, IntPtr ccontext);

        // MRB_API mrb_value mrb_load_string(mrb_state *mrb, const char *s);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrb_load_string(IntPtr mrb, [MarshalAs(UnmanagedType.LPUTF8Str)] string code);

        // MRB_API mrb_value mrb_load_string_cxt(mrb_state *mrb, const char *s, mrb_ccontext *cxt);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrb_load_string_cxt(IntPtr mrb, [MarshalAs(UnmanagedType.LPUTF8Str)] string code, IntPtr ccontext);

        // MRB_API mrb_value mrb_top_run(mrb_state *mrb, const struct RProc *proc, mrb_value self, mrb_int stack_keep);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrb_top_run(IntPtr mrb, IntPtr proc, UInt64 self, Int64 stackKeep);

        // longjmp firewall (mruby-shared/src/main.c): protected variants catch a runtime
        // raise during load/run in NATIVE code instead of letting it longjmp across a
        // managed callback frame above us (the macOS GC-suspension crash).
        // MRB_API mrb_value mrbdotnet_load_string_protected(mrb_state*, const char*, mrb_bool*);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrbdotnet_load_string_protected(IntPtr mrb, [MarshalAs(UnmanagedType.LPUTF8Str)] string code, [MarshalAs(UnmanagedType.U1)] ref Boolean raised);

        // MRB_API mrb_value mrbdotnet_load_string_cxt_protected(mrb_state*, const char*, mrbc_context*, mrb_bool*);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrbdotnet_load_string_cxt_protected(IntPtr mrb, [MarshalAs(UnmanagedType.LPUTF8Str)] string code, IntPtr ccontext, [MarshalAs(UnmanagedType.U1)] ref Boolean raised);

        // MRB_API mrb_value mrbdotnet_top_run_protected(mrb_state*, const struct RProc*, mrb_value, mrb_int, mrb_bool*);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern UInt64 mrbdotnet_top_run_protected(IntPtr mrb, IntPtr proc, UInt64 self, Int64 stackKeep, [MarshalAs(UnmanagedType.U1)] ref Boolean raised);

        // MRB_API mrb_ccontext* mrb_ccontext_new(mrb_state *mrb);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern IntPtr mrb_ccontext_new(IntPtr mrb);

        // MRB_API void mrb_ccontext_free(mrb_state *mrb, mrb_ccontext *cxt);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        internal static extern void mrb_ccontext_free(IntPtr mrb, IntPtr ccontext);
    }
}