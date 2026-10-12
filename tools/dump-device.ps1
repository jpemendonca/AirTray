param(
    [string]$DeviceId = "00000000-0000-0000-0000-000000000000"
)

$ErrorActionPreference = "Stop"

if (-not $env:ST_TOKEN) {
    throw "Set `$env:ST_TOKEN to your SmartThings token before running."
}

$headers = @{ Authorization = "Bearer $env:ST_TOKEN" }
$base = "https://api.smartthings.com/v1"
$out = Join-Path $PSScriptRoot "out"
New-Item -ItemType Directory -Force $out | Out-Null

$device = Invoke-RestMethod "$base/devices/$DeviceId" -Headers $headers
$device | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $out "device.json")

Invoke-RestMethod "$base/devices/$DeviceId/status" -Headers $headers |
    ConvertTo-Json -Depth 20 | Set-Content (Join-Path $out "status.json")

$definitions = @{}
foreach ($cap in $device.components[0].capabilities) {
    try {
        $definitions[$cap.id] = Invoke-RestMethod "$base/capabilities/$($cap.id)/$($cap.version)" -Headers $headers
    } catch {
        $definitions[$cap.id] = @{ error = $_.Exception.Message }
    }
}
$definitions | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $out "capabilities.json")

Write-Host "Saved device.json, status.json and capabilities.json to $out"
