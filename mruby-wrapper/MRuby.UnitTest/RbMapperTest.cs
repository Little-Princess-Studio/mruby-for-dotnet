namespace MRuby.UnitTest;

using Library;
using Library.Language;
using Library.Mapper;

public class RbMapperTest
{
    [RbClass("TestClass", "Object", "")]
    public static class TestClass
    {
        public static bool Initialized { get; private set; }

        [RbClassMethod("test_class_method0", true)]
        public static RbValue TestClassMethod0(RbState state, RbValue self)
        {
            Assert.True(state.BlockGiven());
            var block = state.GetBlock();
            var res = state.YieldWithClass(block, self, self.GetClass());
            return res;
        }

        [RbClassMethod("test_class_method1")]
        public static RbValue TestClassMethod1(RbState state, RbValue self, RbValue arg0)
        {
            return arg0;
        }

        [RbClassMethod("test_class_method2")]
        public static RbValue TestClassMethod2(RbState state, RbValue self, RbValue[] args)
        {
            return args[0];
        }

        [RbClassMethod("test_class_method3")]
        public static RbValue TestClassMethod3(RbState state, RbValue self, RbValue arg0, RbValue[] args)
        {
            return state.BoxInt(state.UnboxInt(arg0) + state.UnboxInt(args[0]));
        }

        [RbInstanceMethod("test_instance_method0", true)]
        public static RbValue TestInstanceMethod0(RbState state, RbValue self)
        {
            Assert.True(state.BlockGiven());
            var block = state.GetBlock();
            var res = state.YieldWithClass(block, self, self.GetClass());
            return res;
        }

        [RbInstanceMethod("test_instance_method1")]
        public static RbValue TestInstanceMethod1(RbState state, RbValue self, RbValue arg0)
        {
            return arg0;
        }

        [RbInstanceMethod("test_instance_method2")]
        public static RbValue TestInstanceMethod2(RbState state, RbValue self, RbValue[] args)
        {
            return args[0];
        }

        [RbInstanceMethod("test_instance_method3")]
        public static RbValue TestInstanceMethod3(RbState state, RbValue self, RbValue arg0, RbValue[] args)
        {
            return state.BoxInt(state.UnboxInt(arg0) + state.UnboxInt(args[0]));
        }

        [RbInitEntryPoint]
        public static void Init(RbClass cls)
        {
            Initialized = true;
        }
    }

    [RbModule("TestModule", "")]
    public static class TestModule
    {
        public static bool Initialized { get; private set; }
        
        [RbModuleMethod("test_module_method0", true)]
        public static RbValue TestModuleMethod0(RbState state, RbValue self)
        {
            Assert.True(state.BlockGiven());
            var block = state.GetBlock();
            var res = state.YieldWithClass(block, self, self.GetClass());
            return res;
        }

        [RbModuleMethod("test_module_method1")]
        public static RbValue TestModuleMethod1(RbState state, RbValue self, RbValue arg0)
        {
            return arg0;
        }

        [RbModuleMethod("test_module_method2")]
        public static RbValue TestModuleMethod2(RbState state, RbValue self, RbValue[] args)
        {
            return args[0];
        }

        [RbModuleMethod("test_module_method3")]
        public static RbValue TestModuleMethod3(RbState state, RbValue self, RbValue arg0, RbValue[] args)
        {
            return state.BoxInt(state.UnboxInt(arg0) + state.UnboxInt(args[0]));
        }

        [RbInitEntryPoint]
        public static void Init(RbClass cls)
        {
            Initialized = true;
        }
    }

    [Fact]
    void TestClassMapping()
    {
        using var state = Ruby.Open();
        RbTypeRegisterHelper.Init(state, new []{typeof(TestClass).Assembly});
        
        Assert.True(TestClass.Initialized);

        var cls = state.GetClass("TestClass");
        var obj = cls.NewObject();

        var block = state.NewProc((stat, self, args) => stat.BoxInt(42), out var blk);

        var res = obj.CallMethodWithBlock("test_instance_method0", block);
        Assert.Equal(42, state.UnboxInt(res));

        res = obj.CallMethod("test_instance_method1", state.BoxInt(42));
        Assert.Equal(42, state.UnboxInt(res));

        res = obj.CallMethod("test_instance_method2", state.BoxInt(42));
        Assert.Equal(state.BoxInt(42), res);

        res = obj.CallMethod("test_instance_method3", state.BoxInt(42), state.BoxInt(24));
        Assert.Equal(state.BoxInt(66), res);
        
        res = cls.CallMethodWithBlock("test_class_method0", block);
        Assert.Equal(42, state.UnboxInt(res));

        res = cls.CallMethod("test_class_method1", state.BoxInt(42));
        Assert.Equal(42, state.UnboxInt(res));

        res = cls.CallMethod("test_class_method2", state.BoxInt(42));
        Assert.Equal(state.BoxInt(42), res);

        res = cls.CallMethod("test_class_method3", state.BoxInt(42), state.BoxInt(24));
        Assert.Equal(state.BoxInt(66), res);
        
        GC.KeepAlive(blk);
    }

    [Fact]
    void TestModuleMappint()
    {
        using var state = Ruby.Open();
        RbTypeRegisterHelper.Init(state, new []{typeof(TestModule).Assembly});
        Assert.True(TestModule.Initialized);

        var cls = state.GetModule("TestModule");
        var block = state.NewProc((stat, self, args) => stat.BoxInt(42), out _);

        var res = cls.CallMethodWithBlock("test_module_method0", block);
        Assert.Equal(42, state.UnboxInt(res));

        res = cls.CallMethod("test_module_method1", state.BoxInt(42));
        Assert.Equal(42, state.UnboxInt(res));

        res = cls.CallMethod("test_module_method2", state.BoxInt(42));
        Assert.Equal(state.BoxInt(42), res);

        res = cls.CallMethod("test_module_method3", state.BoxInt(42), state.BoxInt(24));
        Assert.Equal(state.BoxInt(66), res);
    }
}