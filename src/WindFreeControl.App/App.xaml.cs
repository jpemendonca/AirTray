using Microsoft.UI.Xaml;
using WindFreeControl.App.Services;

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
        _window = new MainWindow(_http, new CredentialLockerTokenStore(), options.Demo);
        _window.ShowStart();
        if (options.Demo && options.SnapshotPath is { } path)
        {
            _window.ShowOffscreen(options.Theme);
            await Task.Delay(options.SnapshotDelay);
            await Snapshot.SaveAsync(_window.SnapshotTarget, path);
            Exit();
            return;
        }

        _window.Activate();
    }
}
