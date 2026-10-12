# WindFree Control

Small native Windows app (WinUI 3, .NET 10) to control a Samsung WindFree air conditioner through
SmartThings. Portuguese and English, light and dark theme, tray icon.

## Requirements

- Windows 10 1809+ / Windows 11, x64
- .NET 10 SDK (to build)
- For the Native AOT build: Visual Studio "MSVC x64 build tools" + Windows SDK component
- A SmartThings Personal Access Token with `r:devices:*` and `x:devices:*`
  (<https://account.smartthings.com/tokens>). Tokens created after 2024-12-30 expire after 24 h.

## Run from source

```powershell
dotnet run --project src/WindFreeControl.App
```

On first run, paste the token. It is stored in Windows Credential Locker.
Settings (gear in the title bar): language, change/forget token, close to tray, start minimized,
start with Windows. Settings file: `%LocalAppData%\WindFreeControl\settings.json`.
Unhandled errors go to `%LocalAppData%\WindFreeControl\crash.log`.

## Publish and install

```powershell
./publish.ps1          # Native AOT (needs the MSVC tools), output in publish/
./publish.ps1 -NoAot   # self-contained ReadyToRun build, works without the MSVC tools
./tools/install-shortcut.ps1          # Start menu shortcut to publish/WindFreeControl.exe
./tools/install-shortcut.ps1 -Remove  # remove it
```

## Test and gate

```powershell
git config core.hooksPath .githooks   # once per clone
./gate.ps1
```

## Demo mode (no token, no device)

```powershell
WindFreeControl.exe --demo                       # real status fixture, commands applied in memory
WindFreeControl.exe --demo --startup             # starts hidden in the tray
WindFreeControl.exe --demo --snapshot out.png    # off-screen render of the page, then exits
    [--snapshot-delay 5] [--theme light|dark] [--lang en|pt-BR] [--page settings]
```

Demo mode never touches the real settings file, the Run registry key or the device.

## Inspect the device

```powershell
$env:ST_TOKEN = "<token>"; ./tools/dump-device.ps1   # read-only, writes tools/out/
```

## Icons

```powershell
python tools/make-icons.py   # needs Pillow; writes src/WindFreeControl.App/Assets
```

Agents: start at [AGENTS.md](AGENTS.md).
