using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using WindFreeControl.App.Look;
using WindFreeControl.Core.Localization;
using WindFreeControl.Core.ViewModels;

namespace WindFreeControl.App.Views;

public sealed partial class FlyoutPanel : UserControl
{
    private static readonly (string Prefix, string Value)[] ShortcutValues =
    [
        ("Mode_", "cool"),
        ("Optional_", "windFree"),
        ("Optional_", "smart"),
        ("Optional_", "sleep"),
    ];

    private readonly ToggleButton[] _shortcuts = new ToggleButton[ShortcutValues.Length];

    public FlyoutPanel(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        OpenButton.Click += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);
        SettingsButton.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);
        BuildShortcuts();
        viewModel.PropertyChanged += OnViewModelChanged;
        Unloaded += (_, _) => viewModel.PropertyChanged -= OnViewModelChanged;
        Sync();
    }

    public event EventHandler? OpenRequested;

    public event EventHandler? SettingsRequested;

    public MainViewModel ViewModel { get; }

    private void BuildShortcuts()
    {
        for (var i = 0; i < ShortcutValues.Length; i++)
        {
            var (prefix, value) = ShortcutValues[i];
            var label = Labels.ForValue(prefix, value);
            var content = new StackPanel { Spacing = 4, HorizontalAlignment = HorizontalAlignment.Center };
            content.Children.Add(new FontIcon { Glyph = Glyphs.ForValue(prefix, value), FontSize = 18 });
            content.Children.Add(new TextBlock { Text = label, FontSize = 11, TextAlignment = TextAlignment.Center });
            var tile = new ToggleButton
            {
                Content = content,
                Padding = new Thickness(2, 8, 2, 8),
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            ToolTipService.SetToolTip(tile, label);
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(tile, label);
            tile.Click += (_, _) =>
            {
                if (prefix == "Mode_")
                {
                    ViewModel.SelectModeCommand.Execute(value);
                }
                else
                {
                    ViewModel.ToggleOptionalModeCommand.Execute(value);
                }

                Sync();
            };
            Grid.SetColumn(tile, i);
            Shortcuts.Children.Add(tile);
            _shortcuts[i] = tile;
        }
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.CurrentMode) or nameof(MainViewModel.CurrentOptionalMode) or nameof(MainViewModel.IsOn))
        {
            Sync();
        }
    }

    private void Sync()
    {
        for (var i = 0; i < ShortcutValues.Length; i++)
        {
            var (prefix, value) = ShortcutValues[i];
            var supported = prefix == "Mode_" ? ViewModel.SupportedModes : ViewModel.SupportedOptionalModes;
            _shortcuts[i].IsChecked = prefix == "Mode_" ? ViewModel.CurrentMode == value : ViewModel.CurrentOptionalMode == value;
            _shortcuts[i].IsEnabled = ViewModel.IsOn && supported.Contains(value);
        }
    }
}
