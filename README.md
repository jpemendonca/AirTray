# WindFree Control

Small native Windows app (WinUI 3, .NET 10, Native AOT) to control a Samsung WindFree air
conditioner through SmartThings. Portuguese and English.

## Requirements

- Windows 10 1809+ / Windows 11, x64
- .NET 10 SDK (to build)
- A SmartThings Personal Access Token with `r:devices:*` and `x:devices:*`
  (<https://account.smartthings.com/tokens>). Tokens created after 2024-12-30 expire after 24 h.

## Run

```powershell
dotnet run --project src/WindFreeControl.App
```

On first run, paste the token. It is stored in Windows Credential Locker.

## Test and gate

```powershell
git config core.hooksPath .githooks   # once per clone
./gate.ps1
```

## Publish

```powershell
./publish.ps1   # Native AOT, output in publish/
```

## Inspect the device

```powershell
$env:ST_TOKEN = "<token>"; ./tools/dump-device.ps1   # read-only, writes tools/out/
```

Agents: start at [AGENTS.md](AGENTS.md).
