using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindFreeControl.Core.ViewModels;

namespace WindFreeControl.App.Views;

public sealed partial class OptionGroupView : UserControl
{
    public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(
        nameof(Group), typeof(OptionGroup), typeof(OptionGroupView), new PropertyMetadata(null, OnGroupChanged));

    public OptionGroupView()
    {
        InitializeComponent();
    }

    public OptionGroup Group
    {
        get => (OptionGroup)GetValue(GroupProperty);
        set => SetValue(GroupProperty, value);
    }

    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((OptionGroupView)d).Bindings.Update();
}
