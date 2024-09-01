namespace MRubyWrapper.UnitTest;

using Library;
using Library.Language;

public class RbExceptionTest
{
    [Fact]
    void TestRaiseProtect()
    {
        var state = Ruby.Open();
        var errorMsg = "Error message";

        bool err = false;

        state.Protect(stat =>
        {
            var excClass = stat.GetExceptionClass("Exception");
            stat.Raise(excClass, errorMsg);
            return stat.RbNil;
        }, ref err);
        Assert.True(err);

        state.Protect(stat => stat.RbNil, ref err);
        Assert.False(err);
        
        state.Protect((stat, userdata, _) =>
        {
            Assert.True(stat.RbNil == userdata);
            var excClass = stat.GetExceptionClass("Exception");
            var excObj = excClass.NewObject(stat.BoxString(errorMsg));
            stat.Raise(excObj);
            return stat.RbNil;
        }, ref err);
        Assert.True(err);
        
        var @class = state.DefineClass("TestClass", null);
        @class.DefineMethod("initialize", (stat, self, args) =>
        {
            self.SetInstanceVariable("@a", stat.BoxString(""));
            return self;
        }, RbHelper.MRB_ARGS_NONE());
        var obj = @class.NewObject();

        state.Protect((stat, userdata, _) =>
        {
            Assert.Equal("TestClass", userdata.GetClassName());
            userdata.SetInstanceVariable("@a", stat.BoxString(errorMsg));
            return stat.RbNil;
        }, obj, ref err);
        Assert.False(err);

        var str = state.UnboxString(obj.GetInstanceVariable("@a"));
        Assert.Equal(errorMsg, str);
        
        Assert.False(state.CheckError());
        Ruby.Close(state);
    }
}

