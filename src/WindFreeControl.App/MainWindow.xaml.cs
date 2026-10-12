using Microsoft.UI.Xaml;
using WindFreeControl.Core;

namespace WindFreeControl.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = AppInfo.Name;
        TitleText.Text = AppInfo.Name;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBarArea);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(440, 820));
    }
}
