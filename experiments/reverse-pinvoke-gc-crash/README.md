# Reverse-P/Invoke GC-Suspension Crash — minimal reproduction (no mruby)

This folder reproduces the **macOS-specific CoreCLR crash** that has dogged this
repo's CI **without any mruby code at all**. It is a pure `C# + ~20-line C shim`
experiment whose only job is to cross the **native→managed (reverse-P/Invoke)**
boundary millions of times while the GC is suspending threads.

> **Why this exists:** to prove the crash is a property of **CoreCLR's
> reverse-P/Invoke + GC-thread-suspension machinery on Unix/macOS**, not a bug in
> mruby or in this wrapper. If a 20-line C shim crashes the same way the mruby
> binding does, the binding was never the cause.

## The mechanism (what we're testing)

1. A CLR `delegate` function pointer is handed to native C code.
2. Native code calls it in a tight loop (`run_callback_loop` in `nativeshim.c`).
   Each call is a **reverse-P/Invoke**: the worker thread flips its GC mode
   `preemptive → cooperative` and pushes the explicit transition frame the GC
   stack-walker relies on, then pops it on return. Millions of times per second.
3. The callback allocates managed memory that **survives** into gen2 (rolling
   survivor list), and a dedicated **`GC.Collect(2, Forced, blocking:true)`
   driver thread** keeps blocking GCs firing.
4. To suspend the worker threads for a GC, CoreCLR on **Unix/macOS sends a POSIX
   activation signal** (`SIGUSR1`/`SIGRTMIN`). If that signal lands on a worker
   **exactly while it is mid-transition at the reverse-P/Invoke boundary**, the
   runtime can't safely resume it → **SIGSEGV / SIGABRT**.
5. On **Windows**, the GC suspends threads with `SuspendThread` — a synchronous
   OS call, **no async signal at the boundary** — so the window does not exist.

## Class A vs Class B (why the delegate is rooted)

- **Class A** — delegate gets GC-collected while native holds its raw pointer →
  call through freed memory. *Not what we're testing.*
- **Class B** — the GC-suspension-at-boundary window above. *This is the target.*

`Program.cs` **roots** the delegate in a `static` field **and** a `GCHandle` for
the whole process. Class A is therefore impossible, so any crash that remains is
**purely Class B**. (This binding already fixes Class A in production via
`RbCallbackKeeper`.)

## The proof is the cross-platform contrast

Run the GitHub Actions workflow **`Reverse-PInvoke GC-Suspension Crash Repro`**
(`workflow_dispatch`). It runs the same binary ~30× on a matrix of
`{macos-14, ubuntu-24.04, windows-2022} × {server, workstation}` GC and tallies
crashes. Expected shape (probabilistic, per prior art — see below):

| Platform | Suspension mechanism | Expected crashes |
|---|---|---|
| **macOS-14 (arm64, 3-core)** | POSIX signal | **> 0** — fires some % of attempts |
| Linux (x64) | POSIX signal | rare / occasional |
| Windows (x64) | `SuspendThread` | **0** |

`139 = SIGSEGV`, `134 = SIGABRT`, `135 = SIGBUS`.

## Run locally

```bash
# macOS
cc -O2 -dynamiclib -o libnativeshim.dylib nativeshim.c
# Linux
cc -O2 -fPIC -shared -o libnativeshim.so nativeshim.c
# Windows (VS x64 Native Tools prompt)
cl /O2 /LD nativeshim.c /Fe:nativeshim.dll

dotnet build -c Release
DOTNET_gcServer=1 DOTNET_gcConcurrent=1 dotnet bin/Release/net8.0/ReproGcCrash.dll
```

### Tunables (env vars)

| Var | Default | Meaning |
|---|---|---|
| `REPRO_THREADS` | 4 | worker threads each looping through the native callback |
| `REPRO_SECONDS` | 20 | wall-clock per attempt before draining to a clean exit |
| `REPRO_ITERS` | 100000000 | native callback iterations per thread |
| `REPRO_GARBAGE_KB` | 16 | bytes allocated per callback |
| `REPRO_SURVIVORS` | 2048 | rolling survivor-list length (drives gen2 promotion) |
| `REPRO_FORCE_GC` | 1 | run the forced blocking-gen2 driver thread |
| `REPRO_FORCE_GC_US` | 20 | sleep between forced collects (µs) |
| `DOTNET_gcServer` | (matrix) | `1` = Server GC, `0` = Workstation |
| `DOTNET_gcConcurrent` | 1 | background/concurrent GC ON (needed for the BGC suspender) |

## Honest caveat on reproducibility

Prior art (dotnet/runtime **#82684**, **#127320**) shows this exact Class-B
window is **probabilistic** for a *pure* C shim — the only published *deterministic*
repro (egonelbre, #127320) needed Go's broken `sigaltstack` as an amplifier. So a
single clean run **does not** disprove the bug; the workflow re-runs many times
and reports a **rate**. A non-zero macOS rate next to a zero Windows rate is the
result that matters. This mirrors the ~10% crash rate the real mruby storm shows
on the same `macos-14` runner — same window, same runner, no mruby.

## Files

- `nativeshim.c` — the entire native side (~20 lines, no dependencies).
- `Program.cs` — rooted delegate, worker threads, survivor allocator, forced-GC
  driver, gen-count monitor, deadline watchdog.
- `ReproGcCrash.csproj` — Server+concurrent GC, tiering off, copies the shim.
- `../../.github/workflows/repro-gc-crash.yml` — the matrix runner + tally.
