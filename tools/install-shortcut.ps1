param(
    [switch]$Remove
)

$ErrorActionPreference = "Stop"

$exe = Join-Path $PSScriptRoot "..\publish\WindFreeControl.exe" | Resolve-Path -ErrorAction SilentlyContinue
$shortcut = Join-Path ([Environment]::GetFolderPath("Programs")) "WindFree Control.lnk"

if ($Remove) {
    Remove-Item $shortcut -ErrorAction SilentlyContinue
    Write-Host "Removed $shortcut"
    exit 0
}

if (-not $exe) {
    throw "publish\WindFreeControl.exe not found. Run ./publish.ps1 first."
}

$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut($shortcut)
$link.TargetPath = $exe.Path
$link.WorkingDirectory = Split-Path $exe.Path
$link.IconLocation = "$($exe.Path),0"
$link.Description = "WindFree Control"
$link.Save()
Write-Host "Created $shortcut"
