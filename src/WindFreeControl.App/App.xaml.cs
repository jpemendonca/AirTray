using Microsoft.UI.Xaml;
using WindFreeControl.App.Services;
using WindFreeControl.Core.Abstractions;
using WindFreeControl.Core.Localization;

namespace WindFreeControl.App;

public partial class App : Application
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };
    private MainWindow? _window;

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) => CrashLog.Write(e.Exception);
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var options = LaunchOptions.Parse(Environment.GetCommandLineArgs());

        // Demo runs never touch the real settings file or the Run registry key.
        ISettingsStore settings = options.Demo ? new MemorySettingsStore() : new JsonSettingsStore();
        ISystemIntegration system = options.Demo ? new NoSystemIntegration() : new StartupRegistration();
        var loaded = settings.Load();
        LanguageSettings.Apply(options.Language ?? loaded.Language);

        _window = new MainWindow(_http, new CredentialLockerTokenStore(), settings, system, options.Demo);
        _window.ShowStart();
        if (options.Demo && options.SnapshotPath is { } path)
        {
            if (options.Page == "settings")
            {
                _window.ShowSettingsForSnapshot();
            }

            _window.ShowOffscreen(options.Theme);
            await Task.Delay(options.SnapshotDelay);
            await Snapshot.SaveAsync(_window.SnapshotTarget, path);
            Exit();
            return;
        }

        _window.Open(startHidden: options.Startup || loaded.StartMinimized);
    }
}
