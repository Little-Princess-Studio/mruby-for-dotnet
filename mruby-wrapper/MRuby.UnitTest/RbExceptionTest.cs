namespace MRuby.UnitTest;

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

        var errObj = state.Protect(stat =>
        {
            var excClass = stat.GetExceptionClass("Exception");
            stat.Raise(excClass, errorMsg);
            return stat.RbNil;
        }, ref err);
        Assert.True(err);
        Assert.Equal("Exception", errObj.GetClassName());

        state.Protect(stat => stat.RbNil, ref err);
        Assert.False(err);

        errObj = state.Protect((stat, userdata, _) =>
        {
            Assert.True(stat.RbNil == userdata);
            var excClass = stat.GetExceptionClass("Exception");
            var excObj = stat.GenerateExceptionWithNewStr(excClass, errorMsg);
            stat.Raise(excObj);
            return stat.RbNil;
        }, ref err);
        Assert.True(err);
        Assert.Equal("Exception", errObj.GetClassName());

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

        Ruby.Close(state);
    }

    [Fact]
    void TestRescueAndEnsure()
    {
        var state = Ruby.Open();

        var expCls = state.GetExceptionClass("Exception");
        var clsExc1 = state.DefineClass("Excption1", expCls);

        var excFlag1 = false;
        var rescueFlag1 = false;
        var excFlag2 = false;
        var rescueFlag2 = false;

        var funcThrowExc1 = new CSharpMethodFunc((stat, _, _) =>
        {
            var excClass = stat.GetExceptionClass("StandardError");
            var exc = excClass.NewObject(stat.BoxString(""));
            excFlag1 = true;
            stat.Raise(exc);

            // should not reach here
            excFlag1 = false;
            return stat.RbNil;
        });

        var funcThrowExc2 = new CSharpMethodFunc((stat, _, _) =>
        {
            var exc = clsExc1.NewObject(stat.BoxString(""));
            excFlag2 = true;
            stat.Raise(exc);

            // should not reach here
            excFlag2 = false;
            return stat.RbNil;
        });

        state.Rescue(funcThrowExc1, state.RbNil, (stat, userdata, _) =>
        {
            Assert.True(userdata == stat.RbNil);
            rescueFlag1 = true;
            return stat.RbNil;
        }, state.RbNil);

        state.RescueExceptions(funcThrowExc2, state.RbNil, (stat, userdata, _) =>
        {
            Assert.True(userdata == stat.RbNil);
            rescueFlag2 = true;
            return stat.RbNil;
        }, state.RbNil, new[] { clsExc1 });

        Assert.True(excFlag1);
        Assert.True(excFlag2);
        Assert.True(rescueFlag1);
        Assert.True(rescueFlag2);

        Ruby.Close(state);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    void TestExceptionHandle(int exceptionType)
    {
        var state = Ruby.Open();

        var expCls = state.GetExceptionClass("Exception");
        var clsExc1 = state.DefineClass("Excption1", expCls);
        var clsExc2 = state.DefineClass("Excption2", expCls);
        var clsExc3 = state.DefineClass("Excption3", expCls);

        var clsExc = exceptionType switch
        {
            0 => clsExc1,
            1 => clsExc2,
            2 => clsExc3,
            _ => throw new ArgumentOutOfRangeException(nameof(exceptionType))
        };

        var mainFlag = false;
        var rescueFlag = false;
        var ensureFlag = false;

        var funcThrowExc = new CSharpMethodFunc((stat, userdata, _) =>
        {
            Assert.True(userdata == stat.RbNil);
            var exc = clsExc.NewObject(stat.BoxString(""));
            mainFlag = true;
            stat.Raise(exc);

            // should not reach here
            mainFlag = false;
            return stat.RbNil;
        });

        var funcRescue = new CSharpMethodFunc((stat, userdata, _) =>
        {
            Assert.True(userdata == stat.RbNil);
            rescueFlag = true;
            return stat.RbNil;
        });

        var funcEnsure = new CSharpMethodFunc((stat, userdata, _) =>
        {
            Assert.True(userdata == stat.RbNil);
            ensureFlag = true;
            return stat.RbNil;
        });

        var excClsArray = new[] { clsExc1, clsExc2, clsExc3 };

        state.HandleException(
            funcThrowExc,
            state.RbNil,
            funcRescue,
            state.RbNil,
            excClsArray,
            funcEnsure,
            state.RbNil);

        Assert.True(mainFlag);
        Assert.True(rescueFlag);
        Assert.True(ensureFlag);

        Ruby.Close(state);
    }
}