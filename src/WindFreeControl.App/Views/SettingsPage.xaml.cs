using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindFreeControl.Core;
using WindFreeControl.Core.ViewModels;

namespace WindFreeControl.App.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        // Items are added in code: binding ItemsSource to a Core list crashes WinUI (see CODESTYLE.md).
        var index = viewModel.LanguageIndex;
        foreach (var name in viewModel.LanguageNames)
        {
            LanguageBox.Items.Add(name);
        }

        LanguageBox.SelectedIndex = index;
    }

    public SettingsViewModel ViewModel { get; }

    public FrameworkElement ScrollContent => ContentPanel;

    public string VersionText { get; } =
        $"{AppInfo.Name} {typeof(SettingsPage).Assembly.GetName().Version?.ToString(3)}";
}
