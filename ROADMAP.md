# ROADMAP.md

The only work queue. Take the first unchecked bullet. Do not skip, do not batch.

## Definition of done

- **Code written**: gate green (`./gate.ps1`), essential tests only (SPECS.md), ROADMAP updated,
  conventional commit.
- The user checks the product only at bullet 1.5. After the user's OK there, the agent finishes
  every remaining phase without stopping for review; path walks after 1.5 are done by the agent
  where possible and otherwise recorded as "path not walked".
- **Path walked**: the user opened the app and used the feature on the real air conditioner.
  Only the user can do this (it needs the real token). Each validation note says which of the two
  happened. A bullet that only has code written says "code written, path not walked".

Do not confuse code written with path validated.

Each bullet: origin, scope, acceptance. On completion add `> Validation (YYYY-MM-DD): ...`.
When stopping midway: `> In progress (YYYY-MM-DD): what exists, what is missing, next step`.
User fixes and requests enter here as new bullets before any code.

## Phase 0 — Foundation

- [x] **0.1 Project documents and repository.**
  Origin: bootstrap. Scope: AGENTS/SPECS/CODESTYLE/ROADMAP/README, CLAUDE.md and GEMINI.md
  pointers, `.gitignore`, `.gitattributes`, `docs/DEVICE.md`, `docs/VISUAL.md`, git init, private
  GitHub repo `WindFreeControl`. Acceptance: first commit pushed; `tools/out/` not tracked.
  > Validation (2026-10-12): documents written from the real device dump; `tools/out/` confirmed
  > untracked by `git status`. Code written, no product to walk yet.

- [x] **0.2 Solution skeleton.**
  Origin: SPECS architecture. Scope: `WindFreeControl.slnx`, `Directory.Build.props`
  (net10.0, nullable, warnings as errors, analyzers), `Directory.Packages.props` (central package
  versions), `src/WindFreeControl.Core` (`IsAotCompatible`), `src/WindFreeControl.App`
  (WinUI 3 unpackaged, `PublishAot`, Windows App SDK self-contained, Mica window with custom title
  bar showing the app name), `tests/WindFreeControl.Core.Tests` (xUnit v3, one trivial test).
  Packages: Microsoft.WindowsAppSDK, Microsoft.Windows.SDK.BuildTools, CommunityToolkit.Mvvm,
  xunit.v3. Acceptance: `dotnet build` and `dotnet test` green; the app opens an empty Mica window.

- [x] **0.3 Quality gate and pre-commit hook.**
  Origin: AGENTS. Scope: `gate.ps1` (format verify, Release build with warnings as errors, tests),
  `.githooks/pre-commit`, `.editorconfig`. Acceptance: gate passes on a clean tree and fails on
  a deliberately misformatted file (checked once, not committed).
  > Validation (2026-10-12): first run failed on LF line endings in `.cs` files (the misformat
  > case, observed for real); after `dotnet format` the gate passed. Hook enabled via
  > `core.hooksPath`. Code written.

- [ ] **0.4 AOT publish smoke.**
  Origin: decision log (AOT risk must surface early). Scope: `publish.ps1` producing
  `publish/WindFreeControl.exe`; record exe folder size, cold start time and working set of the
  empty window in the validation note. Acceptance: published exe opens on a machine path without
  the .NET SDK on PATH; zero AOT/trim warnings.
  > Blocked (2026-10-12): `publish.ps1` exists, but ILCompiler reports "Platform linker not found".
  > VS 2026 Community lacks the MSVC x64 tools; the user installs them (VC.Tools.x86.x64 + Windows SDK).

- [x] **0.5 Real payload fixtures.**
  Origin: tests policy. Scope: copy `tools/out/status.json` and `device.json` to
  `tests/.../Fixtures/` with ids scrubbed. Acceptance: fixtures load in a test; `git grep` finds no
  real device/location/owner id.
  > Validation (2026-10-12): `status.json` and `devices.json` (list wrapper around the real
  > device) scrubbed; grep for the five real ids and the OTN DUID finds nothing. Code written.

## Phase 1 — Main path, end to end

Ugly is fine. Walkable from first run to controlling the unit.

- [x] **1.1 SmartThings client.**
  Origin: SPECS contracts. Scope: `SmartThingsClient` with `GetDevicesAsync`, `GetStatusAsync`,
  `SendAsync(params DeviceCommand[])`; DTOs + `SmartThingsJsonContext`; command builders for the
  table in SPECS; `SmartThingsError` for 401/403/422/429/network. Caller: 1.3. Acceptance: tests
  for body JSON per command and 401 mapping.
  > Validation (2026-10-12): client, DTOs (source-generated JSON), command builders and error
  > mapping in Core; tests for the six command bodies and 401. Code written; caller arrives in 1.3.

- [x] **1.2 AcState parsing (control fields).**
  Origin: SPECS device model. Scope: `IsOn`, room temperature, humidity, target + range, mode,
  fan, wind direction, optional mode, each with options (available ?? supported). Unknown values
  kept raw. Caller: 1.4. Acceptance: test on the real fixture.
  > Validation (2026-10-12): `AcState.Parse` + `StatusReader`; real fixture test green. Code written.

- [x] **1.3 First-run token setup and discovery.**
  Origin: main path. Scope: `ITokenStore` + `CredentialLockerTokenStore`; `TokenValidation`;
  `SetupViewModel` + `SetupPage` (paste token, "Connect", link to the SmartThings token page,
  error text); on success discover the air conditioner and navigate to the control page; on
  start with a stored token go straight to the control page; 401 anywhere returns to setup.
  Localization infrastructure lands here: `Strings.resx` + `Strings.en.resx`, culture from the
  system, parity test. Acceptance: parity test; app opens on setup without a token.
  > Validation (2026-10-12): setup page captured from the running Debug app (pt-BR text, token
  > field, link, Connect). Token connect not walked: needs the real token (user, 1.5).

- [x] **1.4 Control screen with live state and all main controls.**
  Origin: main path. Scope: `MainViewModel` + `ControlPage`: power button, room temperature and
  humidity, target temperature with +/- (debounced), mode, fan, wind direction, optional mode
  (WindFree etc.) as selectable lists built from `AcState` options; optimistic update, POST,
  re-read after 2 s, rollback + `InfoBar` on failure; 15 s refresh while visible; stale marker
  on network loss. Plain stacked layout. Acceptance: build green; the control page renders the
  real fixture state through the view model (checked by running the app).
  > Validation (2026-10-12): `MainViewModel`, `OptionGroup`, `ControlPage`. Rendering checked in
  > 1.4a demo mode. Code written; path not walked.

- [x] **1.4a Demo mode.**
  Origin: agent cannot use the real token, so it cannot see the control screen. Scope: `--demo`
  argument runs the app against `DemoSmartThingsClient` (embedded scrubbed status, commands
  applied in memory); title shows "(demo)". Acceptance: the control page renders the fixture
  state and reacts to clicks, captured by the agent.
  > Validation (2026-10-12): demo renders the real fixture; clicks via UI Automation on WindFree,
  > Turbo and three times "+" gave WindFree, Turbo and 28° after the re-read. Found and fixed on
  > the way: (1) binding `ItemsSource` to a Core collection crashed WinUI (COMException
  > 0x80040111), so option tiles are built in code; (2) "+"/"−" were disabled for ~2.6 s after a
  > click, defeating the debounce. Added `%LocalAppData%\WindFreeControl\crash.log` for unhandled
  > exceptions.

- [ ] **1.5 Main path walked on the real unit (user).**
  Origin: definition of done. Scope: user runs the AOT published exe, pastes the token, turns the
  unit on and off, changes temperature, mode, fan, wind and WindFree, closes and reopens the app
  (no token prompt). Agent records findings; every problem found becomes a new bullet below this
  one. Acceptance: user confirms each control changed the real unit.

## Phase 2 — Control screen as in SmartThings

- [ ] **2.1 Hero card.**
  Origin: docs/VISUAL.md. Scope: device name, on/off state, big power button, large target
  temperature with -/+ circular buttons, room temperature and humidity line, color/state accent
  by mode (cool blue, dry teal, wind green, auto purple) and greyed when off. Acceptance:
  screenshot compared with VISUAL.md by the user.

- [ ] **2.2 Control cards.**
  Origin: docs/VISUAL.md. Scope: cards for Mode, Fan speed, Wind direction, Special modes
  (WindFree first) as icon tiles (Segoe Fluent Icons), selected tile filled with accent; controls
  disabled while off (except power) and per `available*`. Acceptance: user walks the controls
  in the new layout on the real unit.

- [ ] **2.3 Light and dark theme, window sizing.**
  Origin: native look. Scope: follow system theme; Mica; compact default size (about 440×820),
  minimum width; content scrolls; keyboard focus visuals and access keys. Acceptance: user checks
  both themes.

## Phase 3 — Readings and maintenance

- [ ] **3.1 Energy card.** Power now (W) and total (kWh) from `powerConsumptionReport`.
  Acceptance: values match the SmartThings app.
- [ ] **3.2 Filter card.** Usage %, status (`wash`/`replace`), capacity; "Reset filter" with
  confirmation dialog → `resetDustFilter`. Acceptance: user resets after cleaning and sees 0 %.
- [ ] **3.3 Tropical night mode.** Hours 0-35 (0 = off) with remaining time when active.
  Acceptance: user enables and sees it on the unit/SmartThings.
- [ ] **3.4 Auto clean.** Toggle `setAutoCleaningMode`; show `operatingState` while cleaning.
- [ ] **3.5 Beep sound.** Toggle via `audioVolume.setVolume` 0/100. Acceptance: unit stops beeping.
- [ ] **3.6 Display light (experimental).** Try `samsungce.airConditionerLighting` on/off on the
  real unit (capability is listed as disabled). If the unit ignores it, try the documented
  `execute` fallback; if neither works, leave this bullet unchecked with the blocker noted and
  hide the control.
- [ ] **3.7 Self-check and device info.** Start/cancel self-check, show progress and error codes;
  read-only firmware/model info in an expander.

## Phase 4 — Desktop integration and settings

- [ ] **4.1 Settings page.** Language (System / Português / English) applied without restart;
  change token; forget token (returns to setup); close-to-tray; start minimized.
  Adds `ISettingsStore` + `JsonSettingsStore`.
- [ ] **4.2 Tray icon.** Adds H.NotifyIcon.WinUI. Icon reflects on/off; tooltip with room and
  target temperature; left click shows window; menu: Turn on/off, Open, Exit. 60 s refresh while
  hidden.
- [ ] **4.3 Remember window position and size.**
- [ ] **4.4 App icon and title bar polish.** Own `.ico`, used by window, taskbar and tray.

## Phase 5 — Release

- [ ] **5.1 Publish and measure.** Final AOT publish; record start time and memory against the
  SPECS targets; fix what misses them (new bullets).
- [ ] **5.2 Start menu shortcut and autostart option.** Script to create a Start menu shortcut
  to the published exe; Settings option to start with Windows (Run key).
- [ ] **5.3 README final pass.** Run, test, publish, how to get a token.
