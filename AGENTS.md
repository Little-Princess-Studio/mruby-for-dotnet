# AGENTS.md

`mruby-for-dotnet` is a C#/.NET binding for the embeddable **mruby** interpreter (Windows/Linux/macOS), published to NuGet as `MRuby.Library`.

## Repository layout

- `mruby/` — **git submodule** (upstream mruby, pinned to 4.0.0). Do NOT edit; it is vendored. Run `git submodule update --init --recursive` before any build.
- `mruby-shared/` — thin **native C glue** (`src/main.c` + `main.h`) compiled with **xmake** into `libmruby_x64.{dll,so,dylib}`. It statically links mruby and exposes extra boxing/unboxing/type-check helpers the wrapper needs.
- `mruby-wrapper/` — the actual **.NET code** (`MRubyWrapper.sln`):
  - `MRuby.Library/` — the published wrapper library.
  - `MRuby.UnitTest/` — xUnit tests.
- Root `build-*.sh/.ps1/.bat` scripts + `costumized-build-conf-*.rb` — the multi-stage build (see below).

## Build & test (stage order is mandatory)

You **cannot** just `dotnet build`. The native lib must be built first (stages 1→2) and dropped next to the .NET projects, or the build/tests fail at load time. Build mruby (rake) → mruby-shared (xmake) → wrapper (dotnet).

Prerequisites: Ruby 3.3 + rake, **xmake 2.9.6**, **.NET SDK 8.0**. Plus per-OS C toolchain: Windows=MSVC (VS 2022), Linux=`build-essential` (gcc), macOS=clang+lipo.

**One-shot (preferred):** from repo root run `./build-linux.sh`, `./build-mac.sh`, or `.\build-win.ps1`. Each runs the full pipeline (mruby → xmake → `dotnet restore/build/test/pack`). Clean with the `clean` arg (e.g. `./build-linux.sh clean`, `.\build-win.ps1 -clean`).

**Manual stages** (for rebuilding just one layer):
1. mruby: `cd mruby && rake MRUBY_CONFIG=../costumized-build-conf-<os>.rb all test` (see macOS quirk below).
2. native lib: `cd mruby-shared && xmake f -m release && xmake`.
3. wrapper: `cd mruby-wrapper && dotnet restore && dotnet build -c Release && dotnet test -c Release`.

**Single test:** `cd mruby-wrapper && dotnet test --filter "FullyQualifiedName~RbClassTest"` (or `~TestInstanceMethodAndInstanceVariables` for one method).

**Native lib placement is automatic:** `mruby-shared/xmake.lua`'s `after_build` hook copies the built lib into BOTH `mruby-wrapper/MRuby.Library/` and `mruby-wrapper/MRuby.UnitTest/`. If you build xmake from the wrong cwd those relative copies break and tests fail with a `DllNotFoundException` for `libmruby_x64`. The `.csproj` files only bundle the lib via `Condition="Exists(...)"`, so a missing file fails silently at pack time, not build time.

CI (`.github/workflows/main.yml`, PRs to `main` only): Linux+macOS build in parallel and upload their `.so`/`.dylib`; the Windows job `needs` them, downloads both into `MRuby.Library/`, then `dotnet pack` — that is how one NuGet package gets all three native libs.

## Wrapper architecture (`mruby-wrapper`)

Core flow: `using var state = Ruby.Open();` → `using var compiler = state.NewCompiler();` → `compiler.LoadString(code)` returns an `RbValue` → unbox via `state.UnboxString(value)` / `value.ToString()`.

- **Partial-class split is the project-wide pattern.** Every wrapper type lives in `Foo.cs` (managed logic) + `Foo.Decl.cs` (ONLY `[DllImport] private static extern` declarations). `RbState` exceptions get a further `RbState.Exception.cs` + `RbState.Decl.Exception.cs` pair. When adding a native binding, follow this split — put the extern in the `.Decl.cs`.
- **Single native-lib name constant:** `Ruby.MrubyLib = "libmruby_x64"` in `Ruby.cs`. Every `[DllImport]` references it. `.NET`'s default OS search path resolves it (no custom resolver).
- **Interop primitives:** an mruby `mrb_value` marshals as **`UInt64`** (NaN-boxed tagged union); any `struct R*` pointer (`mrb_state*`, `RClass*`, `RProc*`) marshals as **`IntPtr`**. `mrb_bool` returns need `[return: MarshalAs(UnmanagedType.U1)]`. Ruby code strings use `UnmanagedType.LPUTF8Str`.
- **IDisposable applies ONLY to** `RbState` (`mrb_close`), `RbCompiler`, and `RbContext`. `RbValue`/`RbClass`/`RbArray`/`RbHash`/`RbProc` are non-owning handles into mruby's GC heap — do not dispose them.
- **Delegate lifetime:** C# method callbacks (`CSharpMethodFunc(RbState, RbValue self, params RbValue[] args)`) handed to native `mrb_define_method` etc. hold mruby only a raw function pointer, so the managed `NativeMethodFunc` is GC-collected unless rooted. **The library auto-roots them**: every define-path (`RbClass.DefineMethod`/`DefinePrivateMethod`/`DefineClassMethod`/`DefineModuleMethod`, `RbValue.DefineSingletonMethod`, `RbState.NewProc`, `RbState` Protect/Ensure/Rescue) builds via `RbHelper.BuildAndRootNativeCallback`, which pins the delegate into the per-state `RbCallbackKeeper` until `Ruby.Close`. The `out NativeMethodFunc` is still returned for compatibility but callers **no longer need to keep it alive** (the idiomatic `out _` is safe). Skipping the rooting was the real macOS CI crash: discarded delegates were collected while mruby still held their pointers, and a later call jumped through freed memory → hard native test-host abort (GC-timing dependent, so it fired on the 3-core macOS runner but not on faster dev boxes). Attribute-based registration additionally roots into `RbAutoRegisterKeeper`.
- **Threading contract:** an `RbState`/`mrb_state` is **thread-affine** — mruby has no GIL, so a single state must never be touched by two threads at once. **Distinct** `RbState`s may be used concurrently on their own threads. VM **lifecycle is serialized internally**: `Ruby.Open`/`Ruby.Close` take a process-wide `VmLifecycleLock` around `mrb_open`/`mrb_close` (teardown runs a final GC that calls managed data-object free callbacks back across the boundary). The two process-global wrapper maps — `RbNativeObjectLiveKeeper.StateMapper` and `RbHelper.RbDataClassMapping` — are mutated on every state setup/teardown and are guarded by their own locks (`StateMapperLock`, `RbDataClassMappingLock`); an unsynchronized `Dictionary` here corrupts under xUnit's parallel test collections and surfaces as a hard native crash, so any process-`static` mutable map touched during open/close MUST be locked the same way. Note: forcing a `GC.Collect` from one test while **other** test classes are executing inside native mruby callbacks hard-exits the macOS/Linux test host (signal-based GC thread-suspension can't safely suspend threads parked in native code) — this is a test-host limit, not a library bug, so GC-storm and heavy multi-thread regression tests use `[WindowsOnlyFact]` (stable + detective on Windows). The ordinary parallel suite is green on all platforms once delegates are rooted and the maps are locked.
- **Attribute-based registration:** decorate a class with `[RbClass]`/`[RbModule]`, tag `static` methods with `[RbClassMethod]`/`[RbInstanceMethod]`/`[RbModuleMethod]`, optionally `[RbInitEntryPoint]` on a `static Init(RbClass)`, then call `RbTypeRegisterHelper.Init(state, assemblies)` to auto-define them (`MRuby.Library.Mapper`).

Namespaces: `MRuby.Library` (entrypoint), `MRuby.Library.Language` (wrapper types), `MRuby.Library.Mapper` (attributes). `MRuby.Library` has `ImplicitUsings=disable` + `AllowUnsafeBlocks=true`, multi-targets `net8.0;netstandard2.1`; tests target `net8.0` only. `Nullable=enable` everywhere. All wrapper types are `Rb`-prefixed.

## Native layer (`mruby-shared`)

If the wrapper needs a native function mruby doesn't conveniently export, add it to `src/main.c` + `src/main.h` (this is where the `*_boxing`/`*_unboxing`/`mrb_check_type_*` helpers live). **For Windows you must also add the symbol to `tools/mruby_x64.def`** — that `.def` explicitly lists every exported symbol (regen via `tools/create_def.rb`, which uses `ctags`). xmake includes headers from `mruby/build/host/include`, which only exists after stage 1.

**Upgrading the mruby submodule:** the `.def` is the fragile part. A symbol listed there that the new mruby no longer exports (removed, or turned into a macro/`static inline`) becomes a Windows-only `LNK2001` at `xmake` link time — it does NOT show on Linux/macOS (those use `.a` whole-archive links, not an export list). After a bump, diff every `.def` line against the new headers and drop/rename stale ones. Example seen in the 3.3→4.0 bump: `mrb_default_allocf` + `mrb_open_allocf` were removed and `mrb_alloca` became a compat macro (`→ mrb_temp_alloc`). The `mrb_*_boxing` glue itself stayed source-compatible (word boxing is still the x64 default; `mrb_word_boxing_float_value`, `mrb_ci_bidx`, `Data_Wrap_Struct`, etc. are unchanged).

## Gotchas

- **`costumized-build-conf-*.rb` is a deliberate misspelling** (no "u"). Build scripts hardcode this exact name — do not "fix" it.
- **macOS build is different:** `build-mruby-mac.sh` is just `rake` (no args), which runs the **repo-root `Rakefile.rb`** → `costumized-build-conf-mac.rb`. That config does a dual `CrossBuild` (x86_64 + arm64); xmake then `lipo`-merges them into a universal `.dylib`. Other platforms call `rake` inside `mruby/` directly.
- **Windows VS path is hardcoded** in `build-win.ps1` to the **Community** edition vcvars64.bat; CI uses **Enterprise**. Edit the path to match your install or MSVC activation fails silently. Run mruby's build from a VS x64 Native Tools prompt.
- **Platform `DefineConstants` are typo'd** in `MRuby.Library.csproj`: `PALTFORM_WINDOWS` / `PALTFORM_UNIX` / `PALTFORM_MACOS`. Match the existing spelling in `#if` guards.
- README says `xmake f -m releasedbg`, but all scripts/CI use `-m release`. Use `release`.

## Tests

xUnit 2.9 + coverlet. One test class per wrapper type (`Rb<Type>Test.cs`), `[Fact]` methods named `TestXxx`. `Xunit` is a global using. Each test opens its own `Ruby.Open()` (no shared fixtures). `test_scripts/*.rb` are copied to output and loaded by file-path tests. Project targets 100% coverage; intentional exclusions use `[ExcludeFromCodeCoverage]` (GC keeper, debug/inspect helpers).
