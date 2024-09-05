# mruby-for-dotnet

This is a mruby-wrapper for .NET, current for Windows and will come to other platform soon.

## How to Use

A simple code to embed mruby into C# code.

```csharp
using MRubyWrapper.Library

// create ruby env
using var state = Ruby.Open();

// ruby code string
var code = @"
def hello
  'Hello, World!'
end

hello
";

// compile code, run and get the result
using var compiler = RbCompiler.ParserNew(state);
var res = compiler.LoadString(code);

// unbox the ruby value
var unboxed = state.UnboxString(res);
Assert.Equal("Hello, World!", unboxed);

```

## How to Build

1. `git submodule update --init --recursive`
2. `./build-mruby.bat` (for Windows, run this command under `VS x64 Command Prommpt)` or `./build-mruby.sh` for (*nix)
3. `cd ../mruby-win-shared`
4. `xmake f-m releasedbg`
5. `xmake`
6. `cd ../mruby-wrapper`
7. `dotnet build --configuration Release`
8. `dotnet test`

## Status

- [X] 100% Unittest Coverage
- [X] Nuget package
- [ ] Unity integral test
- [ ] Test on Linux, macOS
- [ ] Test on Android
