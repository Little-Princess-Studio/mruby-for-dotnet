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
        var rbClass2 = state.DefineClass("MyClass2", rbClass);

        // test for rbClass2
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

        // test for rbClass2
        rbClass2.DefineMethod("initialize", (_, self, args) =>
        {
            self.SetInstanceVariable("@a", args[0]);
            self.SetInstanceVariable("@b", args[1]);
            return self;
        }, RbHelper.MRB_ARGS_REQ(2));

        rbClass2.DefineMethod("plus_new", (state, self, args) =>
        {
            // add @a and @b
            var a = self.GetInstanceVariable("@a");
            var b = self.GetInstanceVariable("@b");
            var sum = state.UnboxInt(a) + state.UnboxInt(b);
            var boxedRes = state.BoxInt(sum);
            return boxedRes;
        }, RbHelper.MRB_ARGS_NONE());

        var obj2 = rbClass2.NewObject(state.BoxInt(3), state.BoxInt(4)); 
        var res3 = obj2.CallMethod("plus", state.BoxInt(1), state.BoxInt(2));
        Assert.True(res3 == boxed);
        var res4 = obj2.CallMethod("plus_new");
        Assert.True(res4 == state.BoxInt(7));
        
        Ruby.Close(state);
    }
}