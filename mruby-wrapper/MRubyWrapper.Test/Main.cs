namespace MRubyWrapper.Test;

using System;
using Library;
using Library.Language;

internal static class Program
{
    public static void Main(string[] args)
    {
        var state = Ruby.Open();
        var rbClass = state.DefineClass("MyClass", null);
        rbClass.DefineMethod(rbClass, "echo", (ptrToState, value) =>
        {
            Console.Out.WriteLine("Callback from Ruby!");
            return 0;
        }, RbHelper.MRB_ARGS_NONE());
        Ruby.Close(state);
    }
}
