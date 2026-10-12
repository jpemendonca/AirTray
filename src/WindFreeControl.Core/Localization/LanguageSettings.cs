using System.Globalization;
using WindFreeControl.Core.Abstractions;

namespace WindFreeControl.Core.Localization;

public static class LanguageSettings
{
    public static readonly IReadOnlyList<string> Choices = [AppSettings.SystemLanguage, "pt-BR", "en"];

    private static readonly CultureInfo SystemCulture = CultureInfo.CurrentUICulture;

    public static void Apply(string language)
    {
        var culture = language == AppSettings.SystemLanguage
            ? SystemCulture
            : CultureInfo.GetCultureInfo(language);
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        Strings.Culture = culture;
    }
}
