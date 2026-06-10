namespace MRuby.Library
{
    using System;
    using System.Runtime.InteropServices;

    public static partial class Ruby
    {
        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrb_data_disarm(IntPtr mrb, UInt64 obj);
    }
}
