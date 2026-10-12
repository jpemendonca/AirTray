using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace WindFreeControl.App.Look;

public static class ModeAccent
{
    private static readonly SolidColorBrush Cool = new(Color.FromArgb(255, 0x3B, 0x82, 0xF6));
    private static readonly SolidColorBrush Dry = new(Color.FromArgb(255, 0x14, 0xB8, 0xA6));
    private static readonly SolidColorBrush Wind = new(Color.FromArgb(255, 0x22, 0xC5, 0x5E));
    private static readonly SolidColorBrush Auto = new(Color.FromArgb(255, 0x8B, 0x5C, 0xF6));
    private static readonly SolidColorBrush Heat = new(Color.FromArgb(255, 0xF9, 0x73, 0x16));
    private static readonly SolidColorBrush Off = new(Colors.Gray);

    public static SolidColorBrush Brush(string key) => key switch
    {
        "cool" => Cool,
        "dry" => Dry,
        "wind" => Wind,
        "auto" => Auto,
        "heat" => Heat,
        _ => Off,
    };

    public static SolidColorBrush Foreground(string key) =>
        key == "off" ? Off : new SolidColorBrush(Colors.White);
}
