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
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow(_http, new CredentialLockerTokenStore());
        _window.ShowStart();
        _window.Activate();
    }
}
