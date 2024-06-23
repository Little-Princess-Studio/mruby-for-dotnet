namespace MRubyWrapper.UnitTest;

using Library;
using Library.Language;

public class BasicTest
{
    [Fact]
    public void TestCreateClassAndCallInstanceMethod()
    {
        var state = Ruby.Open();
        RbHelper.Init(state);

        var rbClass = state.DefineClass("MyClass", null);

        var setVal = false;
        rbClass.DefineMethod(rbClass, "test", (self, argv) =>
        {
            setVal = true;
            return RbValue.RbNil;
        }, RbHelper.MRB_ARGS_NONE());

        var obj = rbClass.NewObject();
        obj.CallMethod("test");

        Assert.True(setVal);

        var res = rbClass.ObjRespondTo("test");
        Assert.True(res);

        Ruby.Close(state);
    }
}