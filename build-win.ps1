param(
    [switch]$clean
)

if ($clean) {
    Write-Output "Cleaning mruby and mruby-wrapper build cache"
    Set-Location mruby
    Start-Process cmd.exe -ArgumentList "/c rake clean" -Wait -NoNewWindow

    Set-Location ..\mruby-shared
    xmake clean

    Set-Location ..
    exit
}

Write-Output "Building mruby and mruby-wrapper for Windows"

Write-Output "Building mruby"
$cmd_cmds = @"
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" && call .\build-mruby-win.bat
"@

Start-Process cmd.exe -ArgumentList "/c $cmd_cmds" -Wait -NoNewWindow

Write-Output "Building mruby-shared"
Set-Location mruby-shared
& xmake f -m release
& xmake

Write-Output "Building mruby-wrapper"
Set-Location ..\mruby-wrapper
dotnet restore
dotnet build --configuration=release
dotnet test --configuration=release
dotnet pack --configuration=release

Set-Location ..\

Write-Output "Build successful"
 