$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$out = Join-Path $PSScriptRoot "publish"
if (Test-Path $out) { Remove-Item -Recurse -Force $out }

dotnet publish src/WindFreeControl.App -c Release -r win-x64 -o $out --nologo -warnaserror
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$size = (Get-ChildItem $out -Recurse -File | Measure-Object Length -Sum).Sum / 1MB
Write-Host ("Published to {0} ({1:N1} MB)" -f $out, $size) -ForegroundColor Green
