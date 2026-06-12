# mruby-for-dotnet

This is a mruby-wrapper for .NET, current for Windows/Linux/MacOS and will come to other platform soon.

## How to Install

From nuget: https://www.nuget.org/packages/MRuby.Library/

```bash
dotnet add package MRuby.Library --version 0.2.0
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

### macOS: native crash under concurrent multi-threaded mruby use (test-suite mitigation)

On **macOS**, the CoreCLR garbage collector suspends managed threads using POSIX signals.
The **leading hypothesis** (consistent with our measurements, but not yet crash-dump-confirmed)
is: if a GC suspension signal lands on a managed thread that is parked inside a native mruby
reverse-P/Invoke callback (a `dfree` during `mrb_close`, a method/`initialize` thunk, or a
deeper native path such as mruby's exception `longjmp` or fiber `swapcontext`), the runtime
cannot safely resume it and the process hard-exits. This is distinct from
`dotnet/runtime#44498` (a stack-corruption race fixed upstream ~.NET 6, not applicable to
`net8.0`) and from `dotnet/runtime#102887` (a different .NET 9 libdispatch case). It reproduces
on both .NET 8 and .NET 10.

**What actually drives it (measured in this repo).** The dominant factor is running many mruby
operations **concurrently across threads**, so that several threads sit in native mruby frames
at once and a GC on one can signal-suspend another. A 2×2 control on a macOS-14 runner (full
test suite, 20 runs per arm) measured native test-host crashes of **0/20 when the suite runs
serialized vs 7/20 when it runs with parallel test collections** (Fisher p≈0.008). Code-coverage
collection was only a minor secondary pressure, not the driver.

**Practical impact.** A single `RbState` used from one thread, or independent states each used
on their own thread with the lifecycle scattered among real work, is the normal supported
pattern and is not where this was observed. The crash showed up specifically under *heavy
concurrent* mruby native-callback execution (the parallel test suite). The risk is
probabilistic — a serialized run measured 0/20, but `0/20` means "0 observed", not "0% risk".

**Mitigations.** The test suite is serialized via
`[CollectionBehavior(DisableTestParallelization=true)]` (the load-bearing fix for this crash).
Separately and orthogonally, the library fixes a genuine *managed* data race by locking the two
process-global maps (`StateMapper`, `RbDataClassMapping`), roots callback delegates for the
state lifetime, pre-frees data-object `GCHandle`s, and disarms native `RData` before `mrb_close`
so the close-time `dfree` reverse-callback is removed. See `XunitAssemblyInfo.cs`,
`RbConcurrencyTest`, and `StabilizedStormFactAttribute` for the rationale and the CI design.

## How to Build

The native `libmruby_x64` library is **not** committed to the repository — it is a
build artifact. You must build it from source before the .NET tests can load it.

### One-shot (recommended)

From the repo root, run the script for your platform — each one builds mruby, the
native glue (`mruby-shared` via xmake), then restores/builds/tests/packs the wrapper:

- Windows: `.\build-win.ps1` (run from a *VS x64 Native Tools* prompt; the script
  hardcodes the **Community** edition vcvars path — edit it if you use Pro/Enterprise)
- Linux: `./build-linux.sh`
- macOS: `./build-mac.sh`

Add the `clean` argument (e.g. `./build-linux.sh clean`, `.\build-win.ps1 -clean`)
to wipe the mruby + xmake build cache.

### Manual stages

1. `git submodule update --init --recursive`
2. Build mruby: `./build-mruby-win.bat` (Windows, from a VS x64 Native Tools prompt)
   or `./build-mruby-linux.sh` (Linux) or `./build-mruby-mac.sh` (macOS)
3. `cd mruby-shared`
4. `xmake f -m release`
5. `xmake` (the `after_build` hook copies the native lib next to both .NET projects)
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
