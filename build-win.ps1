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

# Locate vcvars64.bat via vswhere instead of hardcoding an edition/version path
# (Community/Professional/Enterprise and the version folder all differ per machine).
function Find-VcVars64 {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $installPath = & $vswhere -latest -products * `
            -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 `
            -property installationPath
        if ($installPath) {
            $candidate = Join-Path $installPath "VC\Auxiliary\Build\vcvars64.bat"
            if (Test-Path $candidate) { return $candidate }
        }
    }

    # Fallback: probe the common fixed locations for each edition.
    foreach ($base in @(${env:ProgramFiles}, ${env:ProgramFiles(x86)})) {
        foreach ($ver in @("2022")) {
            foreach ($ed in @("Enterprise", "Professional", "Community", "BuildTools")) {
                $candidate = Join-Path $base "Microsoft Visual Studio\$ver\$ed\VC\Auxiliary\Build\vcvars64.bat"
                if (Test-Path $candidate) { return $candidate }
            }
        }
    }

    return $null
}

$vcvars = Find-VcVars64
if (-not $vcvars) {
    Write-Error "Could not locate vcvars64.bat. Install Visual Studio with the 'Desktop development with C++' (VC x64) workload, or run this script from a VS x64 Native Tools prompt."
    exit 1
}
Write-Output "Using vcvars64: $vcvars"

$cmd_cmds = @"
call "$vcvars" && call .\build-mruby-win.bat
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
 