# Security

AirTray is a personal project. If you find a security problem, open a
[private security advisory](https://github.com/jpemendonca/AirTray/security/advisories/new)
instead of a public issue.

What the app does with your data:

- The SmartThings token is stored in the Windows Credential Locker and sent only to
  `https://api.smartthings.com`.
- Settings live in `%LocalAppData%\WindFreeControl\settings.json`; unhandled errors go to
  `crash.log` in the same folder (capped at 256 KB, never contains the token).
- No telemetry and no other network calls.
