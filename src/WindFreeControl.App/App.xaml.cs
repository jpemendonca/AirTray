using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WindFreeControl.App.Services;
using WindFreeControl.Core.Abstractions;
using WindFreeControl.Core.Localization;

namespace WindFreeControl.App;

public partial class App : Application
{
    private static DispatcherQueue? _dispatcher;
    private static MainWindow? _runningWindow;

    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };
    private MainWindow? _window;

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) => CrashLog.Write(e.Exception);
    }

    // Called on a background thread when a second launch redirects to this instance.
    public static void ShowRunningWindow() =>
        _dispatcher?.TryEnqueue(() => _runningWindow?.ShowFromTray());

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        var options = LaunchOptions.Parse(Environment.GetCommandLineArgs());

        // Demo runs never touch the real settings file or the Run registry key.
        ISettingsStore settings = options.Demo ? new MemorySettingsStore() : new JsonSettingsStore();
        ISystemIntegration system = options.Demo ? new NoSystemIntegration() : new StartupRegistration();
        var loaded = settings.Load();
        LanguageSettings.Apply(options.Language ?? loaded.Language);

        _window = new MainWindow(_http, new CredentialLockerTokenStore(), settings, system, options.Demo, options.DemoRejectToken);
        _window.ShowStart();
        if (options.Demo && options.SnapshotPath is { } path)
        {
            if (options.Page == "settings")
            {
                _window.ShowSettingsForSnapshot();
            }

            _window.ShowOffscreen(options.Theme, options.SnapshotWidth);
            await Task.Delay(options.SnapshotDelay);
            await Snapshot.SaveAsync(options.SnapshotWidth > 440 ? _window.RootElement : _window.SnapshotTarget, path);
            Exit();
            return;
        }

        _runningWindow = _window;
        _window.Open(startHidden: options.Startup || loaded.StartMinimized);
    }
}
