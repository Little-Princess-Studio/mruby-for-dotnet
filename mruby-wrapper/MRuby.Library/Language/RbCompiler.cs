namespace MRuby.Library.Language
{
    using System;

    public struct RbProc
    {
        private readonly RbState state;
        internal readonly IntPtr NativeHandler;

        public RbProc(RbState state, IntPtr nativeHandler)
        {
            this.NativeHandler = nativeHandler;
            this.state = state;
        }

        public RbValue ToRbValue() => RbHelper.PtrToRbValue(this.state, this.NativeHandler);
        
        public static RbProc FromRbValue(RbValue value)
        {
            var ptr = RbHelper.GetRbObjectPtrFromValue(value);
            return new RbProc(value.RbState, ptr);
        }
    }

    public struct RbContext : IDisposable
    {
        private readonly RbState state;
        internal IntPtr NativeHandler;

        public RbContext(IntPtr nativeHandler, RbState state)
        {
            this.NativeHandler = nativeHandler;
            this.state = state;
        }

        public void Dispose()
        {
            if (this.NativeHandler != IntPtr.Zero)
            {
                RbCompiler.FreeContext(this.state, this);
                this.NativeHandler = IntPtr.Zero;
            }
        }
    }
    
    public partial class RbCompiler: IDisposable
    {
        private readonly RbState rbState;
        private IntPtr nativeHandler;
        
        private RbCompiler(RbState rbState, IntPtr nativeHandler)
        {
            this.rbState = rbState;
            this.nativeHandler = nativeHandler;
        }
        
        public static RbCompiler ParseString(RbState mrb, string code, RbContext ccontext)
        {
            var state = mrb_parse_string(mrb.NativeHandler, code, ccontext.NativeHandler);
            return new RbCompiler(mrb, state);
        }
        
        public static RbCompiler ParserNew(RbState mrb)
        {
            var state = mrb_parser_new(mrb.NativeHandler);
            return new RbCompiler(mrb, state);
        }
        
        public static RbContext NewContext(RbState mrb)
        {
            var state = mrb_ccontext_new(mrb.NativeHandler);
            return new RbContext(state, mrb);
        }
        
        internal static void FreeContext(RbState mrb, RbContext context) => mrb_ccontext_free(mrb.NativeHandler, context.NativeHandler);

        private void Free() => mrb_parser_free(this.nativeHandler);

        // public void Parse(RbContext ccontext) => mrb_parser_parse(this.nativeHandler, ccontext.NativeHandler);

        public void SetFilename(string filename) => mrb_parser_set_filename(this.nativeHandler, filename);

        public string GetFilename(UInt16 idx)
        {
            var sym = mrb_parser_get_filename(this.nativeHandler, idx);
            var filename = this.rbState.GetSymbolName(sym);
            return filename!;
        }
        
        public RbProc GenerateCode()
        {
            var nativeProc = mrb_generate_code(this.rbState.NativeHandler, this.nativeHandler);
            return new RbProc(this.rbState, nativeProc);
        }

        // public RbValue LoadAndRun(RbContext ccontext)
        // {
        //     var nativeVal = mrb_load_exec(this.rbState.NativeHandler, this.nativeHandler, ccontext.NativeHandler);
        //     return new RbValue(this.rbState, nativeVal);
        // }

        public RbValue LoadString(string code)
        {
            var nativeVal = mrb_load_string(this.rbState.NativeHandler, code);
            return new RbValue(this.rbState, nativeVal);
        }

        public RbValue LoadString(string code, RbContext ccontext)
        {
            var nativeVal = mrb_load_string_cxt(this.rbState.NativeHandler, code, ccontext.NativeHandler);
            return new RbValue(this.rbState, nativeVal);
        }
        
        public RbValue TopRun(RbProc proc, RbValue self, Int64 stackKeep)
        {
            var nativeVal = mrb_top_run(this.rbState.NativeHandler, proc.NativeHandler, self.NativeValue, stackKeep);
            return new RbValue(this.rbState, nativeVal);
        }

        public void Dispose()
        {
            if (this.nativeHandler != IntPtr.Zero)
            {
                this.Free();
                this.nativeHandler = IntPtr.Zero;
            }
        }
    }
}