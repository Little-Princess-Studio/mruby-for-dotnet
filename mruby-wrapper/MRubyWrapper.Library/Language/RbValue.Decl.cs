namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbValue
    {
        // MRB_API mrb_value mrb_obj_dup(mrb_state *mrb, mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_dup(IntPtr state, UInt64 obj);
        
        // MRB_API mrb_value mrb_obj_freeze(mrb_state*, mrb_value);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_freeze(IntPtr mrb, UInt64 obj);
        
        // MRB_API mrb_bool mrb_check_frozen_ex(mrb_value o);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_check_frozen_ex(UInt64 obj);
        
        // MRB_API mrb_int mrb_obj_id(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern Int64 mrb_obj_id(UInt64 obj);

        // MRB_API mrb_bool mrb_obj_eq(mrb_state *mrb, mrb_value a, mrb_value b);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_obj_equal(IntPtr mrb, UInt64 a, UInt64 b);

        // MRB_API mrb_bool mrb_eql(mrb_state *mrb, mrb_value obj1, mrb_value obj2);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_eql(IntPtr mrb, UInt64 obj1, UInt64 obj2);
        
        // MRB_API mrb_int mrb_cmp(mrb_state *mrb, mrb_value obj1, mrb_value obj2);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern Int64 mrb_cmp(IntPtr mrb, UInt64 obj1, UInt64 obj2);
        
        // MRB_API mrb_value mrb_any_to_s(mrb_state *mrb, mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_any_to_s(IntPtr mrb, UInt64 obj);
        
        // MRB_API const char * mrb_obj_classname(mrb_state *mrb, mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_obj_classname(IntPtr mrb, UInt64 obj);
        
        // MRB_API struct RClass* mrb_obj_class(mrb_state *mrb, mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_obj_class(IntPtr mrb, UInt64 obj);
        
        // MRB_API mrb_bool mrb_obj_is_kind_of(mrb_state *mrb, mrb_value obj, struct RClass *c);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_obj_is_kind_of(IntPtr mrb, UInt64 obj, IntPtr @class);
        
        // MRB_API mrb_value mrb_obj_inspect(mrb_state *mrb, mrb_value self);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_inspect(IntPtr mrb, UInt64 self);
        
        // MRB_API mrb_value mrb_obj_clone(mrb_state *mrb, mrb_value self);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_obj_clone(IntPtr mrb, UInt64 self);
        
        // MRB_API mrb_value mrb_attr_get(mrb_state *mrb, mrb_value obj, mrb_sym id);
        // [DllImport(Ruby.MRUBY_LIB, CharSet = CharSet.Ansi, SetLastError = true)]
        // private static extern UInt64 mrb_attr_get(IntPtr mrb, UInt64 obj, UInt64 id);
        
        // MRB_API mrb_value mrb_iv_get(mrb_state *mrb, mrb_value obj, mrb_sym sym);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_iv_get(IntPtr mrb, UInt64 obj, UInt64 sym);

        // MRB_API void mrb_iv_set(mrb_state *mrb, mrb_value obj, mrb_sym sym, mrb_value v);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_iv_set(IntPtr mrb, UInt64 obj, UInt64 sym, UInt64 v);

        // MRB_API mrb_bool mrb_iv_defined(mrb_state*, mrb_value, mrb_sym);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern Boolean mrb_iv_defined(IntPtr mrb, UInt64 obj, UInt64 sym);

        // MRB_API mrb_value mrb_iv_remove(mrb_state *mrb, mrb_value obj, mrb_sym sym);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern UInt64 mrb_iv_remove(IntPtr mrb, UInt64 obj, UInt64 sym);

        // MRB_API void mrb_iv_copy(mrb_state *mrb, mrb_value dst, mrb_value src);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_iv_copy(IntPtr mrb, UInt64 dst, UInt64 src);
        
        // MRB_API void mrb_iv_foreach(mrb_state *mrb, mrb_value obj, mrb_iv_foreach_func *func, void *p);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void mrb_iv_foreach(IntPtr mrb, UInt64 obj, IvForeachFunc func, IntPtr p);
       
        // MRB_API void *mrb_data_object_get_ptr(mrb_state *mrb, mrb_value obj, mrb_data_type *type);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_data_object_get_ptr(IntPtr mrb, UInt64 obj, IntPtr type);

        // void *mrb_data_object_get_type(mrb_value obj);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_data_object_get_type(UInt64 obj);
        
        // MRB_API void mrb_define_singleton_method(mrb_state *mrb, struct RObject *cla, const char *name, mrb_func_t fun, mrb_aspec aspec);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrb_define_singleton_method(
            IntPtr mrb,
            IntPtr obj,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeMethodFunc nativeMethod,
            uint parameterAspect);
        
        // MRB_API struct RClass *mrb_singleton_class_ptr(mrb_state *mrb, mrb_value val);
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr mrb_singleton_class_ptr(IntPtr state, UInt64 val);
        
        // mrb_int mrb_obj_hash(mrb_state *mrb, mrb_value self)
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern Int64 mrb_obj_hash(IntPtr mrb, UInt64 self);
    }
}