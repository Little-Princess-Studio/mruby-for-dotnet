namespace MRuby.UnitTest;

using Library;
using Library.Language;

public class RbClassTest
{
    [Fact]
    public void TestInstanceMethodAndInstanceVariables()
    {
        var state = Ruby.Open();

        var rbClass = state.DefineClass("MyClass", null);
        var rbClass2 = state.DefineClass("MyClass2", rbClass);

        Assert.True(state.ClassDefined("MyClass"));
        Assert.True(state.ClassDefined("MyClass2"));

        Assert.Equal("MyClass", rbClass.GetClassName());
        Assert.Equal("MyClass2", rbClass2.GetClassName());

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

        Assert.Equal("MyClass", obj.GetClassName());
        Assert.Equal(obj.GetClass().NativeHandler, rbClass.NativeHandler);

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

        rbClass2.DefineMethod("plus_new", (stat, self, args) =>
        {
            // add @a and @b
            var a = self.GetInstanceVariable("@a");
            var b = self.GetInstanceVariable("@b");
            var sum = stat.UnboxInt(a) + stat.UnboxInt(b);
            var boxedRes = stat.BoxInt(sum);
            return boxedRes;
        }, RbHelper.MRB_ARGS_NONE());

        var obj2 = rbClass2.NewObject(state.BoxInt(3), state.BoxInt(4));
        var res3 = obj2.CallMethod("plus", state.BoxInt(1), state.BoxInt(2));
        var obj3 = rbClass2.NewObject(state.BoxInt(8), state.BoxInt(9));

        obj2.CopyInstanceVariables(obj3);
        Assert.True(obj3.GetInstanceVariable("@a") == obj2.GetInstanceVariable("@a"));
        Assert.True(obj3.GetInstanceVariable("@b") == obj2.GetInstanceVariable("@b"));
        
        Assert.False(obj.IsInstanceVariableDefined("@a"));
        Assert.False(obj.IsInstanceVariableDefined("@b"));
        
        Assert.True(obj2.IsInstanceVariableDefined("@a"));
        Assert.True(obj2.IsInstanceVariableDefined("@b"));

        Assert.True(obj2.IsKindOf(rbClass));
        Assert.True(obj2.IsKindOf(rbClass2));
        
        Assert.Equal("MyClass2", obj2.GetClassName());
        Assert.Equal(obj2.GetClass().NativeHandler, rbClass2.NativeHandler);
        
        Assert.True(res3 == boxed);
        var res4 = obj2.CallMethod("plus_new");
        Assert.True(res4 == state.BoxInt(7));

        rbClass.UndefMethod("test");
        respondTo = rbClass.ObjRespondTo("test");
        Assert.False(respondTo);
        
        obj2.RemoveInstanceVariable("@a");
        obj2.RemoveInstanceVariable("@b");

        Assert.False(obj2.IsInstanceVariableDefined("@a"));
        Assert.False(obj2.IsInstanceVariableDefined("@b"));

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
            state.BoxSymbol(state.GetInternSymbol("class_mul")));
        Assert.True(respondTo == state.RbTrue);

        class2.UndefClassMethod("class_mul");

        respondTo = classObj.CallMethod("respond_to?",
            state.BoxSymbol(state.GetInternSymbol("class_mul")));
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

        module = state.GetModule("MyModule");
        callRes = module.CallMethod("test_with_string", state.BoxString(string1), state.BoxString(string2));
        unboxedRes = state.UnboxString(callRes);

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

        @class.SetConst("CONST1", state.BoxInt(789));
        var constVal = @class.GetConst("CONST1");
        Assert.True(constVal == state.BoxInt(789));

        @class.RemoveConst("CONST1");

        defined = @class.ConstDefined("CONST1");
        Assert.False(defined);

        Ruby.Close(state);
    }

    private class MyData
    {
        public Int64 Value { get; set; }
    }

    [Fact]
    void TestNewObjectWithCSharpDataObject()
    {
        var state = Ruby.Open();

        var @class = state.DefineClass("MyData", null);
        @class.DefineMethod("initialize", (stat, self, args) =>
        {
            var value = state.UnboxInt(args[0]);
            var obj = self.GetDataObject<MyData>("MyData")!;
            obj.Value = value;
            return self;
        }, RbHelper.MRB_ARGS_REQ(1));

        @class.DefineMethod("get_value", (stat, self, args) =>
        {
            var obj = self.GetDataObject<MyData>("MyData")!;
            var boxed = stat.BoxInt(obj.Value);
            return boxed;
        }, RbHelper.MRB_ARGS_NONE());

        var myData = new MyData();
        var dataObj = @class.NewObjectWithCSharpDataObject("MyData", myData, state.BoxInt(12345));
        var v = dataObj.CallMethod("get_value");

        var dataObjectType = dataObj.GetDataObjectType();
        Assert.Equal("MyData", dataObjectType.Name);
        
        var unboxed = state.UnboxInt(v);
        Assert.Equal(12345, unboxed);

        Ruby.Close(state);
    }

    [Fact]
    void TestPrependModule()
    {
        var state = Ruby.Open();

        var mod = state.DefineModule("MyModule");
        var cls = state.DefineClass("MyClass", null);

        var val = 0;
        mod.DefineMethod("test_to_override", (stat, self, args) =>
        {
            val = 1;
            return stat.RbNil;
        }, RbHelper.MRB_ARGS_NONE());

        cls.DefineMethod("test_to_override", (stat, self, args) =>
        {
            val = 2;
            return stat.RbTrue;
        }, RbHelper.MRB_ARGS_NONE());

        cls.PrependModule(mod);

        var obj = cls.NewObject();
        var res = obj.CallMethod("test_to_override");

        Assert.True(res == state.RbNil);
        Assert.Equal(1, val);

        Ruby.Close(state);
    }

    [Fact]
    void TestDefineClassAndModuleUnder()
    {
        var state = Ruby.Open();

        var mod = state.DefineModule("MyModule");
        var cls = state.DefineClass("MyClass", null);

        var mod2 = state.DefineModuleUnder(mod, "MyModule2");
        var cls2 = state.DefineClassUnder(cls, "MyClass2", null);

        Assert.True(state.ClassDefinedUnder(mod, "MyModule2"));
        Assert.True(state.ClassDefinedUnder(cls, "MyClass2"));

        Assert.Equal("MyModule::MyModule2", mod2.GetClassName());
        Assert.Equal("MyClass::MyClass2", cls2.GetClassName());

        Assert.True(state.BoxString("MyClass") == cls.GetClassPath());
        Assert.True(state.BoxString("MyClass::MyClass2") == cls2.GetClassPath());
        
        Assert.False(state.ClassDefined("MyClass::MyClass2"));
        Assert.False(state.ClassDefined("MyModule::MyModule2"));

        var cls3 = state.GetClassUnder(cls, "MyClass2");
        Assert.Equal("MyClass::MyClass2", cls3.GetClassName());

        var mod3 = state.GetModuleUnder(mod, "MyModule2");
        Assert.Equal("MyModule::MyModule2", mod3.GetClassName());

        Ruby.Close(state);
    }

    [Fact]
    void TestDefineAlias()
    {
        var state = Ruby.Open();

        var @class = state.DefineClass("MyClass", null);

        var val = 0;
        @class.DefineMethod("test_method", (stat, self, args) =>
        {
            ++val;
            return stat.RbNil;
        }, RbHelper.MRB_ARGS_NONE());

        @class.DefineAlias("alias_method0", "test_method");
        @class.DefineAliasId(state.GetInternSymbol("alias_method1"), state.GetInternSymbol("test_method"));

        var obj = @class.NewObject();
        obj.CallMethod("test_method");
        Assert.Equal(1, val);

        obj.CallMethod("alias_method0");
        Assert.Equal(2, val);

        obj.CallMethod("alias_method1");
        Assert.Equal(3, val);

        Ruby.Close(state);
    }

    [Fact]
    void TestGlobalVariable()
    {
        var state = Ruby.Open();

        var v1 = state.BoxInt(123);
        var v2 = state.BoxString("str");

        state.SetGlobalVariable("$g1", v1);

        var gv2Sym = state.GetInternSymbol("$g2");
        state.SetGlobalVariable(gv2Sym, v2);

        var gv1 = state.GetGlobalVariable("$g1");
        var gv2 = state.GetGlobalVariable(gv2Sym);

        Assert.True(gv1 == v1);
        Assert.True(gv2 == v2);

        state.RemoveGlobalVariable("$g1");
        state.RemoveGlobalVariable(gv2Sym);

        gv1 = state.GetGlobalVariable("$g1");
        gv2 = state.GetGlobalVariable(gv2Sym);

        Assert.True(gv1 == state.RbNil);
        Assert.True(gv2 == state.RbNil);

        Ruby.Close(state);
    }

    [Fact]
    void TestGlobalConstance()
    {
        var state = Ruby.Open();
        
        // state.DefineGlobalConst("G1", state.BoxInt(123));
        
        Ruby.Close(state);
    }
}