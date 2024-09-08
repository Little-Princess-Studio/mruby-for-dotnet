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

        public RbValue Protect(CSharpMethodFunc body, ref bool error, out NativeMethodFunc delegateFunc)
        {
            delegateFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var result = mrb_protect(this.NativeHandler, delegateFunc, this.RbNil.NativeValue, ref error);
            return new RbValue(this, result);
        }

        public RbValue Protect(CSharpMethodFunc body, RbValue data, ref bool error, out NativeMethodFunc delegateFunc)
        {
            delegateFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var result = mrb_protect(this.NativeHandler, delegateFunc, data.NativeValue, ref error);
            return new RbValue(this, result);
        }

        public RbValue Ensure(CSharpMethodFunc body, RbValue userdata, CSharpMethodFunc ensureBody, RbValue eData,
            out NativeMethodFunc delegateBodyFunc, out NativeMethodFunc delegateEnsureFunc)
        {
            delegateBodyFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            delegateEnsureFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(ensureBody);
            var result = mrb_ensure(this.NativeHandler, delegateBodyFunc, userdata.NativeValue, delegateEnsureFunc, eData.NativeValue);
            return new RbValue(this, result);
        }

        public RbValue Rescue(CSharpMethodFunc body, RbValue userdata, CSharpMethodFunc rescueBody, RbValue rData,
            out NativeMethodFunc delegateFunc, out NativeMethodFunc delegateRescueFunc)
        {
            delegateFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            delegateRescueFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(rescueBody);
            var result = mrb_rescue(this.NativeHandler, delegateFunc, userdata.NativeValue, delegateRescueFunc, rData.NativeValue);
            return new RbValue(this, result);
        }

        public RbValue RescueExceptions(CSharpMethodFunc body, RbValue bData, CSharpMethodFunc rescue, RbValue rData, RbClass[] classes,
            out NativeMethodFunc delegateFunc, out NativeMethodFunc delegateRescueFunc)
        {
            delegateFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            delegateRescueFunc = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(rescue);
            var nativeExcClasses = classes.Select(c => c.NativeHandler).ToArray();
            var result = mrb_rescue_exceptions(this.NativeHandler, delegateFunc, bData.NativeValue, delegateRescueFunc, rData.NativeValue, classes.Length, nativeExcClasses);
            return new RbValue(this, result);
        }

        public RbValue HandleException(
            CSharpMethodFunc main, RbValue mainUserData,
            CSharpMethodFunc rescue, RbValue rescueUserData, RbClass[] classes,
            CSharpMethodFunc ensure, RbValue ensureUserData,
            out NativeMethodFunc fnMain, out NativeMethodFunc fnRescue, out NativeMethodFunc fnEnsure)
        {
            NativeMethodFunc fnMainInside = null!;
            NativeMethodFunc fnRescueInside  = null!;

            var fnCaughtMain = new CSharpMethodFunc((stat, userdata, args) =>
            {
                var res = this.RescueExceptions(main, mainUserData, rescue, rescueUserData, classes, out fnMainInside, out fnRescueInside);
                return res;
            });

            var result = this.Ensure(fnCaughtMain, this.RbNil, ensure, ensureUserData, out fnMainInside, out fnEnsure);
            fnMain = fnMainInside;
            fnRescue = fnRescueInside;
            return result;
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