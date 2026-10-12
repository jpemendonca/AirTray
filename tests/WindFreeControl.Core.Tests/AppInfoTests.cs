namespace WindFreeControl.Core.Tests;

public class AppInfoTests
{
    [Fact(DisplayName = "The app name shown in the title bar is WindFree Control")]
    public void Name_Always_IsWindFreeControl()
    {
        // arrange
        const string expected = "WindFree Control";

        // act
        var name = AppInfo.Name;

        // assert
        Assert.Equal(expected, name);
    }
}
