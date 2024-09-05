namespace MRuby.Library.Language
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.InteropServices;

    // typedef mrb_value mrb_protect_error_func(mrb_state *mrb, void *userdata);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt64 NativeProtectErrorFunc(IntPtr mrb, IntPtr userdata);

    public delegate RbValue CSharpProtectErrorFunc(RbState state);

    public partial class RbState
    {
        // public void SysFail(string message) => mrb_sys_fail(this.NativeHandler, message);

        public RbValue GenerateExceptionWithNewStr(RbClass c, string msg)
        {
            var str = this.BoxString(msg);
            var result = mrb_exc_new_str(this.NativeHandler, c.NativeHandler, str.NativeValue);
            return new RbValue(this, result);
        }

        // public void ClearError() => mrb_clear_error(this.NativeHandler);
        //
        // public bool CheckError() => mrb_check_error(this.NativeHandler);

        public RbValue Protect(CSharpProtectErrorFunc body, ref bool error)
        {
            var fn = new NativeProtectErrorFunc((mrb, _) =>
            {
                var stat = new RbState { NativeHandler = mrb };
                var res = body(stat).NativeValue;
                return res;
            });
            var result = mrb_protect_error(this.NativeHandler, fn, IntPtr.Zero, ref error);
            return new RbValue(this, result);
        }

        public RbValue Protect(CSharpMethodFunc body, ref bool error)
        {
            var fn = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var result = mrb_protect(this.NativeHandler, fn, this.RbNil.NativeValue, ref error);
            return new RbValue(this, result);
        }

        public RbValue Protect(CSharpMethodFunc body, RbValue data, ref bool error)
        {
            var fn = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var result = mrb_protect(this.NativeHandler, fn, data.NativeValue, ref error);
            return new RbValue(this, result);
        }

        public RbValue Ensure(CSharpMethodFunc body, RbValue userdata, CSharpMethodFunc ensureBody, RbValue eData)
        {
            var fn = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var fnEnsure = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(ensureBody);
            var result = mrb_ensure(this.NativeHandler, fn, userdata.NativeValue, fnEnsure, eData.NativeValue);
            return new RbValue(this, result);
        }

        public RbValue Rescue(CSharpMethodFunc body, RbValue userdata, CSharpMethodFunc rescueBody, RbValue rData)
        {
            var fn = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var fnRescure = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(rescueBody);
            var result = mrb_rescue(this.NativeHandler, fn, userdata.NativeValue, fnRescure, rData.NativeValue);
            return new RbValue(this, result);
        }

        public RbValue RescueExceptions(CSharpMethodFunc body, RbValue bData, CSharpMethodFunc rescue, RbValue rData, RbClass[] classes)
        {
            var funcBody = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var funcRescue = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(rescue);
            var nativeExcClasses = classes.Select(c => c.NativeHandler).ToArray();
            var result = mrb_rescue_exceptions(this.NativeHandler, funcBody, bData.NativeValue, funcRescue, rData.NativeValue, classes.Length, nativeExcClasses);
            return new RbValue(this, result);
        }

        public RbValue HandleException(
            CSharpMethodFunc main, RbValue mainUserData,
            CSharpMethodFunc rescue, RbValue rescueUserData, RbClass[] classes,
            CSharpMethodFunc ensure, RbValue ensureUserData)
        {
            var fnCaughtMain = new CSharpMethodFunc((stat, userdata, args) =>
                this.RescueExceptions(main, mainUserData, rescue, rescueUserData, classes));
            return this.Ensure(fnCaughtMain, this.RbNil, ensure, ensureUserData);
        }

        public void Raise(RbValue exc) => mrb_exc_raise(this.NativeHandler, exc.NativeValue);

        public void Raise(RbClass c, string msg) => mrb_raise(this.NativeHandler, c.NativeHandler, msg);

        // public void RaiseNameError(string name, string msg)
        // {
        //     var symId = GetInternSymbol(name);
        //     mrb_name_error_ex(this.NativeHandler, symId, msg);
        // }
        //
        // public void RaiseFrozenError(RbValue frozenObj) => mrb_frozen_error(this.NativeHandler, RbHelper.GetRbObjectPtrFromValue(frozenObj));
        //
        // public void RaiseArgumentNumberError(int argc, int min, int max) => mrb_argnum_error(this.NativeHandler, argc, min, max);

        [ExcludeFromCodeCoverage]
        public void Warn(string msg) => mrb_warn_ex(this.NativeHandler, msg);

        [ExcludeFromCodeCoverage]
        public void Bug(string msg) => mrb_bug(this.NativeHandler, msg);

        [ExcludeFromCodeCoverage]
        public void PrintBacktrace() => mrb_print_backtrace(this.NativeHandler);

        [ExcludeFromCodeCoverage]
        public void PrintError() => mrb_print_error(this.NativeHandler);
    }
}