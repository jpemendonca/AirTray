# AGENTS.md

WindFree Control is a small native Windows app (WinUI 3, .NET 10, Native AOT) that controls one
Samsung WindFree air conditioner through the SmartThings cloud REST API. Personal use, one user,
one device. It must open fast, use little memory and look like a native Windows 11 app, with the
spirit of the SmartThings mobile app's air-conditioner screen.

## Main path

> Open the app. On first use, paste the SmartThings token. The app finds the air conditioner and
> shows its state: on/off, room temperature, humidity, target temperature, mode, fan speed, wind
> direction and WindFree. Tap power to turn it on or off, raise or lower the target temperature,
> change mode / fan / wind / WindFree, and see the screen reflect what the device really did.
> Close the app. Next time it opens straight on the control screen.

Every bullet in [ROADMAP.md](ROADMAP.md) is judged against this path. If a bullet does not move
the user along it (or expand it later), question it.

## Repository areas

| Folder | What it is | State |
|---|---|---|
| `src/WindFreeControl.Core` | SmartThings client, device model, view models. No WinUI reference. | active |
| `src/WindFreeControl.App` | WinUI 3 shell: XAML views, token storage, tray icon, startup. | active |
| `tests/WindFreeControl.Core.Tests` | Unit tests for Core (xUnit v3). | active |
| `tools/` | Developer scripts. `tools/out/` is local output, git-ignored. | active |
| `docs/` | Reference material: visual teardown, real device payloads. | active |

## Read before working

1. [CODESTYLE.md](CODESTYLE.md) — always.
2. [SPECS.md](SPECS.md) — architecture, API contract, decisions.
3. [ROADMAP.md](ROADMAP.md) — the work queue.
4. [docs/VISUAL.md](docs/VISUAL.md) — only when the bullet touches layout or styling.

## Work loop

1. Take the first unchecked bullet in ROADMAP.md. Do not skip ahead. Do not batch unrelated bullets.
2. Implement it following CODESTYLE.md.
3. Run the quality gate (below). It must be green.
4. Check the bullet and add the dated `> Validation (YYYY-MM-DD): ...` note. Say whether the
   path was walked or only the code was written.
5. Commit with Conventional Commits. One logical change per commit.

If a bullet turns out wrong or impossible, do not redefine it silently. Leave it unchecked, add a
one-line note below it with the blocker, and move to the next one.

## Quality gate

```powershell
./gate.ps1
```

It runs `dotnet format --verify-no-changes`, `dotnet build -c Release -warnaserror` (including
AOT/trim analyzers) and `dotnet test`. The pre-commit hook in `.githooks/` calls it; enable it once
per clone with `git config core.hooksPath .githooks`.

The gate does not open the app, does not talk to the real device and does not prove a command
works. A green gate is necessary, not proof that the product works.

## Hard rules

- Every change becomes a ROADMAP bullet before it becomes code, including fixes the user asks for
  after testing. Only exception: changes that do not alter behavior (typo, formatting, link,
  local variable name). If the user reported it, it is a bullet.
- No new NuGet package without a ROADMAP bullet that asks for it.
- Warnings are errors. Fix the code; never suppress (`#pragma warning disable`, `NoWarn`,
  `[UnconditionalSuppressMessage]`) to make the gate pass.
- Never bypass the gate (`--no-verify`).
- Nothing enters without a caller. A class, command or view with no caller in production code is
  not a finished bullet. A test is not a caller.
- The agent never sees, writes or commits the real SmartThings token. The user types it into the
  app. Tests use obvious fakes (`"test-token"`).
- Never send a command to the real device from tests. Tests use a fake `HttpMessageHandler`.
- Only show controls the device reports as supported (see SPECS.md, "Supported vs available").
  Never hard-code a value list the device did not report.
- Native AOT compatible code only: `x:Bind`, never `{Binding}`; System.Text.Json with source
  generation; no runtime reflection.
- Commit messages never carry AI attribution lines (`Co-Authored-By` or similar).

## Repository map

```
WindFreeControl.slnx
gate.ps1                     quality gate
.githooks/pre-commit         calls gate.ps1
src/WindFreeControl.Core/
  SmartThings/               HTTP client, DTOs, JSON context, command builders
  Device/                    AcState (parsed status), capability enums and labels
  ViewModels/                MainViewModel, SetupViewModel (CommunityToolkit.Mvvm)
  Abstractions/              ITokenStore, IUiDispatcher
src/WindFreeControl.App/
  Views/                     SetupPage, ControlPage, controls
  Services/                  CredentialLockerTokenStore, dispatcher adapter
  App.xaml, MainWindow.xaml
tests/WindFreeControl.Core.Tests/
  Fixtures/                  real status/device payloads, ids scrubbed
docs/
  VISUAL.md                  appearance teardown of the SmartThings AC screen
  DEVICE.md                  what this specific unit supports (from tools/dump-device.ps1)
tools/dump-device.ps1        read-only dump of device, status and capability definitions
```
