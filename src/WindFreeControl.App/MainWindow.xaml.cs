using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindFreeControl.App.Services;
using WindFreeControl.App.Views;
using WindFreeControl.Core;
using WindFreeControl.Core.Abstractions;
using WindFreeControl.Core.Localization;
using WindFreeControl.Core.SmartThings;
using WindFreeControl.Core.ViewModels;
using Windows.Graphics;

namespace WindFreeControl.App;

public sealed partial class MainWindow : Window
{
    private static readonly string AssetsFolder = Path.Combine(AppContext.BaseDirectory, "Assets");

    private readonly HttpClient _http;
    private readonly ITokenStore _tokens;
    private readonly ISettingsStore _settings;
    private readonly ISystemIntegration _system;
    private readonly bool _demo;
    private TrayIcon? _tray;
    private MainViewModel? _control;
    private ControlPage? _controlPage;
    private bool _exiting;

    public MainWindow(HttpClient http, ITokenStore tokens, ISettingsStore settings, ISystemIntegration system, bool demo)
    {
        _http = http;
        _tokens = tokens;
        _settings = settings;
        _system = system;
        _demo = demo;
        InitializeComponent();
        Title = demo ? $"{AppInfo.Name} (demo)" : AppInfo.Name;
        TitleText.Text = Title;
        SettingsButton.SetValue(ToolTipService.ToolTipProperty, Strings.Settings_Open);
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(SettingsButton, Strings.Settings_Open);
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBarArea);
        AppWindow.SetIcon(Path.Combine(AssetsFolder, "app.ico"));
        AppWindow.Resize(new SizeInt32(440, 820));
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = 400;
            presenter.PreferredMinimumHeight = 480;
        }

        AppWindow.Closing += OnClosing;
        Closed += (_, _) =>
        {
            ReleaseControl();
            _tray?.Dispose();
        };
    }

    public UIElement SnapshotTarget
    {
        get
        {
            var content = Host.Content switch
            {
                ControlPage page => page.ScrollContent,
                SettingsPage page => page.ScrollContent,
                _ => null,
            };
            if (content is not Panel panel)
            {
                return RootGrid;
            }

            panel.Background = RootGrid.Background;
            return panel;
        }
    }

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
        AppWindow.MoveAndResize(new RectInt32(-32000, -32000, 440, 820));
        AppWindow.Show(false);
    }

    public void ShowSettingsForSnapshot() => ShowSettings();

    public void Open(bool startHidden)
    {
        RestoreBounds();
        _tray = new TrayIcon(Win32Interop.GetWindowFromWindowId(AppWindow.Id));
        _tray.Activated += (_, _) => ShowFromTray();
        _tray.MenuItems = BuildTrayMenu;
        UpdateTray();

        if (startHidden)
        {
            _control?.SetWindowHidden(true);
        }
        else
        {
            Activate();
        }
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
        SettingsButton.Visibility = Visibility.Collapsed;
        var viewModel = new SetupViewModel(_tokens, CreateClient) { Error = message };
        viewModel.Connected += (_, _) => ShowStart();
        Host.Content = new SetupPage(viewModel);
    }

    private async void ShowControl(string token)
    {
        ReleaseControl();
        SettingsButton.Visibility = Visibility.Visible;
        var viewModel = new MainViewModel(CreateClient(token));
        viewModel.Unauthorized += (_, message) => ShowSetup(message);
        viewModel.PropertyChanged += OnControlPropertyChanged;
        _control = viewModel;
        _controlPage = new ControlPage(viewModel);
        Host.Content = _controlPage;
        try
        {
            await viewModel.StartAsync();
        }
        catch (OperationCanceledException) when (_control != viewModel)
        {
        }
    }

    private void ShowSettings()
    {
        var languageChanged = false;
        var viewModel = new SettingsViewModel(_settings, _tokens, _system);
        viewModel.LanguageChanged += (_, _) =>
        {
            languageChanged = true;
            ShowSettings();
            TitleText.Text = Title;
            SettingsButton.SetValue(ToolTipService.ToolTipProperty, Strings.Settings_Open);
        };
        viewModel.TokenChangeRequested += (_, _) => ShowSetup(null);
        viewModel.Closed += (_, _) =>
        {
            if (languageChanged || _controlPage is null)
            {
                ShowStart();
            }
            else
            {
                SettingsButton.Visibility = Visibility.Visible;
                Host.Content = _controlPage;
            }
        };
        SettingsButton.Visibility = Visibility.Collapsed;
        Host.Content = new SettingsPage(viewModel);
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e) => ShowSettings();

    private void OnControlPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.IsOn) or nameof(MainViewModel.TrayText))
        {
            UpdateTray();
        }
    }

    private void UpdateTray()
    {
        var on = _control?.IsOn == true;
        var text = _control?.TrayText is { Length: > 0 } trayText ? trayText : AppInfo.Name;
        _tray?.Update(Path.Combine(AssetsFolder, on ? "tray-on.ico" : "tray-off.ico"), text);
    }

    private IReadOnlyList<TrayMenuItem> BuildTrayMenu()
    {
        var items = new List<TrayMenuItem>();
        if (_control is { } control)
        {
            items.Add(new TrayMenuItem(
                control.IsOn ? Strings.Tray_TurnOff : Strings.Tray_TurnOn,
                () => control.TogglePowerCommand.Execute(null)));
        }

        items.Add(new TrayMenuItem(Strings.Tray_Open, ShowFromTray));
        items.Add(TrayMenuItem.Separator);
        items.Add(new TrayMenuItem(Strings.Tray_Exit, ExitApp));
        return items;
    }

    private void OnClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        SaveBounds();
        if (!_exiting && _tray is not null && _settings.Load().CloseToTray)
        {
            args.Cancel = true;
            AppWindow.Hide();
            _control?.SetWindowHidden(true);
        }
    }

    private void ShowFromTray()
    {
        AppWindow.Show();
        Activate();
        _control?.SetWindowHidden(false);
    }

    private void ExitApp()
    {
        _exiting = true;
        Close();
        Application.Current.Exit();
    }

    private void SaveBounds()
    {
        if (_demo || AppWindow.Presenter is not OverlappedPresenter { State: OverlappedPresenterState.Restored } || !AppWindow.IsVisible)
        {
            return;
        }

        var bounds = new WindowBounds(AppWindow.Position.X, AppWindow.Position.Y, AppWindow.Size.Width, AppWindow.Size.Height);
        _settings.Save(_settings.Load() with { Window = bounds });
    }

    private void RestoreBounds()
    {
        if (_settings.Load().Window is not { } saved)
        {
            return;
        }

        var rect = new RectInt32(saved.X, saved.Y, saved.Width, saved.Height);
        if (DisplayArea.GetFromRect(rect, DisplayAreaFallback.None) is not null)
        {
            AppWindow.MoveAndResize(rect);
        }
    }

    private void ReleaseControl()
    {
        if (_control is not null)
        {
            _control.PropertyChanged -= OnControlPropertyChanged;
            _control.Dispose();
        }

        _control = null;
        _controlPage = null;
        UpdateTray();
    }
}
