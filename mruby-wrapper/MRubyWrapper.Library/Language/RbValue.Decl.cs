namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbValue
    {
        // MRB_API struct RClass *mrb_singleton_class_ptr(mrb_state *mrb, mrb_value val);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_singleton_class_ptr(IntPtr state, UInt64 val);

        // MRB_API mrb_value mrb_obj_dup(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_dup(IntPtr state, UInt64 obj);
        
        // MRB_API mrb_value mrb_obj_freeze(mrb_state*, mrb_value);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_freeze(IntPtr mrb, UInt64 obj);
        
        // MRB_API mrb_int mrb_obj_id(mrb_value obj);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern Int64 mrb_obj_id(UInt64 obj);

        // MRB_API mrb_bool mrb_obj_eq(mrb_state *mrb, mrb_value a, mrb_value b);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool mrb_obj_eq(IntPtr mrb, UInt64 a, UInt64 b);

        // MRB_API mrb_bool mrb_equal(mrb_state *mrb, mrb_value obj1, mrb_value obj2);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool mrb_equal(IntPtr mrb, UInt64 obj1, UInt64 obj2);
        
        // MRB_API mrb_int mrb_cmp(mrb_state *mrb, mrb_value obj1, mrb_value obj2);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern Int64 mrb_cmp(IntPtr mrb, UInt64 obj1, UInt64 obj2);
        
        // MRB_API mrb_value mrb_any_to_s(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_any_to_s(IntPtr mrb, UInt64 obj);
        
        // MRB_API const char * mrb_obj_classname(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_obj_classname(IntPtr mrb, UInt64 obj);
        
        // MRB_API struct RClass* mrb_obj_class(mrb_state *mrb, mrb_value obj);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_obj_class(IntPtr mrb, UInt64 obj);
        
        // MRB_API mrb_value mrb_class_path(mrb_state *mrb, struct RClass *c);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_class_path(IntPtr mrb, IntPtr @class);
        
        // MRB_API mrb_bool mrb_obj_is_kind_of(mrb_state *mrb, mrb_value obj, struct RClass *c);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool mrb_obj_is_kind_of(IntPtr mrb, UInt64 obj, IntPtr @class);
        
        // MRB_API mrb_value mrb_obj_inspect(mrb_state *mrb, mrb_value self);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_inspect(IntPtr mrb, UInt64 self);
        
        // MRB_API mrb_value mrb_obj_clone(mrb_state *mrb, mrb_value self);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_clone(IntPtr mrb, UInt64 self);
        
        // MRB_API mrb_value mrb_attr_get(mrb_state *mrb, mrb_value obj, mrb_sym id);
        [DllImport("mruby.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_attr_get(IntPtr mrb, UInt64 obj, UInt64 id);
    }
}