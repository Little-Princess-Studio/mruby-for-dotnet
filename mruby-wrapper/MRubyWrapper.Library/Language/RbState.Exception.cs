namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    // typedef mrb_value mrb_protect_error_func(mrb_state *mrb, void *userdata);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt64 NativeProtectErrorFunc(IntPtr mrb, IntPtr userdata);
    
    public delegate RbValue CSharpProtectErrorFunc(RbState state, object userdata);
    
    public partial class RbState
    {
        public void SysFail(string message) => mrb_sys_fail(this.NativeHandler, message);

        public RbValue GenerateExceptionWithNewStr(RbClass c, RbValue str)
        {
            var result = mrb_exc_new_str(this.NativeHandler, c.NativeHandler, str.NativeValue);
            return new RbValue(this, result);
        }
        
        public void ClearError() => mrb_clear_error(this.NativeHandler);
        
        public bool CheckError() => mrb_check_error(this.NativeHandler);

        public RbValue Protect(CSharpProtectErrorFunc body, object data, ref bool error)
        {
            var fn = new NativeProtectErrorFunc((mrb, userdata) =>
            {
                var stat = new RbState() { NativeHandler = mrb };
                var udata = RbHelper.GetObjectFromIntPtr(userdata);
                var res= body(stat, udata).NativeValue;
                RbHelper.FreeIntPtrOfCSharpObject(userdata);
                return res;
            });
            var ptr = RbHelper.GetIntPtrOfCSharpObject(data);
            var result = mrb_protect_error(this.NativeHandler, fn, ptr, ref error);
            return new RbValue(this, result);
        }

        public RbValue Protect(CSharpMethodFunc body, RbValue data, ref bool error)
        {
            var fn = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var result = mrb_protect(this.NativeHandler, fn, data.NativeValue, ref error);
            return new RbValue(this, result);
        }

        public RbValue Ensure(CSharpMethodFunc body, RbValue bData, IntPtr ensure, RbValue eData)
        {
            var fn = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var result = mrb_ensure(this.NativeHandler, fn, bData.NativeValue, ensure, eData.NativeValue);
            return new RbValue(this, result);
        }
        
        public RbValue Rescue(CSharpMethodFunc body, RbValue bData, IntPtr rescue, RbValue rData)
        {
            var fn = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var result = mrb_rescue(this.NativeHandler, fn, bData.NativeValue, rescue, rData.NativeValue);
            return new RbValue(this, result);
        }
        
        public RbValue RescueExceptions(CSharpMethodFunc body, RbValue bData, CSharpMethodFunc rescue, RbValue rData, IntPtr[] classes)
        {
            var funcBody = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(body);
            var funcRescue = RbHelper.BuildCSharpCallbackToNativeCallbackBridgeMethod(rescue);
            var result = mrb_rescue_exceptions(this.NativeHandler, funcBody, bData.NativeValue, funcRescue, rData.NativeValue, classes.Length, classes);
            return new RbValue(this, result);
        }

        public void Raise(RbValue exc) => mrb_exc_raise(this.NativeHandler, exc.NativeValue);

        public void Raise(RbClass c, string msg) => mrb_raise(this.NativeHandler, c.NativeHandler, msg);

        public void RaiseNameError(string name, string msg)
        {
            var symId = GetInternSymbol(name);
            mrb_name_error_ex(this.NativeHandler, symId, msg);
        }

        public void RaiseFrozenError(RbValue frozenObj) => mrb_frozen_error(this.NativeHandler, RbHelper.GetRbObjectPtrFromValue(frozenObj));

        public void RaiseArgumentNumberError(int argc, int min, int max) => mrb_argnum_error(this.NativeHandler, argc, min, max);

        public void WarnEx(string msg) => mrb_warn_ex(this.NativeHandler, msg);

        public void Bug(string msg) => mrb_bug(this.NativeHandler, msg);

        public void PrintBacktrace() => mrb_print_backtrace(this.NativeHandler);

        public void PrintError() => mrb_print_error(this.NativeHandler);

        public RbValue GetExceptionObj()
        {
            var nativeObj = mrb_get_exc_obj(this.NativeHandler);
            return new RbValue(this, nativeObj);
        }
    }
}