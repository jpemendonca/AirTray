namespace WindFreeControl.Core.Tests;

public class FixturesTests
{
    [Theory(DisplayName = "Real payload fixtures are copied to the test output and parse as JSON")]
    [InlineData("status.json")]
    [InlineData("devices.json")]
    public void Load_WithRealPayload_ReturnsObjectRoot(string name)
    {
        // arrange
        var fileName = name;

        // act
        using var document = Fixtures.Load(fileName);

        // assert
        Assert.Equal(System.Text.Json.JsonValueKind.Object, document.RootElement.ValueKind);
    }
}
