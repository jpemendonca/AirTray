using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindFreeControl.Core.Localization;
using WindFreeControl.Core.ViewModels;

namespace WindFreeControl.App.Views;

public sealed partial class ControlPage : Page
{
    public ControlPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    public MainViewModel ViewModel { get; }

    public FrameworkElement ScrollContent => ContentPanel;

    private async void OnResetFilterClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = Strings.Filter_ResetConfirmTitle,
            Content = Strings.Filter_ResetConfirmText,
            PrimaryButtonText = Strings.Dialog_Reset,
            CloseButtonText = Strings.Dialog_Cancel,
            DefaultButton = ContentDialogButton.Close,
        };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.ResetFilterCommand.ExecuteAsync(null);
        }
    }
}
