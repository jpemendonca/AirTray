using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WindFreeControl.App.Views;
using WindFreeControl.Core.ViewModels;
using Windows.Graphics;

namespace WindFreeControl.App;

// Compact panel shown above the clock when the tray icon is clicked, like the volume or
// brightness flyouts. Hidden from the taskbar and Alt+Tab; it hides itself when it loses focus.
public sealed partial class FlyoutWindow : Window
{
    private const int WidthDip = 340;
    private const int MarginDip = 12;

    private static readonly TimeSpan ReopenGuard = TimeSpan.FromMilliseconds(400);

    private DateTimeOffset _hiddenAt;

    public FlyoutWindow()
    {
        SystemBackdrop = new DesktopAcrylicBackdrop();
        AppWindow.IsShownInSwitchers = false;
        var presenter = OverlappedPresenter.Create();
        presenter.SetBorderAndTitleBar(true, false);
        presenter.IsResizable = false;
        presenter.IsMaximizable = false;
        presenter.IsMinimizable = false;
        presenter.IsAlwaysOnTop = true;
        AppWindow.SetPresenter(presenter);
        Activated += (_, e) =>
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated)
            {
                Hide();
            }
        };
    }

    public event EventHandler? OpenRequested;

    public event EventHandler? SettingsRequested;

    public bool IsOpen => AppWindow.IsVisible;

    public void SetViewModel(MainViewModel? viewModel)
    {
        if (viewModel is null)
        {
            Content = null;
            return;
        }

        var panel = new FlyoutPanel(viewModel);
        panel.OpenRequested += (_, _) =>
        {
            Hide();
            OpenRequested?.Invoke(this, EventArgs.Empty);
        };
        panel.SettingsRequested += (_, _) =>
        {
            Hide();
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        };
        Content = panel;
    }

    // A tray click first deactivates the open flyout (hiding it), then arrives here; the guard
    // keeps that same click from reopening it.
    public void Toggle()
    {
        if (IsOpen || DateTimeOffset.Now - _hiddenAt < ReopenGuard)
        {
            Hide();
            return;
        }

        ShowNearClock();
    }

    public void Hide()
    {
        if (AppWindow.IsVisible)
        {
            AppWindow.Hide();
            _hiddenAt = DateTimeOffset.Now;
        }
    }

    private void ShowNearClock()
    {
        if (Content is not FrameworkElement content)
        {
            return;
        }

        var scale = GetDpiForWindow(Win32Interop.GetWindowFromWindowId(AppWindow.Id)) / 96.0;
        content.Measure(new Windows.Foundation.Size(WidthDip, double.PositiveInfinity));
        var width = (int)(WidthDip * scale);
        var heightDip = content.DesiredSize.Height > 0 ? content.DesiredSize.Height : 300;
        var height = (int)(heightDip * scale) + 2;
        var margin = (int)(MarginDip * scale);

        var work = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary).WorkArea;
        var x = work.X + work.Width - width - margin;
        var y = work.Y + work.Height - height - margin;
        AppWindow.MoveAndResize(new RectInt32(x, y, width, height));
        AppWindow.Show(true);
        Activate();
    }

    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForWindow(nint window);
}
