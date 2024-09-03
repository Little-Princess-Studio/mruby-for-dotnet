namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbArray
    {
        // MRB_API mrb_value mrb_ary_new(mrb_state *mrb);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_ary_new(IntPtr mrb);

        // MRB_API mrb_value mrb_ary_new_from_values(mrb_state *mrb, mrb_int size, const mrb_value *vals);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_ary_new_from_values(IntPtr mrb, int size, IntPtr vals);

        // MRB_API mrb_value mrb_assoc_new(mrb_state *mrb, mrb_value car, mrb_value cdr);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_assoc_new(IntPtr mrb, UInt64 car, UInt64 cdr);

        // MRB_API void mrb_ary_concat(mrb_state *mrb, mrb_value self, mrb_value other);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void mrb_ary_concat(IntPtr mrb, UInt64 self, UInt64 other);

        // MRB_API void mrb_ary_push(mrb_state *mrb, mrb_value array, mrb_value value);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void mrb_ary_push(IntPtr mrb, UInt64 array, UInt64 value);

        // MRB_API mrb_value mrb_ary_pop(mrb_state *mrb, mrb_value ary);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_ary_pop(IntPtr mrb, UInt64 ary);

        // MRB_API void mrb_ary_set(mrb_state *mrb, mrb_value ary, mrb_int n, mrb_value val);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void mrb_ary_set(IntPtr mrb, UInt64 ary, int n, UInt64 val);

        // MRB_API void mrb_ary_replace(mrb_state *mrb, mrb_value self, mrb_value other);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void mrb_ary_replace(IntPtr mrb, UInt64 self, UInt64 other);

        // MRB_API mrb_value mrb_ary_unshift(mrb_state *mrb, mrb_value self, mrb_value item);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_ary_unshift(IntPtr mrb, UInt64 self, UInt64 item);

        // MRB_API mrb_value mrb_ary_entry(mrb_value ary, mrb_int offset);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_ary_entry(UInt64 ary, int offset);

        // MRB_API mrb_value mrb_ary_splice(mrb_state *mrb, mrb_value self, mrb_int head, mrb_int len, mrb_value rpl);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_ary_splice(IntPtr mrb, UInt64 self, int head, int len, UInt64 rpl);

        // MRB_API mrb_value mrb_ary_shift(mrb_state *mrb, mrb_value self);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_ary_shift(IntPtr mrb, UInt64 self);

        // MRB_API mrb_value mrb_ary_clear(mrb_state *mrb, mrb_value self);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_ary_clear(IntPtr mrb, UInt64 self);

        // MRB_API mrb_value mrb_ary_join(mrb_state *mrb, mrb_value ary, mrb_value sep);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_ary_join(IntPtr mrb, UInt64 ary, UInt64 sep);

        // MRB_API mrb_value mrb_ary_resize(mrb_state *mrb, mrb_value ary, mrb_int new_len);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_ary_resize(IntPtr mrb, UInt64 ary, int new_len);
        
        // MRB_API mrb_int mrb_array_len(mrb_value array);
        [DllImport("mruby_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int mrb_array_len(UInt64 array);
    }
}