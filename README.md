<p align="center">
  <img src="docs/images/hero.png" alt="AirTray: control a Samsung WindFree air conditioner from the Windows tray" width="100%">
</p>

<p align="center">
  <a href="https://github.com/jpemendonca/AirTray/releases/latest"><img alt="Download" src="https://img.shields.io/github/v/release/jpemendonca/AirTray?label=download&style=for-the-badge&color=2563eb"></a>
  <img alt="Windows 10/11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?style=for-the-badge&logo=windows11&logoColor=white">
  <img alt=".NET 10 Native AOT" src="https://img.shields.io/badge/.NET%2010-Native%20AOT-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
  <img alt="WinUI 3" src="https://img.shields.io/badge/UI-WinUI%203-1f6feb?style=for-the-badge">
</p>

<p align="center">
  <a href="README.pt-BR.md">Leia em português</a>
</p>

**AirTray** is a small native Windows app to control a **Samsung WindFree** air conditioner
through the SmartThings cloud. It opens in a blink, lives in the tray next to the clock, and looks
like it shipped with Windows 11: Mica, light and dark themes, Fluent icons, Portuguese and English.

I built it because the official SmartThings app on Windows kept freezing. AirTray does one thing
and stays out of the way.

## Features

- **Tray flyout**: click the icon next to the clock for power, target temperature and the four
  shortcuts you use the most (Cool, WindFree, Economy, Sleep). Click outside and it is gone.
- **Every control the unit reports**: mode, fan speed, wind direction, special modes (WindFree,
  WindFree Sleep, Economy, Quiet, Fast, Sleep), tropical night, sounds, display light, auto clean.
- **Built from what your unit supports**: buttons come from the device's own capability list, and
  limits are respected (Economy mode raises the minimum to 24 °C, so "−" stops there).
- **Maintenance**: dust filter usage with reset, self-check, firmware info.
- **Feels native**: single instance, remembers window position, closes to tray, can start with
  Windows, follows the system theme, English or Portuguese switchable on the fly.
- **Private by design**: your SmartThings token stays in the Windows Credential Locker. No
  server, no telemetry, nothing but your PC talking to SmartThings.

<p align="center">
  <img src="docs/images/flyout.png" alt="Tray flyout in dark and light theme" width="760">
</p>

<p align="center">
  <img src="docs/images/control.png" alt="Main window in dark and light theme" width="760">
</p>

<p align="center">
  <img src="docs/images/settings-language.png" alt="Settings page and the English interface" width="760">
</p>

## Download and use

1. Grab the latest `AirTray-*-win-x64.zip` from
   [Releases](https://github.com/jpemendonca/AirTray/releases/latest) and unzip it anywhere.
2. Run `WindFreeControl.exe`. The exe is not code-signed, so Windows SmartScreen may warn you the
   first time: choose **More info → Run anyway**.
3. Create a SmartThings personal access token at
   [account.smartthings.com/tokens](https://account.smartthings.com/tokens) with the
   `Devices` scopes (`r:devices:*` and `x:devices:*`), paste it and press **Connect**.

### Compatibility

Tested with a Samsung WindFree wall unit (2021, `ARA-WW-TP1-22-COMMON` Wi-Fi module). Other
Samsung room air conditioners on SmartThings should work, because the interface is built from
what the device reports, but they are untested. The app controls the first air conditioner on the
account.

## Under the hood

| | |
|---|---|
| UI | WinUI 3 (Windows App SDK), unpackaged, Mica and acrylic backdrops |
| Runtime | .NET 10, published with Native AOT (no .NET install needed) |
| Tray | Hand-written `Shell_NotifyIconW` wrapper, no extra packages |
| API | SmartThings REST: `GET /devices`, `GET /devices/{id}/status`, `POST /devices/{id}/commands` |
| Storage | Token in Credential Locker, settings in `%LocalAppData%\WindFreeControl\settings.json` |
| Process | Built with an agent-driven workflow: [AGENTS.md](AGENTS.md), [SPECS.md](SPECS.md), [ROADMAP.md](ROADMAP.md) |

## Build from source

Requirements: Windows 10 1809+ x64, .NET 10 SDK, and for Native AOT the Visual Studio
"MSVC x64 build tools" plus the Windows 11 SDK component.

```powershell
dotnet run --project src/WindFreeControl.App   # run
./gate.ps1                                     # format check, Release build, tests
./publish.ps1                                  # Native AOT build in publish/
./publish.ps1 -NoAot                           # without the MSVC tools (bigger, ReadyToRun)
./tools/install-shortcut.ps1                   # Start menu shortcut to publish/
```

### Demo mode

Runs on a real status snapshot of the unit, without a token or a device. Commands are applied in
memory, and the real settings file and registry are never touched.

```powershell
WindFreeControl.exe --demo
WindFreeControl.exe --demo --startup              # start hidden in the tray
WindFreeControl.exe --demo --demo-reject-token    # simulate an expired token
WindFreeControl.exe --demo --snapshot out.png     # render a page off-screen and exit
    [--snapshot-delay 5] [--theme light|dark] [--lang en|pt-BR] [--page settings|flyout]
```

### Tools

```powershell
$env:ST_TOKEN = "<token>"; ./tools/dump-device.ps1   # read-only dump of your unit's capabilities
python tools/make-icons.py                            # app and tray icons (Pillow)
python tools/make-readme-images.py <snapshot-folder>  # the images in this README (Pillow)
```

## License

[MIT](LICENSE).

## Disclaimer

Not affiliated with or endorsed by Samsung. Samsung, WindFree and SmartThings are trademarks of
Samsung Electronics.
