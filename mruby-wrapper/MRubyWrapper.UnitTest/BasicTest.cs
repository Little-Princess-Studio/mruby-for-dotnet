namespace MRubyWrapper.UnitTest;

using Library;
using Library.Language;

public class BasicTest
{
    [Fact]
    public void TestInstanceMethodAndInstanceVariables()
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

        rbClass.UndefMethod("test");
        respondTo = rbClass.ObjRespondTo("test");
        Assert.False(respondTo);
        
        Ruby.Close(state);
    }

    [Fact]
    void TestClassMethodAndClassVariables()
    {
        var state = Ruby.Open();

        var @class = state.DefineClass("TestClass", null);
        var class2 = state.DefineClass("TestClass2", @class);

        var boxedFloat = state.BoxFloat(12.0);
        var unboxedFloat = state.UnboxFloat(boxedFloat);
        Assert.True(Math.Abs(unboxedFloat - 12.0) < 0.0001);

        @class.DefineClassMethod("class_sub", (stat, self, args) =>
        {
            var arg0 = stat.UnboxFloat(args[0]);
            var arg1 = stat.UnboxFloat(args[1]);
            var res = arg0 - arg1;
            var boxedRes = stat.BoxFloat(res);
            return boxedRes;
        }, RbHelper.MRB_ARGS_REQ(2));

        class2.DefineClassMethod("class_mul", (stat, self, argv) =>
        {
            var arg0 = stat.UnboxFloat(argv[0]);
            var arg1 = stat.UnboxFloat(argv[1]);
            var arg2 = stat.UnboxFloat(argv[2]);
            var res = arg0 * arg1 * arg2;
            var boxedRes = stat.BoxFloat(res);
            return boxedRes;
        }, RbHelper.MRB_ARGS_REQ(3));

        class2.SetClassVariable("@@cls_var", state.BoxInt(123));

        Assert.True(class2.ClassVariableDefined("@@cls_var"));

        var cv = class2.GetClassVariable("@@cls_var");
        var unboxedInt = state.UnboxInt(cv);
        Assert.True(123 == unboxedInt);

        var subRes = @class.CallMethod("class_sub", state.BoxFloat(4.5), state.BoxFloat(0.5));
        var subResUnboxed = state.UnboxFloat(subRes);
        Assert.True(Math.Abs(subResUnboxed - 4.0) < 0.0001);

        var mulRes = @class2.CallMethod("class_mul", state.BoxFloat(2.0), state.BoxFloat(3.0), state.BoxFloat(4.0));
        var mulResUnboxed = state.UnboxFloat(mulRes);
        Assert.True(Math.Abs(mulResUnboxed - 24.0) < 0.0001);

        var subRes2 = @class.CallMethod("class_sub", state.BoxFloat(6.4), state.BoxFloat(6.7));
        var subResUnboxed2 = state.UnboxFloat(subRes2);
        Assert.True(Math.Abs(subResUnboxed2 - -0.3) < 0.0001);

        var classObj = class2.ClassObject;
        var respondTo = classObj.CallMethod("respond_to?",
            state.BoxSymbol(RbHelper.GetInternSymbol(state, "class_mul")));
        Assert.True(respondTo == state.RbTrue);
        
        class2.UndefClassMethod("class_mul");
        
        respondTo = classObj.CallMethod("respond_to?",
            state.BoxSymbol(RbHelper.GetInternSymbol(state, "class_mul")));
        Assert.True(respondTo == state.RbFalse);
        
        Ruby.Close(state);
    }

    [Fact]
    void TestModule()
    {
        var string1 = "str1";
        var string2 = "str2";
        var string3 = "str3";
        var res = $"{string1} - {string2} - {string3}";

        var state = Ruby.Open();

        var module = state.DefineModule("MyModule");
        module.DefineClassMethod("test_with_string", (stat, self, args) =>
        {
            var mod = stat.GetRbClass(self);

            var str1 = stat.UnboxString(args[0]);
            var str2 = stat.UnboxString(args[1]);

            var cv = mod.GetClassVariable("@@mod_var");
            var str3 = stat.UnboxString(cv);

            var boxed = stat.BoxString($"{str1} - {str2} - {str3}");
            return boxed;
        }, RbHelper.MRB_ARGS_REQ(2));

        module.SetClassVariable("@@mod_var", state.BoxString(string3));

        var callRes = module.CallMethod("test_with_string", state.BoxString(string1), state.BoxString(string2));
        var unboxedRes = state.UnboxString(callRes);

        Assert.Equal(res, unboxedRes);

        Ruby.Close(state);
    }
    
    [Fact]
    void TestSingletonMethod()
    {
        var state = Ruby.Open();

        var @class = state.GetClass("Object");
        var obj = @class.NewObject();

        obj.DefineSingletonMethod("test", (stat, self, args) =>
        {
            var arg0 = stat.UnboxInt(args[0]);
            var arg1 = stat.UnboxInt(args[1]);
            var res = arg0 + arg1;
            var boxedRes = stat.BoxInt(res);
            return boxedRes;
        }, RbHelper.MRB_ARGS_REQ(2));

        var res = obj.CallMethod("test", state.BoxInt(1), state.BoxInt(2));
        var unboxedRes = state.UnboxInt(res);

        Assert.Equal(3, unboxedRes);

        Ruby.Close(state);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    void TestModuleMethod(bool anonymousDefine)
    {
        var state = Ruby.Open();

        var module = anonymousDefine ? state.NewModule() : state.DefineModule("MyModule");
        module.DefineModuleMethod("test", (stat, self, args) =>
        {
            var arg0 = stat.UnboxInt(args[0]);
            var arg1 = stat.UnboxInt(args[1]);
            var res = arg0 + arg1;
            var boxedRes = stat.BoxInt(res);
            return boxedRes;
        }, RbHelper.MRB_ARGS_REQ(2));

        var res = module.CallMethod("test", state.BoxInt(1), state.BoxInt(2));
        var unboxedRes = state.UnboxInt(res);

        Assert.Equal(3, unboxedRes);
        
        var @class = anonymousDefine ? state.DefineClass("MyClass", null) : state.NewClass(null);
        @class.IncludeModule(module);
        
        res = module.CallMethod("test", state.BoxInt(3), state.BoxInt(4));
        unboxedRes = state.UnboxInt(res);

        Assert.Equal(7, unboxedRes);
        
        Ruby.Close(state);
    }

    [Fact]
    void TestConstant()
    {
        var state = Ruby.Open();

        var @class = state.DefineClass("MyClass", null);
        
        @class.DefineConstant("CONST1", state.BoxInt(123));
        @class.DefineConstant("CONST2", state.BoxInt(456));
        
        @class.DefineClassMethod("test", (stat, self, args) =>
        {
            var cls = stat.GetClass("MyClass");
            var const1 = cls.GetConst("CONST1");
            var const2 = cls.GetConst("CONST2");
            var res = stat.UnboxInt(const1) + stat.UnboxInt(const2);
            var boxedRes = stat.BoxInt(res);
            return boxedRes;
        }, RbHelper.MRB_ARGS_NONE());
        
        var res = @class.CallMethod("test");
        var unboxedRes = state.UnboxInt(res);
        Assert.Equal(579, unboxedRes);

        var defined = @class.ConstDefined("CONST1");
        Assert.True(defined);
        
        @class.RemoveConst("CONST1");

        defined = @class.ConstDefined("CONST1");
        Assert.False(defined);
        
        Ruby.Close(state);
    }
}