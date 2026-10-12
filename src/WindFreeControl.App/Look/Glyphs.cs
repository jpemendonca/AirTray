namespace WindFreeControl.App.Look;

public static class Glyphs
{
    public const string Power = "";
    public const string Thermometer = "";
    public const string Drop = "";
    public const string Settings = "";

    private static readonly Dictionary<string, string> Values = new()
    {
        ["Mode_auto"] = "",
        ["Mode_cool"] = "",
        ["Mode_dry"] = "",
        ["Mode_wind"] = "",
        ["Mode_heat"] = "",
        ["Fan_auto"] = "",
        ["Fan_low"] = "",
        ["Fan_medium"] = "",
        ["Fan_high"] = "",
        ["Fan_turbo"] = "",
        ["Wind_fixed"] = "",
        ["Wind_vertical"] = "",
        ["Wind_horizontal"] = "",
        ["Wind_all"] = "",
        ["Optional_off"] = "",
        ["Optional_sleep"] = "",
        ["Optional_quiet"] = "",
        ["Optional_smart"] = "",
        ["Optional_speed"] = "",
        ["Optional_windFree"] = "",
        ["Optional_windFreeSleep"] = "",
    };

    private static readonly Dictionary<string, string> Groups = new()
    {
        ["Mode_"] = "",
        ["Fan_"] = "",
        ["Wind_"] = "",
        ["Optional_"] = "",
    };

    public static string ForValue(string prefix, string value) =>
        Values.TryGetValue(prefix + value, out var glyph) ? glyph : "";

    public static string ForGroup(string prefix) =>
        Groups.TryGetValue(prefix, out var glyph) ? glyph : "";
}
