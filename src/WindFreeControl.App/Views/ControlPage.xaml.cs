using Microsoft.UI.Xaml.Controls;
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
}
