using Microsoft.UI.Xaml;

namespace WindFreeControl.App.Look;

public static class Show
{
    public static Visibility Unless(bool value) => value ? Visibility.Collapsed : Visibility.Visible;
}
