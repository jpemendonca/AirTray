# CODESTYLE.md

Only rules you can check by reading a diff.

## Language

- Code, identifiers, comments, commit messages, docs for agents: English.
- Text the user reads on screen: resource strings only, in pt-BR (`Strings.resx`) and English
  (`Strings.en.resx`). Never a literal user-facing string in C# or XAML.

```xml
<!-- wrong -->
<TextBlock Text="Temperatura desejada" />
<!-- right -->
<TextBlock Text="{x:Bind ViewModel.Text.TargetTemperature, Mode=OneWay}" />
```

- Adding a key to one `.resx` without the other fails the localization test.

## Comments

- Comment only when the *why* is not obvious. No comment that repeats the code.
- No commented-out code, no section banners, no file headers, no `#region`.
- Short sentences, active voice, one word for one meaning. Speak about the code, not about the
  change that produced it ("Debounces setpoint clicks", not "Added debounce").

## C# conventions

- File-scoped namespaces. Namespace follows folder: `WindFreeControl.Core.SmartThings`.
- One public type per file, file named after it.
- `Nullable` enabled, `ImplicitUsings` enabled, `TreatWarningsAsErrors` true (Directory.Build.props).
- `PascalCase` types/members, `_camelCase` private fields, `camelCase` locals/parameters,
  `Async` suffix on methods returning `Task`.
- Prefer `sealed` classes and `record` for data. `AcState` and DTOs are immutable.
- Every async API takes a `CancellationToken` as the last parameter.
- `HttpClient` is created once and injected; never `new HttpClient()` per call.

## AOT rules

- XAML binds with `x:Bind` only. `{Binding}` is a review failure.
- JSON via `JsonSerializerContext` source generation. Every serialized type is listed in
  `SmartThingsJsonContext` or `SettingsJsonContext`.

```csharp
// wrong
JsonSerializer.Deserialize<DeviceList>(json);
// right
JsonSerializer.Deserialize(json, SmartThingsJsonContext.Default.DeviceList);
```

- No `Activator.CreateInstance`, `Type.GetType(string)`, `Assembly.Load`, `dynamic`.
- Observable properties with CommunityToolkit.Mvvm partial properties
  (`[ObservableProperty] public partial bool IsOn { get; set; }`), not field-based `[ObservableProperty]`.

## Layers

- Core never references WinUI or `Windows.*`. Enforced: Core targets `net10.0`.
- Views never call `SmartThingsClient`. Views talk to view models only.
- View models never touch `DispatcherQueue`; they use `IUiDispatcher`.
- Platform services (token, settings, tray) are interfaces in `Core/Abstractions`, implemented in App.

## Errors

- User input (pasted token) is validated by a pure function returning all problems at once:
  `TokenValidation.Validate(string) -> IReadOnlyList<TokenProblem>`.
- API failures become a `SmartThingsError` result (kind + message), not exceptions that reach the
  UI. The view model turns it into an `InfoBar` message.
- Exceptions only for impossible state (bug alarm): `throw new UnreachableException(...)`.
- Never `catch (Exception)` and ignore. Catch the specific type and handle or rethrow.

## Tests

- Framework: xUnit v3. Test project mirrors Core folders.
- Fakes, not mocks library: `FakeHttpHandler`, `FakeTokenStore`, `ImmediateDispatcher`. No real
  network, never the real device.
- Name: `Method_Scenario_Behavior`. `DisplayName` with one sentence in English saying what is
  guaranteed. Body marked `// arrange`, `// act`, `// assert`.

```csharp
[Fact(DisplayName = "Parsing the real status reads WindFree as off and the unit as on")]
public void Parse_WithRealStatusFixture_ReadsPowerAndOptionalMode()
{
    // arrange
    using var json = Fixtures.Load("status.json");

    // act
    var state = AcState.Parse(json);

    // assert
    Assert.True(state.IsOn);
    Assert.Equal("off", state.OptionalMode);
}
```

- Fixtures under `tests/.../Fixtures/`, real payloads with device/location/owner ids replaced by
  `00000000-0000-0000-0000-000000000000`.

## Secrets

- The real token never appears in code, tests, fixtures, docs, logs or commits.
- Tests use `"test-token"`.
- Logging (Debug output only) never prints headers or the token.

## Git

- Conventional Commits: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`, `build:`.
- One logical change per commit. Never commit with a red gate. Never `--no-verify`.
- No AI attribution lines in commit messages.
