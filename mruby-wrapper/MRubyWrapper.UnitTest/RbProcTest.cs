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
            obj.CallMethodWithBlock("add", block);
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

            var idx = self.GetInstanceVariable("@idx");
            
            var block = stat.GetBlock();
            var res = stat.YieldWithClass(block, self, @class, idx);
            Assert.True(res == stat.RbTrue);

            var unboxed = stat.UnboxInt(idx);
            var newIdx = stat.BoxInt(unboxed + 1);
            self.SetInstanceVariable("@idx", newIdx);
            
            return stat.RbNil;
        }, RbHelper.MRB_ARGS_BLOCK());

        var obj = @class.NewObject();
        var block = state.NewProc((stat, self, args) =>
        {
            var a = self.GetInstanceVariable("@a");
            var idx = args[0];
            
            var unboxedIdx = stat.UnboxInt(idx);
            
            var unboxedA = stat.UnboxInt(a);
            var newA = stat.BoxInt(unboxedA + list[unboxedIdx]);
            self.SetInstanceVariable("@a", newA);
            return stat.RbTrue;
        });
        
        for (int i = 0; i < 5; i++)
        {
            obj.CallMethodWithBlock("add", block);
        }
        
        var a = obj.GetInstanceVariable("@a");
        Assert.Equal(15, state.UnboxInt(a));
        
        Ruby.Close(state);
    }

    // [Fact]
    // void TestFiber()
    // {
    //     var state = Ruby.Open();
    //
    //     List<int> toCheck = new List<int>();
    //     
    //     var proc0 = state.NewProc((stat, self, args) =>
    //     {
    //         stat.FiberYield(self);
    //
    //         toCheck.Add(0);
    //         return stat.RbNil;
    //     }, null);
    //     
    //     
    //     var proc1 = state.NewProc((stat, self, args) =>
    //     {
    //         stat.FiberYield(self);
    //
    //         toCheck.Add(1);
    //         return stat.RbNil;
    //     }, null);
    //
    //     var fiber0 = state.NewFiber(proc0);
    //     var fiber1 = state.NewFiber(proc1);
    //
    //     Assert.True(state.CheckFiberAlive(fiber0) == state.RbTrue);
    //     Assert.True(state.CheckFiberAlive(fiber1) == state.RbTrue);
    //     
    //     state.FiberResume(fiber1);
    //     state.FiberResume(fiber0);
    //     
    //     Assert.Equal(new List<int> { 1, 0 }, toCheck);
    //     
    //     Ruby.Close(state);
    // }
}