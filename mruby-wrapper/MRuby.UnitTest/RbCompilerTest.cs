namespace MRuby.UnitTest;

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

        using var compiler = state.NewCompiler();
        var res = compiler.LoadString(code);
        var unboxed = state.UnboxString(res);
        Assert.Equal("Hello, World!", unboxed);
    }

    [Fact]
    void TestParseString()
    {
        var state = Ruby.Open();

        var code = File.ReadAllText("test_scripts/test.rb");
        var context = state.NewCompileContext();
        var compiler = state.NewCompilerWithCodeString(code, context);

        compiler.SetFilename("main");
        var contextFileName = context.SetFilename("main");

        // Assert.Equal("main", contextFileName);
        Assert.Equal("main", compiler.GetFilename(0));

        var proc = compiler.GenerateCode();
        var res = compiler.TopRun(proc, state.TopSelf, 0);
        var unboxed = state.UnboxString(res);

        Assert.Equal("Hello, World!", unboxed);

        context.Dispose();
        compiler.Dispose();
        state.Dispose();
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

        using var compiler = state.NewCompiler();
        using var context = state.NewCompileContext();
        var proc = compiler.LoadString(code, context);
        compiler.LoadString(code2, context);

        var res = proc.CallMethod("call");
        var unboxed = state.UnboxInt(res);
        Assert.Equal(2, unboxed);
    }
}