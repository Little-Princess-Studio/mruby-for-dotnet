namespace MRubyWrapper.UnitTest;

using Library;
using Library.Language;

public class RbValueTest
{
    [Fact]
    public void TestRbValueEquals()
    {
        using var state = Ruby.Open();

        RbValue? obj = null;

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
    }

    [Fact]
    void TestDuplicateAndClone()
    {
        using var state = Ruby.Open();

        var cls = state.GetClass("Object");
        var obj = cls.NewObject();

        obj.Freeze();
        Assert.True(obj.CallMethod("frozen?") == state.RbTrue);
        Assert.True(obj.CheckFrozen());

        var dup = obj.Duplicate();
        Assert.False(dup.CallMethod("frozen?") == state.RbTrue);
        Assert.False(dup.CheckFrozen());

        var clone = obj.Clone();
        Assert.True(clone.CallMethod("frozen?") == state.RbTrue);
        Assert.True(clone.CheckFrozen());

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
        var obj3 = obj1.Clone();

        Assert.True(obj1 == obj2);
        Assert.True(obj1.Equals(obj2));
        Assert.False(obj1.StrictEquals(obj2));

        Assert.True(obj1 == obj3);
        Assert.True(obj1.Equals(obj3));
        Assert.False(obj1.StrictEquals(obj3));
    }

    [Fact]
    void TestObjectId()
    {
        using var state = Ruby.Open();

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
    }

    [Fact]
    void TestStringAndSymbol()
    {
        using var state = Ruby.Open();

        var csharpString = "Hello, World!";

        var str = state.NewRubyString(csharpString);
        var sym0 = state.GetInternSymbol(csharpString);
        var sym1 = state.GetInternSymbol(csharpString);

        Assert.Equal(sym0, sym1);

        var symbolNameVal = state.GetSymbolStr(sym0);
        Assert.True(str == symbolNameVal);

        var symbolNameStr = state.GetSymbolName(sym0);
        Assert.Equal(csharpString, symbolNameStr);
    }

    [Fact]
    void TestSymbolBoxingAndUnboxing()
    {
        using var state = Ruby.Open();
        var csharpString = "Hello, World!";

        var sym0 = state.GetInternSymbol(csharpString);
        var sym0Val = state.BoxSymbol(sym0);
        var sym1 = state.GetInternSymbol(csharpString);
        var sym1Val = state.BoxSymbol(sym1);

        Assert.True(sym0Val == sym1Val);

        var sym0Unboxed = state.UnboxSymbol(sym0Val);
        var sym1Unboxed = state.UnboxSymbol(sym1Val);

        Assert.Equal(sym0, sym0Unboxed);
        Assert.Equal(sym1, sym1Unboxed);
    }

    [Fact]
    void TestTopSelf()
    {
        using var state0 = Ruby.Open();
        using var state1 = Ruby.Open();

        var top0 = state0.TopSelf;
        var top1 = state1.TopSelf;

        Assert.True(top0 != top1);

        var a = false;
        top0.DefineSingletonMethod("test", (stat, self, args) =>
        {
            a = true;
            return stat.RbNil;
        }, RbHelper.MRB_ARGS_NONE());

        top0.CallMethod("test");

        Assert.True(a);

        state0.DefineGlobalConst("ABC", state0.BoxString("HHH"));
        var abc0 = state0.GetGlobalConst("ABC");
        var abc0Unboxed = state0.UnboxString(abc0);

        Assert.Equal("HHH", abc0Unboxed);
    }

    [Fact]
    void TestIvForeach()
    {
        using var state = Ruby.Open();

        var cls = state.DefineClass("MyClass", null);
        cls.DefineMethod("initialize", (stat, self, args) =>
        {
            self.SetInstanceVariable("@a", stat.BoxInt(1));
            self.SetInstanceVariable("@b", stat.BoxInt(2));
            self.SetInstanceVariable("@c", stat.BoxInt(3));
            return self;
        }, RbHelper.MRB_ARGS_NONE());

        var obj = cls.NewObject();

        var nameDict = new Dictionary<string, int>
        {
            ["@a"] = 1,
            ["@b"] = 2,
            ["@c"] = 3,
        };

        var callback = new CSharpIvForeachFunc(((stat, name, val) =>
        {
            if (!nameDict.ContainsKey(name))
            {
                return stat.RbNil;
            }

            var valInt = stat.UnboxInt(val);
            var compared = nameDict[name];

            if (valInt != compared)
            {
                return stat.RbNil;
            }

            nameDict.Remove(name);
            
            return stat.RbNil;
        }));

        obj.InstanceVariableForeach(callback);
        
        Assert.Empty(nameDict);
    }

    [Fact]
    void TestCompare()
    {
        using var state = Ruby.Open();
        
        var int1 = state.BoxInt(0);
        var int2 = state.BoxInt(0);
        var int3 = state.BoxInt(2);

        Assert.True(int1.Compare(int2) == 0);
        Assert.True(int1.Compare(int3) < 0);
        Assert.True(int3.Compare(int1) > 0);
    }
    
    [Fact]
    void TestSingletonMethod()
    {
        using var state = Ruby.Open();
        
        var cls = state.DefineClass("MyClass", null);
        var obj = cls.NewObject();
        var singletonClass = obj.SingletonClass;
        
        var a = false;
        singletonClass.DefineMethod("test", (stat, self, args) =>
        {
            a = true;
            return stat.RbNil;
        }, RbHelper.MRB_ARGS_NONE());
        
        obj.CallMethod("test");
        
        Assert.True(a);
    }
}