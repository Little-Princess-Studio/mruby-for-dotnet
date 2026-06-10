# mruby-for-dotnet

This is a mruby-wrapper for .NET, current for Windows/Linux/MacOS and will come to other platform soon.

## How to Install

From nuget: https://www.nuget.org/packages/MRuby.Library/

```bash
dotnet add package MRuby.Library --version 0.1.10
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

### macOS: residual reverse-P/Invoke suspension window (library-side mitigation)

On **macOS**, the CoreCLR garbage collector suspends managed threads using POSIX signals.
If a CLR GC suspension signal lands on a managed thread parked inside a native mruby reverse-
P/Invoke callback (for example, a data-object `dfree` during `mrb_close`, or an `initialize`
thunk during object construction), the runtime cannot safely resume it and hard-exits the process.
This is a **residual suspension window** in the .NET 8 reverse-P/Invoke implementation, not
the `dotnet/runtime#44498` stack-corruption race (which was fixed upstream ~.NET 6 and does
not apply to `net8.0`).

The crash probability is driven by two factors:

1. **How often the CLR attempts GC suspensions**, minimized by reducing managed allocations in
   the callback bridge and using `NoGCRegion` in the stress-test storm.
2. **Whether a suspension signal lands on a reverse-P/Invoke boundary**, minimized by
   pre-freeing data-object GCHandles and disarming native `RData` before `mrb_close`, so the
   close-time `dfree` reverse-callback is eliminated entirely.

This surfaces under *sustained, tight* Open/Close churn with managed data objects, e.g. a
tight loop opening and closing many states each iteration. Ordinary usage (a single state, or
open/close scattered among real work) is unaffected. The mruby 4.0 structural GC changes
shifted timing/cadence, changing the **crash probability** vs. 3.3 (not the amount of work done
in teardown). The library-side fix targets both probability factors; macOS CI re-enables the
managed test suite with a statistical acceptance bar (20 consecutive green amplified runs).

Note: this was verified to reproduce on both **.NET 8 and .NET 10** on macOS, so it is not
tied to a specific runtime version. (It is distinct from `dotnet/runtime#102887`, which fixed
a *different* macOS activation-signal case for libdispatch queue threads in .NET 9.) See
`RbConcurrencyTest` and the `StabilizedStormFactAttribute` amplifier for the CI gate design.

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
