namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public struct RbState
    {
        public IntPtr MrbState { get; set; }

        // MRB_API struct RClass *mrb_define_class(mrb_state *mrb, const char *name, struct RClass *super);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_class(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            IntPtr @class);

        // MRB_API struct RClass *mrb_define_module(mrb_state *mrb, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_define_module(
            IntPtr state,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API mrb_bool mrb_class_defined(mrb_state *mrb, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool mrb_class_defined(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass* mrb_class_get(mrb_state *mrb, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_class_get(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass* mrb_exc_get_id(mrb_state *mrb, mrb_sym name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_exc_get_id(
            IntPtr mrb,
            UInt64 sym);

        // MRB_API mrb_bool mrb_class_defined_under(mrb_state *mrb, struct RClass *outer, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool mrb_class_defined_under(
            IntPtr mrb,
            IntPtr outer,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass * mrb_class_get_under(mrb_state *mrb, struct RClass *outer, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_class_get_under(
            IntPtr mrb,
            IntPtr outer,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API struct RClass * mrb_module_get(mrb_state *mrb, const char *name);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_module_get(
            IntPtr mrb,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        // MRB_API void mrb_notimplement(mrb_state*);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_notimplement(IntPtr mrb);

        // MRB_API mrb_value mrb_notimplement_m(mrb_state*, mrb_value);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_notimplement_m(IntPtr mrb, UInt64 value);

        // MRB_API mrb_value mrb_obj_itself(mrb_state*, mrb_value);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_itself(IntPtr mrb, UInt64 value);

        public RbClass DefineClass(string name, RbClass? @class)
        {
            var classPtr = mrb_define_class(this.MrbState, name, @class?.NativeHandler ?? IntPtr.Zero);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this,
            };
        }

        public RbClass DefineModule(string name)
        {
            var modulePtr = mrb_define_module(this.MrbState, name);
            return new RbClass
            {
                NativeHandler = modulePtr,
                RbState = this,
            };
        }

        public bool ClassDefined(string name) => mrb_class_defined(this.MrbState, name);

        public RbClass GetClass(string name)
        {
            var classPtr = mrb_class_get(this.MrbState, name);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this,
            };
        }

        public RbClass GetExceptionClass(string name)
        {
            var classPtr = mrb_exc_get_id(this.MrbState, RbHelper.GetInternSymbol(this, name));
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this,
            };
        }

        public bool ClassDefinedUnder(RbClass outer, string name) => mrb_class_defined_under(this.MrbState, outer.NativeHandler, name);

        public RbClass GetClassUnder(RbClass outer, string name)
        {
            var classPtr = mrb_class_get_under(this.MrbState, outer.NativeHandler, name);
            return new RbClass
            {
                NativeHandler = classPtr,
                RbState = this,
            };
        }

        public RbClass GetModule(string name)
        {
            var modulePtr = mrb_module_get(this.MrbState, name);
            return new RbClass
            {
                NativeHandler = modulePtr,
                RbState = this,
            };
        }
        
        public void NotImplement() => mrb_notimplement(this.MrbState);

        public RbValue NotImplementM(RbValue value)
        {
            var result = mrb_notimplement_m(this.MrbState, value.NativeValue.Value);
            return new RbValue(this, result);
        }

        public RbValue ObjItself(RbValue value)
        {
            var result = mrb_obj_itself(this.MrbState, value.NativeValue.Value);
            return new RbValue(this, result);
        }
    }
}