namespace MRubyWrapper.Library.Language
{
    using System;
    using System.Runtime.InteropServices;

    public partial class RbHash
    {
        // MRB_API mrb_value mrb_hash_new_capa(mrb_state *mrb, mrb_int capa);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_hash_new_capa(IntPtr mrb, int capa);

        // MRB_API mrb_value mrb_hash_new(mrb_state *mrb);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_hash_new(IntPtr mrb);

        // MRB_API void mrb_hash_set(mrb_state *mrb, mrb_value hash, mrb_value key, mrb_value val);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mrb_hash_set(IntPtr mrb, UInt64 hash, UInt64 key, UInt64 val);

        // MRB_API mrb_value mrb_hash_get(mrb_state *mrb, mrb_value hash, mrb_value key);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_hash_get(IntPtr mrb, UInt64 hash, UInt64 key);

        // MRB_API mrb_value mrb_hash_fetch(mrb_state *mrb, mrb_value hash, mrb_value key, mrb_value def);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_hash_fetch(IntPtr mrb, UInt64 hash, UInt64 key, UInt64 def);

        // MRB_API mrb_value mrb_hash_delete_key(mrb_state *mrb, mrb_value hash, mrb_value key);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_hash_delete_key(IntPtr mrb, UInt64 hash, UInt64 key);

        // MRB_API mrb_value mrb_hash_keys(mrb_state *mrb, mrb_value hash);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_hash_keys(IntPtr mrb, UInt64 hash);

        // MRB_API mrb_bool mrb_hash_key_p(mrb_state *mrb, mrb_value hash, mrb_value key);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_hash_key_p(IntPtr mrb, UInt64 hash, UInt64 key);

        // MRB_API mrb_bool mrb_hash_empty_p(mrb_state *mrb, mrb_value self);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool mrb_hash_empty_p(IntPtr mrb, UInt64 self);

        // MRB_API mrb_value mrb_hash_values(mrb_state *mrb, mrb_value hash);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_hash_values(IntPtr mrb, UInt64 hash);

        // MRB_API mrb_value mrb_hash_clear(mrb_state *mrb, mrb_value hash);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_hash_clear(IntPtr mrb, UInt64 hash);

        // MRB_API mrb_int mrb_hash_size(mrb_state *mrb, mrb_value hash);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mrb_hash_size(IntPtr mrb, UInt64 hash);

        // MRB_API mrb_value mrb_hash_dup(mrb_state *mrb, mrb_value hash);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 mrb_hash_dup(IntPtr mrb, UInt64 hash);

        // MRB_API void mrb_hash_merge(mrb_state *mrb, mrb_value hash1, mrb_value hash2);
        [DllImport(Ruby.MrubyLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mrb_hash_merge(IntPtr mrb, UInt64 hash1, UInt64 hash2);
    }
}