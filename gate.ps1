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
# dotnet format leaves MSBuild nodes holding XAML outputs open; stop them before building.
dotnet build-server shutdown | Out-Null
Step "build" { dotnet build WindFreeControl.slnx -c Release -warnaserror --nologo --verbosity quiet -nodeReuse:false -m:1 }
Step "test" { dotnet test --solution WindFreeControl.slnx -c Release --no-build }

Write-Host "Gate passed." -ForegroundColor Green
