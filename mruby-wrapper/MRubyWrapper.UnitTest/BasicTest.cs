namespace MRubyWrapper.UnitTest;

using Library;
using Library.Language;

public class BasicTest
{
    [Fact]
    public void TestInstanceMethodForClass()
    {
        var state = Ruby.Open();

        var rbClass = state.DefineClass("MyClass", null);

        var setVal = false;
        rbClass.DefineMethod("test", (_, self, argv) =>
        {
            setVal = true;
            return state.RbNil;
        }, RbHelper.MRB_ARGS_NONE());

        rbClass.DefineMethod("plus", (state, self, args) =>
        {
            var a = state.UnboxInt(args[0]);
            var b = state.UnboxInt(args[1]);
            var sum = a + b;
            var boxed = state.BoxInt(sum);
            return boxed;
        }, RbHelper.MRB_ARGS_REQ(2));
        
        var obj = rbClass.NewObject();
        var res = obj.CallMethod("test");

        Assert.True(setVal);
        Assert.True(res == state.RbNil);

        var res2 = obj.CallMethod("plus", state.BoxInt(1), state.BoxInt(2));
        var boxed = state.BoxInt(3);

        Assert.True(res2 == boxed);
        Assert.True(res2.StrictEquals(boxed));
        
        var respondTo = rbClass.ObjRespondTo("test");
        Assert.True(respondTo);

        Ruby.Close(state);
    }

    [Fact]
    public void TestClassMethod()
    {
        
    }
}