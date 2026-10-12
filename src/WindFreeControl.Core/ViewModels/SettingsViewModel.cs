using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindFreeControl.Core.Abstractions;
using WindFreeControl.Core.Localization;

namespace WindFreeControl.Core.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore _settings;
    private readonly ITokenStore _tokens;
    private readonly ISystemIntegration _system;
    private readonly bool _loading;

    public SettingsViewModel(ISettingsStore settings, ITokenStore tokens, ISystemIntegration system)
    {
        _settings = settings;
        _tokens = tokens;
        _system = system;
        var current = settings.Load();
        _loading = true;
        LanguageIndex = Math.Max(0, LanguageSettings.Choices.ToList().IndexOf(current.Language));
        CloseToTray = current.CloseToTray;
        StartMinimized = current.StartMinimized;
        StartWithWindows = current.StartWithWindows;
        _loading = false;
    }

    public event EventHandler? LanguageChanged;

    public event EventHandler? TokenChangeRequested;

    public event EventHandler? Closed;

    public IReadOnlyList<string> LanguageNames { get; } =
        [Strings.Settings_LanguageSystem, "Português (Brasil)", "English"];

    [ObservableProperty]
    public partial int LanguageIndex { get; set; }

    [ObservableProperty]
    public partial bool CloseToTray { get; set; }

    [ObservableProperty]
    public partial bool StartMinimized { get; set; }

    [ObservableProperty]
    public partial bool StartWithWindows { get; set; }

    partial void OnLanguageIndexChanged(int value)
    {
        if (_loading || value < 0)
        {
            return;
        }

        var language = LanguageSettings.Choices[value];
        Update(s => s with { Language = language });
        LanguageSettings.Apply(language);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnCloseToTrayChanged(bool value) => Update(s => s with { CloseToTray = value });

    partial void OnStartMinimizedChanged(bool value) => Update(s => s with { StartMinimized = value });

    partial void OnStartWithWindowsChanged(bool value)
    {
        if (_loading)
        {
            return;
        }

        _system.SetStartWithWindows(value);
        Update(s => s with { StartWithWindows = value });
    }

    [RelayCommand]
    private void ChangeToken() => TokenChangeRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void ForgetToken()
    {
        _tokens.Clear();
        TokenChangeRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Close() => Closed?.Invoke(this, EventArgs.Empty);

    private void Update(Func<AppSettings, AppSettings> change)
    {
        if (!_loading)
        {
            _settings.Save(change(_settings.Load()));
        }
    }
}
