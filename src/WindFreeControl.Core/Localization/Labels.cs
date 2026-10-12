using System.Globalization;
using WindFreeControl.Core.SmartThings;

namespace WindFreeControl.Core.Localization;

public static class Labels
{
    public static string ForValue(string prefix, string value) =>
        Strings.ResourceManager.GetString(prefix + value, Strings.Culture) ?? value;

    public static string Format(string template, object? value) =>
        string.Format(CultureInfo.CurrentCulture, template, value);

    public static string ForError(SmartThingsError error) => error.Kind switch
    {
        SmartThingsErrorKind.Unauthorized => Strings.Error_Unauthorized,
        SmartThingsErrorKind.Forbidden => Strings.Error_Forbidden,
        SmartThingsErrorKind.Rejected => Format(Strings.Error_Rejected, error.Message),
        SmartThingsErrorKind.RateLimited => Strings.Error_RateLimited,
        SmartThingsErrorKind.Network => Strings.Error_Network,
        _ => Format(Strings.Error_Unexpected, error.Message),
    };

    public static string Temperature(double celsius) =>
        celsius.ToString("0.#", CultureInfo.CurrentCulture) + "°";
}
