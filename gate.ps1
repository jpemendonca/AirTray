$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

function Step([string]$name, [scriptblock]$command) {
    Write-Host "==> $name" -ForegroundColor Cyan
    & $command
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Gate failed at: $name" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

Step "format" { dotnet format WindFreeControl.slnx --verify-no-changes --exclude-diagnostics IDE0005 --verbosity quiet }
Step "build" { dotnet build WindFreeControl.slnx -c Release -warnaserror --nologo --verbosity quiet }
Step "test" { dotnet test --solution WindFreeControl.slnx -c Release --no-build }

Write-Host "Gate passed." -ForegroundColor Green
