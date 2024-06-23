namespace MRubyWrapper.Test;

using System;
using Library;
using Library.Language;

internal static class Program
{
    public static void Main(string[] args)
    {
        var state = Ruby.Open();
        
        RbHelper.Init(state);
        
        var rbClass = state.DefineClass("MyClass", null);
        rbClass.DefineMethod(rbClass, "echo", (self, argv) =>
        {
            Console.Out.WriteLine("Callback from Ruby!");
            return RbValue.RbNil;
        }, RbHelper.MRB_ARGS_NONE());
        var obj = rbClass.NewObject();
        obj.CallMethod("echo");
        Ruby.Close(state);
    }
}
