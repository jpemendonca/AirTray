using Microsoft.UI.Xaml.Controls;
using WindFreeControl.Core.ViewModels;

namespace WindFreeControl.App.Views;

public sealed partial class SetupPage : Page
{
    public SetupPage(SetupViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    public SetupViewModel ViewModel { get; }
}
