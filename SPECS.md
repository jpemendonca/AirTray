# SPECS.md

## Product

A native Windows app to control one Samsung WindFree air conditioner via SmartThings. It replaces
the SmartThings app for daily control at the desk: same information, same controls, faster and
without freezes.

In scope:

- First-run token setup; token kept in Windows Credential Locker.
- Automatic discovery of the air conditioner on the account.
- Every control and reading this unit reports as supported (list in [docs/DEVICE.md](docs/DEVICE.md)):
  power, target temperature, mode, fan speed, wind direction, optional modes (WindFree, sleep,
  quiet, smart, speed, WindFree sleep), tropical night mode, auto clean, beep sound,
  display light (experimental), room temperature, humidity, power and energy, dust filter state
  and reset, self-check.
- Tray icon with quick power toggle.
- UI in Brazilian Portuguese and English, chosen in Settings.
- Native AOT publish as a single folder with one `.exe`.

Out of scope (for now):

- More than one device or location. The model allows it later; the UI shows one.
- On/off timers and schedules. This unit exposes no timer capability; a local timer would need
  the app running, which is a different product.
- Energy history charts. The API gives a cumulative counter and the last interval only.
- Firmware update, demand response, OpenADR, Wi-Fi configuration, diagnostics upload.
- Capabilities this unit lists in `custom.disabledCapabilities` and returns `null` for
  (air quality, dust, odor, very fine dust, HEPA/deodor filters, do-not-disturb, welcome
  cooling, odor controller, individual lock). See "Supported vs available".
- Installer / MSIX / auto-update.

## Architecture

Two projects plus tests. The split is enforced by the build: Core targets `net10.0` (no Windows
TFM), so it cannot reference WinUI.

```
WindFreeControl.App (net10.0-windows10.0.19041.0, WinUI 3, unpackaged, PublishAot)
  ├─ MainWindow  (Mica backdrop, custom title bar, hosts a Frame)
  ├─ Views/SetupPage    ── binds ──> SetupViewModel
  ├─ Views/ControlPage  ── binds ──> MainViewModel
  ├─ Views/SettingsPage ── binds ──> SettingsViewModel
  ├─ Services/CredentialLockerTokenStore : ITokenStore
  ├─ Services/JsonSettingsStore : ISettingsStore
  └─ Services/TrayIcon (Win32 Shell_NotifyIconW, no package)
          │
          ▼
WindFreeControl.Core (net10.0, IsAotCompatible)
  ├─ SmartThings/SmartThingsClient   HttpClient wrapper: devices, status, commands
  ├─ SmartThings/Dto + JsonContext   source-generated System.Text.Json
  ├─ SmartThings/Commands            builders: Power(on), Setpoint(25), Mode("cool")...
  ├─ Device/AcState                  immutable record parsed from /status
  ├─ Device/*Mode                    known values + resource keys for labels
  ├─ Localization/Strings.resx       pt-BR (neutral) + Strings.en.resx
  └─ ViewModels/*                    CommunityToolkit.Mvvm, depend only on Core abstractions
```

State lives in `MainViewModel`, which holds the last `AcState`. Views never call the API.

### Refresh and command flow

1. On start (token present): `GET /devices`, pick the air conditioner, `GET /devices/{id}/status`,
   build `AcState`, render.
2. While the window is visible: re-read status every 15 s. While hidden in tray: every 60 s
   (to keep the tray tooltip and power state right). Never faster — PAT rate limits apply.
3. On a user command: update the view model optimistically, `POST /commands`, then re-read
   status after 2 s. If the POST fails or the re-read disagrees, the view reflects the real
   state and shows a short error in an `InfoBar`.
4. Target temperature +/-: debounce 600 ms, then send one `setCoolingSetpoint` with the final
   value. Do not send one command per click.

## Device model

`AcState` is a pure function of the status JSON: `AcState.Parse(JsonDocument)`. Fields (null when
the device reports null or omits the attribute):

| Field | Source (capability.attribute) | Notes |
|---|---|---|
| `IsOn` | `switch.switch` | `"on"`/`"off"` |
| `RoomTemperature` | `temperatureMeasurement.temperature` | °C |
| `Humidity` | `relativeHumidityMeasurement.humidity` | % |
| `TargetTemperature` | `thermostatCoolingSetpoint.coolingSetpoint` | °C |
| `TargetRange` | `thermostatCoolingSetpoint.coolingSetpointRange` | min, max, step (16-30, step 1) |
| `Mode`, `ModeOptions` | `airConditionerMode.airConditionerMode`, `availableAcModes` ?? `supportedAcModes` | |
| `FanMode`, `FanOptions` | `airConditionerFanMode.fanMode`, `availableAcFanModes` ?? `supportedAcFanModes` | |
| `WindDirection`, `WindOptions` | `fanOscillationMode.fanOscillationMode`, `availableFanOscillationModes` ?? `supportedFanOscillationModes` | |
| `OptionalMode`, `OptionalModeOptions` | `custom.airConditionerOptionalMode.acOptionalMode`, `availableAcOptionalMode` ?? `supportedAcOptionalMode` | WindFree lives here |
| `TropicalNightHours` | `custom.airConditionerTropicalNightMode.acTropicalNightModeLevel` | 0 = off, max 35 |
| `AutoClean` | `custom.autoCleaningMode.autoCleaningMode` | on/off |
| `AutoCleanState` | `custom.autoCleaningMode.operatingState` | `ready`/`autoClean` |
| `Volume` | `audioVolume.volume` | 0 = mute, 100 = beep on |
| `Lighting` | `samsungce.airConditionerLighting.lighting` | experimental, see below |
| `PowerWatts` | `powerConsumptionReport.powerConsumption.power` | W |
| `EnergyTotalWh` | `powerConsumptionReport.powerConsumption.energy` | cumulative Wh |
| `FilterUsagePercent`, `FilterStatus`, `FilterCapacityHours` | `custom.dustFilter.*` | status `normal`/`wash`/`replace` |
| `SelfCheckStatus`, `SelfCheckErrors` | `samsungce.selfCheck.*` | |
| `DisabledCapabilities` | `custom.disabledCapabilities.disabledCapabilities` | |

### Supported vs available

- `supported*` lists what the hardware can do. `available*` lists what is allowed right now
  (e.g. in `dry` mode some fan speeds may be locked). Show `supported*` as options, enable only
  those in `available*` when it is non-null.
- A capability in `custom.disabledCapabilities` is hidden — unless the bullet that introduces it
  proves on the real device that the command works (lighting and beep are such candidates: they
  report values while listed as disabled).
- Unknown enum values from the device are shown with their raw value, never dropped or crashed on.

## Contracts (SmartThings REST)

Base: `https://api.smartthings.com/v1`. Header: `Authorization: Bearer {token}`.

| Call | Use |
|---|---|
| `GET /devices` | discovery; pick items whose `ocf.ocfDeviceType == "oic.d.airconditioner"` or that have capability `airConditionerMode`. One → auto-select. Several → first, out of scope to choose. |
| `GET /devices/{id}/status` | full state, parsed into `AcState` |
| `POST /devices/{id}/commands` | body below; 200 = accepted by cloud (not proof the unit obeyed) |

```json
{ "commands": [ { "component": "main", "capability": "switch", "command": "on", "arguments": [] } ] }
```

Commands used, all on component `main`:

| Capability | Command | Arguments |
|---|---|---|
| `switch` | `on` / `off` | — |
| `thermostatCoolingSetpoint` | `setCoolingSetpoint` | `[25]` (number) |
| `airConditionerMode` | `setAirConditionerMode` | `["cool"]` |
| `airConditionerFanMode` | `setFanMode` | `["auto"]` |
| `fanOscillationMode` | `setFanOscillationMode` | `["vertical"]` |
| `custom.airConditionerOptionalMode` | `setAcOptionalMode` | `["windFree"]` |
| `custom.airConditionerTropicalNightMode` | `setAcTropicalNightModeLevel` | `[8]` (hours, 0-35) |
| `custom.autoCleaningMode` | `setAutoCleaningMode` | `["on"]` / `["off"]` |
| `audioVolume` | `setVolume` | `[0]` or `[100]` |
| `samsungce.airConditionerLighting` | `on` / `off` | — (experimental) |
| `custom.dustFilter` | `resetDustFilter` | — |
| `samsungce.selfCheck` | `startSelfCheck` / `cancelSelfCheck` | — |
| `refresh` | `refresh` | — (asks the unit to push fresh state) |

Error mapping: 401 → token invalid/expired → back to SetupPage with a message. 403 → token
lacks scope (`r:devices:*`, `x:devices:*`). 422 → command not accepted for current state → InfoBar
with the API message. 429 → back off, double the refresh interval until the next success.
Network failure → keep last state, mark it stale with its timestamp.

## Data on first use

Nothing is embedded except UI resources. The main path needs network and a valid token; without
them the app shows the setup screen or a "cannot reach SmartThings" state. This is inherent:
the device is controlled only through the cloud.

## Authentication and token

- Personal Access Token (PAT) pasted by the user. Scopes needed: `r:devices:*`, `x:devices:*`.
- PATs created after 2024-12-30 expire after 24 h. The user's current token predates that and
  is long-lived. If it ever dies, a new PAT works for one day only; the long-term answer is the
  OAuth authorization-code flow with refresh tokens — a future bullet, not now.
- Stored in Windows Credential Locker (`PasswordVault`, resource `WindFreeControl`), encrypted by
  Windows per user. Never in a file, never in logs.
- Settings page has "Change token" and "Forget token".

## Localization

- Neutral resources are pt-BR (`Strings.resx`), plus `Strings.en.resx`. Both live in Core, so view
  models produce labels and tests can check both languages.
- Language setting: `System` (default), `Português`, `English`. Stored in settings JSON. Applied
  by setting `CultureInfo.CurrentUICulture` at startup; changing it in Settings rebuilds the
  window content (no restart).
- Device enum values (`cool`, `windFree`...) map to resource keys (`Mode_cool`, `Optional_windFree`).
  A value without a key shows its raw text.

## Local settings

`%LocalAppData%\WindFreeControl\settings.json`, source-generated JSON:
`language`, `deviceId` (last selected), `closeToTray` (default true), `startMinimized`
(default false), `windowBounds`.

## Tests

Policy: essential tests only (revised 2026-10-12, see decision log). A test is written only where
a silent break would be expensive and is not obvious when using the app:

- command bodies sent to SmartThings (a wrong body silently does nothing on the unit);
- `AcState.Parse` against the real `status.json` fixture;
- client error mapping for 401 (it drives the return to the setup screen);
- localization key parity between `Strings.resx` and `Strings.en.resx`.

Everything else is verified by using the app. No UI tests, never the real API.

## Secrets and config

Simple scheme. The only secret is the token, and it never touches the repository: the user types
it into the app, Windows stores it. No `.env`, no user-secrets. `tools/dump-device.ps1` reads it
from `$env:ST_TOKEN` in the user's own terminal; its output folder is git-ignored.

## Build and publish

- `dotnet publish src/WindFreeControl.App -c Release -r win-x64` → Native AOT, self-contained,
  Windows App SDK self-contained (`WindowsAppSDKSelfContained=true`), output in `publish/`.
- Targets: cold start under 1 s to first frame, under 60 MB working set on the control screen.
  Measured, not assumed (Phase 0 and Phase 5 bullets).

## Decision log

- 2026-10-12: WinUI 3 + .NET 10 + Native AOT chosen over plain JIT and over WPF. AOT costs only
  discipline (`x:Bind`, JSON source gen) if adopted from day one; retrofitting is expensive.
- 2026-10-12: Unpackaged app (no MSIX). Personal tool; avoids certificate and package ceremony.
- 2026-10-12: Token in Credential Locker, not DPAPI file, not `.env`.
- 2026-10-12: View models in Core (no WinUI dependency) so they are unit-testable.
- 2026-10-12: Bilingual UI (pt-BR, en) with a Settings switch, requested by the user. Resources in
  Core `.resx` instead of WinUI `.resw`, because runtime switching of `.resw` in unpackaged apps
  is unreliable and `.resx` is testable from Core.
- 2026-10-12: `GenerateDocumentationFile` is on only so the build enforces IDE0005 (unused
  usings). CS1591 (missing XML doc) is turned off project-wide because the comment policy does not
  use XML doc comments. This is policy, not a suppression to pass the gate.
- 2026-10-12: Test policy reduced from "happy-path on Core + view models" to essential tests only,
  at the user's request (speed over coverage on a personal tool).
- 2026-10-12: Tray icon implemented directly on `Shell_NotifyIconW` with `LibraryImport` and a
  window subclass, instead of H.NotifyIcon.WinUI. Reason: no new dependency, small, and AOT safe
  by construction.
- 2026-10-12: Demo mode (`--demo`) uses an in-memory settings store and a no-op system
  integration, so agent checks never touch the user's settings or the Run registry key.
  `--snapshot`, `--theme`, `--lang` and `--page` exist only for these checks.
- 2026-10-12: Pre-commit hook removed at the user's request (the WinUI Release build made each
  commit take minutes). `./gate.ps1` is still required before committing; agents run it by hand.
