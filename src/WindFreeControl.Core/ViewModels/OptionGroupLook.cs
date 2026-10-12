namespace WindFreeControl.Core.ViewModels;

public static class OptionGroupLook
{
    public static double Opacity(bool isAvailable) => isAvailable ? 1.0 : 0.4;
}
