namespace MRuby.Library.Language
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    public struct RbProc
    {
        public readonly RbState State;
        public  readonly IntPtr NativeHandler;

        public RbProc(RbState state, IntPtr nativeHandler)
        {
            this.NativeHandler = nativeHandler;
            this.State = state;
        }
        
        public static RbProc FromRbValue(RbValue value)
        {
            var ptr = RbHelper.GetRbObjectPtrFromValue(value);
            return new RbProc(value.State, ptr);
        }
    }

    public struct RbContext : IDisposable
    {
        public readonly RbState State;
        public IntPtr NativeHandler;

        public RbContext(IntPtr nativeHandler, RbState state)
        {
            this.NativeHandler = nativeHandler;
            this.State = state;
        }

        public string? SetFilename(string filename) => RbCompiler.SetContextFileName(this.State, this, filename);

        public void Dispose()
        {
            if (this.NativeHandler != IntPtr.Zero)
            {
                RbCompiler.FreeContext(this.State, this);
                this.NativeHandler = IntPtr.Zero;
            }
        }
    }

    public partial class RbCompiler : IDisposable
    {
        public readonly RbState State;
        public IntPtr NativeHandler;

        private RbCompiler(RbState state, IntPtr nativeHandler)
        {
            this.State = state;
            this.NativeHandler = nativeHandler;
        }

        internal static RbCompiler ParseString(RbState mrb, string code, RbContext ccontext)
        {
            var state = mrb_parse_string(mrb.NativeHandler, code, ccontext.NativeHandler);
            return new RbCompiler(mrb, state);
        }

        internal static RbCompiler NewCompiler(RbState mrb)
        {
            var state = mrb_parser_new(mrb.NativeHandler);
            return new RbCompiler(mrb, state);
        }

        internal static RbContext NewContext(RbState mrb)
        {
            var state = mrb_ccontext_new(mrb.NativeHandler);
            return new RbContext(state, mrb);
        }

        internal static string? SetContextFileName(RbState mrb, RbContext context, string filename)
        {
            var strPtr = mrb_ccontext_filename(mrb.NativeHandler, context.NativeHandler, filename);
            return strPtr != IntPtr.Zero ? Marshal.PtrToStringAnsi(strPtr) : string.Empty;
        }

        internal static void FreeContext(RbState mrb, RbContext context) => mrb_ccontext_free(mrb.NativeHandler, context.NativeHandler);

        private void Free() => mrb_parser_free(this.NativeHandler);

        // public void Parse(RbContext ccontext) => mrb_parser_parse(this.NativeHandler, ccontext.NativeHandler);

        public void SetFilename(string filename) => mrb_parser_set_filename(this.NativeHandler, filename);

        public string GetFilename(UInt16 idx)
        {
            var sym = mrb_parser_get_filename(this.NativeHandler, idx);
            var filename = this.State.GetSymbolName(sym);
            return filename!;
        }

        public RbProc GenerateCode()
        {
            var nativeProc = mrb_generate_code(this.State.NativeHandler, this.NativeHandler);
            return new RbProc(this.State, nativeProc);
        }

        // public RbValue LoadAndRun(RbContext ccontext)
        // {
        //     var nativeVal = mrb_load_exec(this.State.NativeHandler, this.NativeHandler, ccontext.NativeHandler);
        //     return new RbValue(this.State, nativeVal);
        // }

        public RbValue LoadString(string code)
        {
            var nativeVal = mrb_load_string(this.State.NativeHandler, code);
            return new RbValue(this.State, nativeVal);
        }

        public RbValue LoadString(string code, RbContext ccontext)
        {
            var nativeVal = mrb_load_string_cxt(this.State.NativeHandler, code, ccontext.NativeHandler);
            return new RbValue(this.State, nativeVal);
        }

        public RbValue TopRun(RbProc proc, RbValue self, Int64 stackKeep)
        {
            var nativeVal = mrb_top_run(this.State.NativeHandler, proc.NativeHandler, self.NativeValue, stackKeep);
            return new RbValue(this.State, nativeVal);
        }

        public void Dispose()
        {
            if (this.NativeHandler != IntPtr.Zero)
            {
                this.Free();
                this.NativeHandler = IntPtr.Zero;
            }
        }
    }
}