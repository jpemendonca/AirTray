param(
    # Use when the MSVC x64 build tools are missing: self-contained ReadyToRun build instead of Native AOT.
    [switch]$NoAot,
    [string]$Version
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

# The ILCompiler calls vswhere.exe without a path to find the MSVC linker.
$env:PATH += ";${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer"

$out = Join-Path $PSScriptRoot "publish"
if (Test-Path $out) { Remove-Item -Recurse -Force $out }

$extra = @()
if ($NoAot) { $extra += @("-p:PublishAot=false", "-p:PublishReadyToRun=true") }
if ($Version) { $extra += "-p:Version=$Version" }

dotnet publish src/WindFreeControl.App -c Release -r win-x64 -o $out --nologo -warnaserror @extra
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$size = (Get-ChildItem $out -Recurse -File | Measure-Object Length -Sum).Sum / 1MB
Write-Host ("Published to {0} ({1:N1} MB)" -f $out, $size) -ForegroundColor Green
