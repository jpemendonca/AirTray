using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using WindFreeControl.Core.ViewModels;

namespace WindFreeControl.App.Views;

// Tiles are built in code: binding ItemsSource to a collection of Core types crashes under the
// CsWinRT projection, because Core has no WinRT interop code for them.
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

    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var view = (OptionGroupView)d;
        if (e.OldValue is OptionGroup old)
        {
            old.Items.CollectionChanged -= view.OnItemsChanged;
            old.PropertyChanged -= view.OnGroupPropertyChanged;
        }

        if (e.NewValue is OptionGroup group)
        {
            group.Items.CollectionChanged += view.OnItemsChanged;
            group.PropertyChanged += view.OnGroupPropertyChanged;
        }

        view.Bindings.Update();
        view.Rebuild();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void OnGroupPropertyChanged(object? sender, PropertyChangedEventArgs e) => Sync();

    private void Rebuild()
    {
        Tiles.Children.Clear();
        if (Group is null)
        {
            return;
        }

        for (var i = 0; i < Group.Items.Count; i++)
        {
            var index = i;
            var tile = new ToggleButton
            {
                Content = Group.Items[i].Label,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0, 0, 8, 8),
            };
            tile.Click += (_, _) =>
            {
                Group.SelectedIndex = index;
                Sync();
            };
            Tiles.Children.Add(tile);
        }

        Sync();
    }

    private void Sync()
    {
        for (var i = 0; i < Tiles.Children.Count && i < Group.Items.Count; i++)
        {
            var tile = (ToggleButton)Tiles.Children[i];
            tile.IsChecked = i == Group.SelectedIndex;
            tile.IsEnabled = Group.IsEnabled && Group.Items[i].IsAvailable;
        }
    }
}
