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
        Assert.False(state.RbNil == null);
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
    
    [Fact]
    void TestDuplicate()
    {
        var state = Ruby.Open();
        
        var obj = state.BoxInt(123);
        var dup = obj.Duplicate();
        
        Assert.True(obj == dup);
        Assert.True(obj.Equals(dup));
        Assert.True(obj.StrictEquals(dup));
        
        var @class = state.DefineClass("MyClass", null);
        @class.DefineMethod("initialize", (stat, self, args) =>
        {
            self.SetInstanceVariable("@a", args[0]);
            return self;
        }, RbHelper.MRB_ARGS_REQ(1));
        
        @class.DefineMethod("eql?", (stat, self, args) =>
        {
            var other = args[0];
            var a = self.GetInstanceVariable("@a");
            var b = other.GetInstanceVariable("@a");
            return a == b ? stat.RbTrue : stat.RbFalse;
        }, RbHelper.MRB_ARGS_REQ(1));
        
        var obj1 = @class.NewObject(state.BoxInt(123));
        var obj2 = obj1.Duplicate();
        
        Assert.True(obj1 == obj2);
        Assert.True(obj1.Equals(obj2));
        Assert.False(obj1.StrictEquals(obj2));
        
        Ruby.Close(state);
    }

    [Fact]
    void TestObjectId()
    {
        var state = Ruby.Open();
        
        var obj = state.BoxInt(123);
        var obj2 = state.BoxInt(123);
        var id = obj.ObjectId;
        var id2 = obj2.ObjectId;
        
        Assert.True(id == id2);
        
        var obj3 = state.GetClass("Object").NewObject();
        var obj4 = state.GetClass("Object").NewObject();
        var id3 = obj3.ObjectId;
        var id4 = obj4.ObjectId;
        
        Assert.True(id3 != id4);
        
        Ruby.Close(state);
    }
}