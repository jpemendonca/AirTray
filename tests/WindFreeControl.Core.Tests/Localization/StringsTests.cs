using System.Globalization;
using System.Resources;

namespace WindFreeControl.Core.Tests.Localization;

public class StringsTests
{
    [Fact(DisplayName = "Portuguese and English resources have exactly the same keys")]
    public void ResourceSets_ForPortugueseAndEnglish_HaveSameKeys()
    {
        // arrange
        var manager = WindFreeControl.Core.Localization.Strings.ResourceManager;

        // act
        var portuguese = Keys(manager, CultureInfo.InvariantCulture);
        var english = Keys(manager, CultureInfo.GetCultureInfo("en"));

        // assert
        Assert.NotEmpty(portuguese);
        Assert.Equal(portuguese, english);
    }

    private static SortedSet<string> Keys(ResourceManager manager, CultureInfo culture)
    {
        var set = manager.GetResourceSet(culture, createIfNotExists: true, tryParents: false)!;
        return [.. set.Cast<System.Collections.DictionaryEntry>().Select(e => (string)e.Key)];
    }
}
