using Microsoft.UI.Xaml;
using WindFreeControl.App.Views;
using WindFreeControl.Core;
using WindFreeControl.Core.Abstractions;
using WindFreeControl.Core.SmartThings;
using WindFreeControl.Core.ViewModels;

namespace WindFreeControl.App;

public sealed partial class MainWindow : Window
{
    private readonly HttpClient _http;
    private readonly ITokenStore _tokens;
    private readonly bool _demo;
    private MainViewModel? _control;

    public MainWindow(HttpClient http, ITokenStore tokens, bool demo)
    {
        _http = http;
        _tokens = tokens;
        _demo = demo;
        InitializeComponent();
        Title = demo ? $"{AppInfo.Name} (demo)" : AppInfo.Name;
        TitleText.Text = Title;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBarArea);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(440, 820));
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = 400;
            presenter.PreferredMinimumHeight = 480;
        }
        Closed += (_, _) => _control?.Dispose();
    }

    public UIElement Root => RootGrid;

    // Snapshot runs show the window outside every monitor and never activate it, so nothing
    // appears over what the user is doing.
    public void ShowOffscreen(string? theme)
    {
        RootGrid.RequestedTheme = theme switch
        {
            "light" => ElementTheme.Light,
            "dark" => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
        var light = RootGrid.ActualTheme == ElementTheme.Light;
        RootGrid.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
            light ? Windows.UI.Color.FromArgb(255, 0xF3, 0xF3, 0xF3) : Windows.UI.Color.FromArgb(255, 0x20, 0x20, 0x20));
        AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(-32000, -32000, 440, 1900));
        AppWindow.Show(false);
    }

    public void ShowStart()
    {
        if (_demo)
        {
            ShowControl("demo");
        }
        else if (_tokens.Load() is { } token)
        {
            ShowControl(token);
        }
        else
        {
            ShowSetup(null);
        }
    }

    private ISmartThingsClient CreateClient(string token) =>
        _demo ? new DemoSmartThingsClient() : new SmartThingsClient(_http, token);

    private void ShowSetup(string? message)
    {
        ReleaseControl();
        var viewModel = new SetupViewModel(_tokens, CreateClient) { Error = message };
        viewModel.Connected += (_, _) => ShowStart();
        Host.Content = new SetupPage(viewModel);
    }

    private async void ShowControl(string token)
    {
        ReleaseControl();
        var viewModel = new MainViewModel(CreateClient(token));
        viewModel.Unauthorized += (_, message) => ShowSetup(message);
        _control = viewModel;
        Host.Content = new ControlPage(viewModel);
        try
        {
            await viewModel.StartAsync();
        }
        catch (OperationCanceledException) when (_control != viewModel)
        {
        }
    }

    private void ReleaseControl()
    {
        _control?.Dispose();
        _control = null;
    }
}
