namespace WindFreeControl.App.Services;

public sealed record TrayMenuItem(string Text, Action Invoke)
{
    public static readonly TrayMenuItem Separator = new("", () => { });
}
