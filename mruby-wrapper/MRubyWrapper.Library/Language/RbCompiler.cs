namespace MRubyWrapper.Library.Language
{
    using System;

    public struct RbProc
    {
        internal readonly IntPtr NativeHandler;

        public RbProc(IntPtr nativeHandler)
        {
            this.NativeHandler = nativeHandler;
        }
    }
    
    public partial class RbCompiler: IDisposable
    {
        private readonly IntPtr nativeHandler;
        private readonly RbState rbState;

        private RbCompiler(RbState rbState, IntPtr nativeHandler)
        {
            this.rbState = rbState;
            this.nativeHandler = IntPtr.Zero;
        }
        
        public static RbCompiler ParseString(RbState mrb, string code, IntPtr ccontext)
        {
            var state = mrb_parse_string(mrb.MrbState, code, ccontext);
            return new RbCompiler(mrb, state);
        }
        
        public static RbCompiler ParserNew(RbState mrb)
        {
            var state = mrb_parser_new(mrb.MrbState);
            return new RbCompiler(mrb, state);
        }
        
        public void Free()
        {
            mrb_parser_free(this.nativeHandler);
        }

        public void Parse(IntPtr ccontext)
        {
            mrb_parser_parse(this.nativeHandler, ccontext);
        }

        public void SetFilename(string filename)
        {
            mrb_parser_set_filename(this.nativeHandler, filename);
        }

        public UInt64 GetFilename(UInt16 idx)
        {
            return mrb_parser_get_filename(this.nativeHandler, idx);
        }
        
        public RbProc GenerateCode()
        {
            var nativeProc = mrb_generate_code(this.rbState.MrbState, this.nativeHandler);
            return new RbProc(nativeProc);
        }

        public RbValue LoadExec(IntPtr ccontext)
        {
            var nativeVal = mrb_load_exec(this.rbState.MrbState, this.nativeHandler, ccontext);
            return new RbValue(this.rbState, nativeVal);
        }

        public RbValue LoadString(string code)
        {
            var nativeVal = mrb_load_string(this.rbState.MrbState, code);
            return new RbValue(this.rbState, nativeVal);
        }

        public RbValue LoadStringCxt(string code, IntPtr ccontext)
        {
            var nativeVal = mrb_load_string_cxt(this.rbState.MrbState, code, ccontext);
            return new RbValue(this.rbState, nativeVal);
        }

        public RbValue TopRun(RbProc proc, RbValue self, Int64 stackKeep)
        {
            var nativeVal = mrb_top_run(this.rbState.MrbState, proc.NativeHandler, self.NativeValue.Value, stackKeep);
            return new RbValue(this.rbState, nativeVal);
        }

        public void Dispose()
        {
            if (this.nativeHandler != IntPtr.Zero)
            {
                this.Free();
            }
        }
    }
}