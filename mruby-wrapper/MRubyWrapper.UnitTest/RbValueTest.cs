namespace MRubyWrapper.UnitTest;

using Library;
using Library.Language;

public class RbValueTest
{
    [Fact]
    public void TestRbValueEquals()
    {
        var state = Ruby.Open();

        var obj = null as Object;
        
        Assert.False(obj == state.RbNil);
        Assert.False(state.RbNil.Equals(obj));
        
        Assert.True(state.BoxInt(123) == state.BoxInt(123));
        Assert.True(state.BoxInt(123).Equals(state.BoxInt(123)));
        Assert.True(state.BoxInt(123).StrictEquals(state.BoxInt(123)));

        Assert.False(state.BoxInt(123) == state.BoxInt(321));
        Assert.False(state.BoxInt(123).Equals(state.BoxInt(321)));
        Assert.False(state.BoxInt(123).StrictEquals(state.BoxInt(321)));

        var @class = state.DefineClass("MyClass", null);
        @class.DefineMethod("initialize", (stat, self, args) =>
        {
            self.SetInstanceVariable("@a", stat.BoxInt(1));
            return self;
        }, RbHelper.MRB_ARGS_NONE());
        
        @class.DefineMethod("eql?", (stat, self, args) =>
        {
            var other = args[0];
            var a = self.GetInstanceVariable("@a");
            var b = other.GetInstanceVariable("@a");
            return a == b ? stat.RbTrue : stat.RbFalse;
        }, RbHelper.MRB_ARGS_REQ(1));
        
        var obj1 = @class.NewObject();
        var obj2 = @class.NewObject();
        
        Assert.True(obj1 == obj2);
        Assert.True(obj1.Equals(obj2));
        Assert.False(obj1.StrictEquals(obj2));
        
        Ruby.Close(state);
    }
}