using System.Text;
using WindFreeControl.Core.SmartThings;

namespace WindFreeControl.Core.Tests.SmartThings;

public class CommandsTests
{
    public static TheoryData<DeviceCommand, string> Cases => new()
    {
        { Commands.Power(false), """{"component":"main","capability":"switch","command":"off","arguments":[]}""" },
        { Commands.TargetTemperature(23), """{"component":"main","capability":"thermostatCoolingSetpoint","command":"setCoolingSetpoint","arguments":[23]}""" },
        { Commands.Mode("cool"), """{"component":"main","capability":"airConditionerMode","command":"setAirConditionerMode","arguments":["cool"]}""" },
        { Commands.FanMode("turbo"), """{"component":"main","capability":"airConditionerFanMode","command":"setFanMode","arguments":["turbo"]}""" },
        { Commands.WindDirection("all"), """{"component":"main","capability":"fanOscillationMode","command":"setFanOscillationMode","arguments":["all"]}""" },
        { Commands.TropicalNight(8), """{"component":"main","capability":"custom.airConditionerTropicalNightMode","command":"setAcTropicalNightModeLevel","arguments":[8]}""" },
        { Commands.AutoClean(true), """{"component":"main","capability":"custom.autoCleaningMode","command":"setAutoCleaningMode","arguments":["on"]}""" },
        { Commands.Volume(0), """{"component":"main","capability":"audioVolume","command":"setVolume","arguments":[0]}""" },
        { Commands.ResetFilter(), """{"component":"main","capability":"custom.dustFilter","command":"resetDustFilter","arguments":[]}""" },
        { Commands.SelfCheck(true), """{"component":"main","capability":"samsungce.selfCheck","command":"startSelfCheck","arguments":[]}""" },
        { Commands.OptionalMode("windFree"), """{"component":"main","capability":"custom.airConditionerOptionalMode","command":"setAcOptionalMode","arguments":["windFree"]}""" },
    };

    [Theory(DisplayName = "Each command serializes to the exact body SmartThings expects")]
    [MemberData(nameof(Cases))]
    public void Write_WithSingleCommand_ProducesExpectedJson(DeviceCommand command, string expected)
    {
        // arrange
        var commands = new[] { command };

        // act
        var json = Encoding.UTF8.GetString(CommandBody.Write(commands));

        // assert
        Assert.Equal($$"""{"commands":[{{expected}}]}""", json);
    }
}
