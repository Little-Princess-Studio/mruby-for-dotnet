# mruby-for-dotnet

This is a mruby-wrapper for .NET, current for Windows/Linux/MacOS and will come to other platform soon.

## How to Install

From nuget: https://www.nuget.org/packages/MRuby.Library/

```bash
dotnet add package MRuby.Library --version 0.1.9
```

## How to Use

A simple code to embed mruby into C# code.

```csharp
using MRuby.Library

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
using var compiler = state.NewCompiler();
var res = compiler.LoadString(code);

// unbox the ruby value
var unboxed = res.ToString();
Assert.Equal("Hello, World!", unboxed);

```

## Threading

An `RbState` (the value returned by `Ruby.Open()`) is **thread-affine**: mruby has no
GIL, so a single state must never be accessed from more than one thread at a time. You
**may** use *different* `RbState` instances concurrently, each on its own thread. VM
creation and destruction (`Ruby.Open` / `Ruby.Close`) are serialized internally, so it is
safe to open and close states from multiple threads. If you store C# objects in mruby
data objects, their release callback runs during `Ruby.Close`/GC on the thread performing
that close.

### macOS: best-effort under heavy lifecycle churn

On **macOS**, the CoreCLR garbage collector suspends managed threads using POSIX signals.
If the GC suspends a thread that is currently parked inside a native mruby callback (for
example `Ruby.Close` driving `mrb_close`, which calls your data-object release callback
back across the native boundary), the activation signal can land at a point the runtime
cannot safely resume and it hard-exits the process. This is a CoreCLR/macOS limitation in
how it suspends threads stopped in native frames, not a defect in this library.

This only surfaces under *sustained, tight* churn - e.g. opening and closing many states
in a fast loop while allocating managed data objects each iteration. Ordinary usage (a
single state, or open/close scattered among real work) is unaffected. If you do heavy
`Ruby.Open`/`Ruby.Close` cycling on macOS, prefer **reusing a single `RbState`** instead
of rapidly recreating it. The standalone GC (`DOTNET_GCName=libclrgc.dylib`) with
`DOTNET_gcConcurrent=0` reduces - but does not eliminate - the window.

Note: this was verified to reproduce on both **.NET 8 and .NET 10** on macOS, so it is not
tied to a specific runtime version. (It is distinct from dotnet/runtime#102887, which fixed
a *different* macOS activation-signal case for libdispatch queue threads in .NET 9.) The
library's own regression tests deliberately keep this synthetic storm off macOS/Linux CI
for that reason; see `RbConcurrencyTest`.

## How to Build

1. `git submodule update --init --recursive`
2. `./build-mruby-win.bat` (for Windows, run this command under `VS x64 Command Prommpt)` or `./build-mruby-linux.sh` 
   for (*nix) or `./build-mruby-mac.sh` for macos 
3. `cd ../mruby-shared`
4. `xmake f -m releasedbg`
5. `xmake`
6. `cd ../mruby-wrapper`
7. `dotnet build --configuration Release`
8. `dotnet test`

## Status

- [X] 100% Unittest Coverage
- [X] Nuget package
- [X] Support Linux
- [X] Support macOS
- [ ] Unity integral test
- [ ] Support Android
- [ ] Support iOS
- [ ] Documentation
