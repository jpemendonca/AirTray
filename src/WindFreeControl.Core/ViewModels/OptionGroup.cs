using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WindFreeControl.Core.Device;
using WindFreeControl.Core.Localization;

namespace WindFreeControl.Core.ViewModels;

public sealed partial class OptionGroup(string title, string labelPrefix, Action<string> onUserSelected) : ObservableObject
{
    private bool _applying;

    public string Title { get; } = title;

    public ObservableCollection<OptionItem> Items { get; } = [];

    [ObservableProperty]
    public partial int SelectedIndex { get; set; } = -1;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; } = true;

    public bool HasItems => Items.Count > 0;

    public void Apply(OptionSet options, bool enabled)
    {
        _applying = true;
        try
        {
            var items = options.Supported
                .Select(v => new OptionItem(v, Labels.ForValue(labelPrefix, v), options.IsAvailable(v)))
                .ToList();
            if (!items.SequenceEqual(Items))
            {
                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
                }

                OnPropertyChanged(nameof(HasItems));
            }

            SelectedIndex = options.Current is null ? -1 : items.FindIndex(i => i.Value == options.Current);
            IsEnabled = enabled;
        }
        finally
        {
            _applying = false;
        }
    }

    partial void OnSelectedIndexChanged(int value)
    {
        if (_applying || value < 0 || value >= Items.Count || !Items[value].IsAvailable)
        {
            return;
        }

        onUserSelected(Items[value].Value);
    }
}
