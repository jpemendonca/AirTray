using WindFreeControl.Core.Device;

namespace WindFreeControl.Core.Tests.Device;

public class AcStateTests
{
    [Fact(DisplayName = "Parsing the real status reads every main control and its options")]
    public void Parse_WithRealStatusFixture_ReadsMainControls()
    {
        // arrange
        using var status = Fixtures.Load("status.json");

        // act
        var state = AcState.Parse(status);

        // assert
        Assert.True(state.IsOn);
        Assert.Equal(25, state.RoomTemperature);
        Assert.Equal(68, state.Humidity);
        Assert.Equal(25, state.TargetTemperature);
        Assert.Equal(new TemperatureRange(16, 30, 1), state.TargetRange);
        Assert.Equal("cool", state.Mode.Current);
        Assert.Equal(["auto", "cool", "dry", "wind"], state.Mode.Supported);
        Assert.Equal("auto", state.FanMode.Current);
        Assert.Equal(["auto", "low", "medium", "high", "turbo"], state.FanMode.Supported);
        Assert.Equal("vertical", state.WindDirection.Current);
        Assert.Null(state.WindDirection.Available);
        Assert.Equal("off", state.OptionalMode.Current);
        Assert.Contains("windFree", state.OptionalMode.Supported);
        Assert.Equal(216, state.PowerWatts);
        Assert.Equal(2808106, state.EnergyTotalWh);
        Assert.Equal(100, state.FilterUsagePercent);
        Assert.Equal("wash", state.FilterStatus);
        Assert.False(state.AutoClean);
        Assert.Equal(0, state.Volume);
        Assert.Equal("ready", state.SelfCheckStatus);
    }
}
