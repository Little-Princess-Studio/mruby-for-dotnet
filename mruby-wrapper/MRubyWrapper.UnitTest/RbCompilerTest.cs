namespace MRubyWrapper.UnitTest;

using Library;
using Library.Language;

public class RbCompilerTest
{
    [Fact]
    void TestCompileCodeString()
    {
        using var state = Ruby.Open();
        var code = @"
            def hello
                'Hello, World!'
            end

            hello
         ";

        using var compiler = RbCompiler.ParserNew(state);
        var res = compiler.LoadString(code);
        var unboxed = state.UnboxString(res);
        Assert.Equal("Hello, World!", unboxed);
    }

    [Fact]
    void TestParseString()
    {
        using var state = Ruby.Open();

        var code = File.ReadAllText("test_scripts/test.rb");
        using var context = RbCompiler.NewContext(state);
        using var compiler = RbCompiler.ParseString(state, code, context);
        compiler.SetFilename("main");
        Assert.Equal("main", compiler.GetFilename(0));

        var proc = compiler.GenerateCode();
        var res = compiler.TopRun(proc, state.GetTopSelf(), 0);
        var unboxed = state.UnboxString(res);

        Assert.Equal("Hello, World!", unboxed);
    }

    [Fact]
    void TestParseWithContext()
    {
        using var state = Ruby.Open();

        var code = @"
            x = 1;
            proc { x }
         ";

        var code2 = @"
            x = 2;
         ";

        using var compiler = RbCompiler.ParserNew(state);
        using var context = RbCompiler.NewContext(state);
        var proc = compiler.LoadString(code, context);
        compiler.LoadString(code2, context);

        var res = proc.CallMethod("call");
        var unboxed = state.UnboxInt(res);
        Assert.Equal(2, unboxed);
    }
}