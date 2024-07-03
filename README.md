# mruby-for-dotnet

A mruby-wrapper for .NET, current for Windows.

## How to

1. `git submodule update --init --recursive`
2. `./build-mruby.bat` (for Windows, run this command under `VS x64 Command Prommpt)` or `./build-mruby.sh` for (*nix)
3. `cd ../mruby-win-shared; xmake`

## Status

This project is adding unittest to all wrapped mruby APIs, aimed to 100% coverage.
