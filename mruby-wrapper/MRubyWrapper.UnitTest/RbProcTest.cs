namespace MRubyWrapper.UnitTest;

using Library;
using Library.Language;

public class RbProcTest
{
    [Fact]
    public void TestYieldBlockWithoutArgs()
    {
        var state = Ruby.Open();

        var @class = state.DefineClass("MyClass", null);
        @class.DefineMethod("initialize", (stat, self, args) =>
        {
            self.SetInstanceVariable("@a", stat.BoxInt(1));
            return self;
        }, RbHelper.MRB_ARGS_NONE());

        @class.DefineMethod("add", (stat, self, args) =>
        {
            Assert.True(stat.BlockGiven());

            var argsToParse = new RbValue[1];
            stat.GetArgs("&!", ref argsToParse);
            var block = argsToParse[0];
            var res = stat.YieldWithClass(block, self, @class);
            Assert.True(res == stat.RbTrue);
            return stat.RbNil;
        }, RbHelper.MRB_ARGS_BLOCK());

        var obj = @class.NewObject();
        var block = state.NewProc((stat, self, _) =>
        {
            var a = self.GetInstanceVariable("@a");
            var unboxed = stat.UnboxInt(a);
            var newA = stat.BoxInt(unboxed + 1);
            self.SetInstanceVariable("@a", newA);
            return stat.RbTrue;
        });

        for (int i = 0; i < 5; i++)
        {
            obj.CallMethod("add", block);
        }

        var a = obj.GetInstanceVariable("@a");
        Assert.Equal(6, state.UnboxInt(a));

        Ruby.Close(state);
    }

    [Fact]
    public void TestYieldBlockWithArgs()
    {
        var state = Ruby.Open();

        var @class = state.DefineClass("MyClass", null);
        @class.DefineMethod("initialize", (stat, self, args) =>
        {
            self.SetInstanceVariable("@a", stat.BoxInt(0));
            self.SetInstanceVariable("@idx", stat.BoxInt(0));
            return self;
        }, RbHelper.MRB_ARGS_NONE());

        var list = new int[] { 1, 2, 3, 4, 5 };

        @class.DefineMethod("add", (stat, self, args) =>
        {
            Assert.True(stat.BlockGiven());

            bool extra = args.Length == 1;
            
            var idx = self.GetInstanceVariable("@idx");

            var block = stat.GetBlock();
            var res = stat.YieldWithClass(block, self, @class, idx, stat.BoxInt(extra ? 1 : 0));
            Assert.True(res == stat.RbTrue);

            var unboxed = stat.UnboxInt(idx);
            var newIdx = stat.BoxInt(unboxed + 1);
            self.SetInstanceVariable("@idx", newIdx);

            return stat.RbNil;
        }, RbHelper.MRB_ARGS_ANY() | RbHelper.MRB_ARGS_BLOCK());

        CSharpMethodFunc blockFunc = (stat, self, args) =>
        {
            var a = self.GetInstanceVariable("@a");
            var idx = args[0];
            var extra = stat.UnboxInt(args[1]);

            var unboxedIdx = stat.UnboxInt(idx);

            var unboxedA = stat.UnboxInt(a);
            var newA = stat.BoxInt(unboxedA + list[unboxedIdx] + extra);
            self.SetInstanceVariable("@a", newA);
            return stat.RbTrue;
        };
        
        var block0 = state.NewProc(blockFunc);

        var obj1 = @class.NewObject();
        for (int i = 0; i < 5; i++)
        {
            obj1.CallMethod("add", block0);
        }
        var a1 = obj1.GetInstanceVariable("@a");
        Assert.Equal(15, state.UnboxInt(a1));

        var obj2 = @class.NewObject();
        for (int i = 0; i < 5; i++)
        {
            obj2.CallMethod("add", block0, state.RbTrue);
        }
        var a2 = obj2.GetInstanceVariable("@a");
        Assert.Equal(20, state.UnboxInt(a2));
        
        Ruby.Close(state);
    }

    [Fact]
    void TestSetProcToRuby()
    {
        using var state = Ruby.Open();
        using var compiler = RbCompiler.ParserNew(state);
        using var context = RbCompiler.NewContext(state);

        var proc = state.NewProc((stat, self, args) =>
        {
            var val = stat.UnboxInt(args[0]);
            return stat.BoxInt(1 + val);
        });
        state.SetGlobalVariable("$proc", proc.ToRbValue());

        var code = "$proc.call 1";
        var res = compiler.LoadString(code);

        Assert.Equal(2, state.UnboxInt(res));
    }

    [Fact]
    void TestFiber()
    {
        using var state = Ruby.Open();
        using var compiler = RbCompiler.ParserNew(state);
        using var context = RbCompiler.NewContext(state);
        string code = File.ReadAllText("test_scripts/fiber_test.rb");

        compiler.LoadString(code, context);
        var fiberProc0 = state.GetGlobalVariable("$p0");
        var fiberProc1 = state.GetGlobalVariable("$p1");

        // the proc of a fiber must be a ruby-side proc not c-side proc
        var fiber0 = state.NewFiber(RbProc.FromRbValue(fiberProc0));
        var fiber1 = state.NewFiber(RbProc.FromRbValue(fiberProc1));

        Assert.True(state.CheckFiberAlive(fiber0) == state.RbTrue);
        Assert.True(state.CheckFiberAlive(fiber1) == state.RbTrue);

        Int64 cnt0 = 0;
        Int64 cnt1 = 0;
        for (int i = 0; i < 2; ++i)
        {
            var res = state.FiberResume(fiber0);
            var unboxed = state.UnboxInt(res);
            cnt0 += unboxed;
        }
        Assert.Equal(3, cnt0);

        for (int i = 0; i < 2; ++i)
        {
            var res = state.FiberResume(fiber1, state.BoxInt(1));
            var unboxed = state.UnboxInt(res);
            cnt1 += unboxed;
        }
        Assert.Equal(8, cnt1);
    }
}