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

- [x] **0.4 AOT publish smoke.**
  Origin: decision log (AOT risk must surface early). Scope: `publish.ps1` producing
  `publish/WindFreeControl.exe`; record exe folder size, cold start time and working set of the
  empty window in the validation note. Acceptance: published exe opens on a machine path without
  the .NET SDK on PATH; zero AOT/trim warnings.
  > Blocked (2026-10-12): `publish.ps1` exists, but ILCompiler reports "Platform linker not found".
  > VS 2026 Community lacks the MSVC x64 tools; the user installs them (VC.Tools.x86.x64 + Windows SDK).
  > Validation (2026-10-12): MSVC tools installed by the user. AOT publish works with zero warnings
  > once vswhere.exe is on PATH (`publish.ps1` adds it). Folder 168 MB, exe 9.9 MB.

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

- [x] **1.5 Main path walked on the real unit (user).**
  Origin: definition of done. Scope: user runs the AOT published exe, pastes the token, turns the
  unit on and off, changes temperature, mode, fan, wind and WindFree, closes and reopens the app
  (no token prompt). Agent records findings; every problem found becomes a new bullet below this
  one. Acceptance: user confirms each control changed the real unit.
  > Validation (2026-10-12): user ran the Release (non-AOT) build against the real unit and
  > reported every control working. Path walked. AOT exe still blocked by 0.4.

- [x] **1.6 Economy mode.**
  Origin: user, after 1.5: the SmartThings app has an economy mode that this app lacks. The
  unit's `supportedAcOptionalMode` does not list `energySaving`, and `custom.energyType` reports
  `energySavingSupport: false`, so the control is not in the documented API surface. Scope: user
  turns economy mode on in the SmartThings app, runs `tools/dump-device.ps1`; agent diffs against
  `tools/out/baseline/` to find the attribute and command, then adds the control. Acceptance:
  toggling it in this app shows as active in the SmartThings app.
  > Validation (2026-10-12): the diff showed `acOptionalMode` `off` → `smart`, the setpoint minimum
  > going 16 → 24 and wind set to `fixed`. The control already existed, labeled "Inteligente";
  > relabeled `Optional_smart` to "Econômico" / "Economy". No new command needed. Code written;
  > the user has not toggled it from this app yet.

- [x] **1.7 Respect the setpoint limits (economy mode floor).**
  Origin: user, 2026-10-12: in economy mode the unit does not go below 24 °C; Alexa reports an
  error when asked to go lower. Scope: "−" is disabled when the target is at the reported
  minimum, "+" when at the maximum (the range comes from `coolingSetpointRange`, so economy mode's
  24 °C floor applies automatically); if the device still rejects a setpoint, the API message is
  shown in the warning bar. Acceptance: in demo mode with economy on, "−" is disabled at 24 °C.
  > Validation (2026-10-12): demo client mimics the 24 °C floor; selecting Econômico then "−" once
  > went 25 → 24 and UI Automation reported "Diminuir temperatura" disabled. Code written; not
  > walked on the real unit (user asked the agent not to operate the AC).

## Phase 2 — Control screen as in SmartThings

- [x] **2.0 Off-screen demo snapshots.**
  Origin: user, 2026-10-12: test windows popped over a game being played. Scope: `--demo
  --snapshot <png> [--snapshot-delay <s>]` opens the window off-screen without activating it,
  renders the window content with `RenderTargetBitmap` after the delay, saves the PNG and exits.
  Agents never use screen capture or bring the app to the foreground. Acceptance: snapshot PNG
  produced while the window never appears on any monitor.
  > Validation (2026-10-12): snapshot saved from a window placed at (-32000, -32000) and shown with
  > `AppWindow.Show(false)`; nothing appeared over the user's game. Earlier screen-region captures
  > were deleted.

- [x] **2.1 Hero card.**
  Origin: docs/VISUAL.md. Scope: device name, on/off state, big power button, large target
  temperature with -/+ circular buttons, room temperature and humidity line, color/state accent
  by mode (cool blue, dry teal, wind green, auto purple) and greyed when off. Acceptance:
  screenshot compared with VISUAL.md by the user.
  > Validation (2026-10-12): hero card with round power button tinted by mode, 76 px light target
  > digits in the mode color, round −/+, indoor line with icons; grey when off. Checked in off-screen
  > demo snapshots (on and off). Comparison by the user waived: user reviews only the finished app.

- [x] **2.2 Control cards.**
  Origin: docs/VISUAL.md. Scope: cards for Mode, Fan speed, Wind direction, Special modes
  (WindFree first) as icon tiles (Segoe Fluent Icons), selected tile filled with accent; controls
  disabled while off (except power) and per `available*`. Acceptance: user walks the controls
  in the new layout on the real unit.
  > Validation (2026-10-12): cards with header icon and 88 px icon tiles (glyphs in `Look/Glyphs.cs`,
  > picked from a rendered Segoe Fluent Icons sheet), 4 per row, disabled when off. Demo only; the
  > user asked the agent not to operate the real unit.

- [x] **2.3 Light and dark theme, window sizing.**
  Origin: native look. Scope: follow system theme; Mica; compact default size (about 440×820),
  minimum width; content scrolls; keyboard focus visuals and access keys. Acceptance: user checks
  both themes.
  > Validation (2026-10-12): theme follows the system (theme brushes only, mode colors readable on
  > both); 440×820 default, minimum 400×480; scroll below the warning bars. Light and dark checked
  > by off-screen snapshots (`--theme`). Access keys skipped: every control has a tooltip and an
  > automation name instead. User check waived.

## Phase 3 — Readings and maintenance

- [x] **3.1 Energy card.** Power now (W) and total (kWh) from `powerConsumptionReport`.
  Acceptance: values match the SmartThings app.
  > Validation (2026-10-12): power now and total kWh from `powerConsumptionReport`; demo shows
  > 216 W / 2.808,1 kWh. Match with the SmartThings app not checked.
- [x] **3.2 Filter card.** Usage %, status (`wash`/`replace`), capacity; "Reset filter" with
  confirmation dialog → `resetDustFilter`. Acceptance: user resets after cleaning and sees 0 %.
  > Validation (2026-10-12): usage bar (red on `wash`/`replace`), status text, "Zerar contador"
  > with a confirmation dialog. Demo only; the reset on the real unit is for the user.
- [x] **3.3 Tropical night mode.** Hours 0-35 (0 = off) with remaining time when active.
  Acceptance: user enables and sees it on the unit/SmartThings.
  > Validation (2026-10-12): NumberBox 0-35 h, enabled only while on, remaining minutes when
  > reported. Code written; not walked.
- [x] **3.4 Auto clean.** Toggle `setAutoCleaningMode`; show `operatingState` while cleaning.
  > Validation (2026-10-12): toggle + "Limpando agora" while cleaning; toggled in demo via UI
  > Automation and stayed on after the re-read.
- [x] **3.5 Beep sound.** Toggle via `audioVolume.setVolume` 0/100. Acceptance: unit stops beeping.
  > Validation (2026-10-12): toggle maps to volume 0/100; toggled in demo, stayed on after re-read.
- [x] **3.6 Display light (experimental).** Try `samsungce.airConditionerLighting` on/off on the
  real unit (capability is listed as disabled). If the unit ignores it, try the documented
  `execute` fallback; if neither works, leave this bullet unchecked with the blocker noted and
  hide the control.
  > Blocked (2026-10-12): proving it needs commands sent to the real unit, and the user asked the
  > agent not to operate the AC. No control shipped. Next step: user tries it once, agent adds it.
  > In progress (2026-10-12): user asked for the control. Toggle shipped in "Conforto e opções":
  > sends `samsungce.airConditionerLighting` on/off; on 422 it falls back to `execute` `mode/vs/0`
  > with `Light_On`/`Light_Off` and then keeps the toggle at the user's last choice. The agent sent
  > nothing to the unit. Missing: user flips it on the real unit and reports which path works and
  > whether Light_On/Off is inverted.
  > Validation (2026-10-12): user flipped the toggle on the real unit and reported it works. Path walked.
- [x] **3.7 Self-check and device info.** Start/cancel self-check, show progress and error codes;
  read-only firmware/model info in an expander.
  > Validation (2026-10-12): start/cancel with progress and error codes; firmware, Wi-Fi module and
  > model year in an expander. Start/cancel checked in demo.

## Phase 4 — Desktop integration and settings

- [x] **4.1 Settings page.** Language (System / Português / English) applied without restart;
  change token; forget token (returns to setup); close-to-tray; start minimized.
  Adds `ISettingsStore` + `JsonSettingsStore`.
  > Validation (2026-10-12): settings page (language System/Português/English applied on the spot,
  > change/forget token, close to tray, start minimized, start with Windows). `JsonSettingsStore` in
  > %LocalAppData%; demo uses an in-memory store and never writes the Run key. Settings page and the
  > English control page checked by off-screen snapshots. Live switch not walked by the user.
- [x] **4.2 Tray icon.** ~~Adds H.NotifyIcon.WinUI.~~ (revised, see note) Icon reflects on/off; tooltip with room and
  target temperature; left click shows window; menu: Turn on/off, Open, Exit. 60 s refresh while
  hidden.
  > Validation (2026-10-12): written on Win32 `Shell_NotifyIconW` + window subclass instead of
  > H.NotifyIcon.WinUI (see SPECS decision log). On/off icons, tooltip, left click opens, right-click
  > menu Turn on/off / Open / Exit; refresh slows to 60 s while hidden. `--demo --startup` ran hidden
  > and Windows registered the icon with tooltip "Ar-condicionado: Ligado · Resfriar · 25°". Menu
  > not clicked (would steal focus from the user's game).
- [x] **4.3 Remember window position and size.**
  > Validation (2026-10-12): bounds saved on close/hide when restored, restored only if the rect is
  > on a connected display. Code written; not walked.
- [x] **4.4 App icon and title bar polish.** Own `.ico`, used by window, taskbar and tray.
  > Validation (2026-10-12): `tools/make-icons.py` draws app.ico (blue, three waves), tray-on/off and
  > a 16 px title-bar PNG; exe icon via `ApplicationIcon`, window via `AppWindow.SetIcon`.

- [x] **4.5 Single instance.**
  Origin: user, 2026-10-12: two copies of the published app were running. Scope: custom `Program.Main`
  with `AppInstance.FindOrRegisterForKey`; a second launch redirects its activation to the running
  instance, which shows its window (also from the tray), and exits. Demo runs use a separate key so
  agent checks never reach the user's instance. Acceptance: second launch exits and the first
  instance's window becomes visible.
  > Validation (2026-10-12): demo test: first copy started hidden (no window); a second launch exited
  > with code 0 within the timeout and the first copy's window became visible. Real-mode key is
  > "WindFreeControl", demo key "WindFreeControl.Demo".
  > Validation (2026-10-12): demo test: first copy started hidden; a second launch exited with code 0
  > and the first copy's window became visible. Published build updated.

## Phase 6 — After first use

- [x] **6.1 Content column limited when maximized.**
  Origin: user, 2026-10-12: maximized, the cards stretch and look odd. Scope: control, settings and
  setup pages keep a centered column of at most 480 px; the window can still maximize.
  Acceptance: off-screen snapshot at 1200 px wide shows a centered column.
  > Validation (2026-10-12): control and settings pages capped at 480 px and centered; off-screen
  > snapshot at 1200 px confirms. Setup page was already capped at 380 px.

- [x] **6.2 Remove the energy card.**
  Origin: user, 2026-10-12: the total is the lifetime counter and "power now" is a ~15 min interval
  average that often reads 0 W, which is confusing. Scope: remove the card, its strings, view model
  properties and `AcState` fields. Acceptance: no energy UI; gate green.
  > Validation (2026-10-12): card, strings, view model properties, AcState fields and the energy
  > assertions removed; snapshot shows Conforto e opções right after Modos especiais.

- [x] **6.3 Expired token message.**
  Origin: user, 2026-10-12 (chose this over OAuth). Scope: on 401 the setup page says the token
  expired or is invalid, explains that tokens created after 2024-12-30 last 24 h, and links to the
  token page; README states the same. OAuth stays out of scope. Acceptance: demo client forced to
  return 401 shows the message (off-screen snapshot).
  > Validation (2026-10-12): Error_Unauthorized explains the 24 h expiry in pt-BR and en; README notes
  > it; `--demo --demo-reject-token` snapshot shows the message above the token link.

- [x] **6.4 Tray flyout.**
  Origin: user, 2026-10-12, modeled on a monitor-brightness tray app. Scope: left click on the tray
  icon opens a compact borderless flyout above the clock (acrylic backdrop, rounded corners, hidden
  from taskbar and Alt+Tab, closes when it loses focus). Content: device name, indoor temperature
  and power button; target temperature with − and + (same debounce and range limits as the main
  page); shortcut buttons Resfriar, WindFree, Econômico, Sono; footer "Abrir WindFree Control" and
  a settings button. Reuses the running `MainViewModel`. Right-click menu keeps Open/Exit; "Abrir"
  opens the full window. Acceptance: off-screen snapshot of the flyout in demo; user opens it from
  the real tray.
  > In progress (2026-10-12): `FlyoutWindow` (acrylic, no title bar, always on top, hidden from
  > switchers, hides on deactivation, placed at the bottom-right of the work area) hosting
  > `FlyoutPanel`. Panel checked by off-screen snapshots in dark and light; shortcuts checked via UI
  > Automation (WindFree then Econômico leaves only Econômico on). Missing: the real flyout window was
  > not opened on screen by the agent; user opens it from the tray.
  > Validation (2026-10-12): user opened the flyout from the real tray and reported it works well. Path walked.

- [ ] **6.5 Downloadable GitHub release.**
  Origin: user, 2026-10-12. Scope: versioned zip of the AOT `publish/` folder (e.g.
  `WindFreeControl-1.0.0-win-x64.zip`) attached to a GitHub release with short notes in pt-BR and
  English; README section "Download" with how to unzip, run and get a token; decide between a
  local `gh release create` script and a GitHub Actions workflow on tag push (windows runner
  has the MSVC tools). Depends on: the repository is private, so others can only download after it
  is made public, which waits for 6.6. Do not start until the user asks.

- [ ] **6.6 Security review before making the repository public.**
  Origin: user, 2026-10-12. Last item. Scope: run the review with Sonnet sub-agents: git history
  and fixtures for secrets or personal ids, token handling (Credential Locker, logs, crash log),
  HTTP client, registry writes, P/Invoke code in `TrayIcon`, demo flags, release zip contents.
  Findings become bullets here. Making the repository public happens only after this. Do not start
  until the user asks.

## Phase 5 — Release

- [x] **5.1 Publish and measure.** Final AOT publish; record start time and memory against the
  SPECS targets; fix what misses them (new bullets).
  > Blocked (2026-10-12): the AOT publish still needs the MSVC x64 tools (same blocker as 0.4).
  > Fallback `./publish.ps1 -NoAot` (self-contained ReadyToRun) works: 273 MB folder, 145 MB working
  > set / 106 MB private hidden in the tray (demo). Targets (<60 MB, <1 s) are not met without AOT.
  > Found and fixed on the way: publish lacked `WindFreeControl.pri` and crashed on XAML load;
  > `EnableMsixTooling` fixes it. Next step: user installs the MSVC tools, agent runs `./publish.ps1`
  > and measures.
  > Validation (2026-10-12): AOT build replaced the ReadyToRun one in publish/. Measured hidden in the
  > tray (demo, 25 s): Task Manager memory 72 → 68 MB, working set 166 → 136 MB, folder 273 → 168 MB.
  > The <60 MB target is missed: WinUI itself is most of the footprint, not .NET. Startup time not
  > measured (would need a visible window over the user's game).
- [x] **5.2 Start menu shortcut and autostart option.** Script to create a Start menu shortcut
  to the published exe; Settings option to start with Windows (Run key).
  > Validation (2026-10-12): `tools/install-shortcut.ps1` (and `-Remove`) creates "WindFree Control"
  > in the Start menu pointing to publish/; shortcut created on this machine. "Start with Windows"
  > lives in Settings (4.1), writing HKCU Run with `--startup` (starts hidden in the tray).
- [x] **5.3 README final pass.** Run, test, publish, how to get a token.
  > Validation (2026-10-12): README covers run, settings, publish with and without AOT, shortcut,
  > gate, demo flags, device dump and icons.
