using System.Text.Json;

namespace WindFreeControl.Core.Device;

public sealed record AcState
{
    public bool IsOn { get; init; }

    public double? RoomTemperature { get; init; }

    public double? Humidity { get; init; }

    public double? TargetTemperature { get; init; }

    public TemperatureRange TargetRange { get; init; } = TemperatureRange.Default;

    public OptionSet Mode { get; init; } = OptionSet.Empty;

    public OptionSet FanMode { get; init; } = OptionSet.Empty;

    public OptionSet WindDirection { get; init; } = OptionSet.Empty;

    public OptionSet OptionalMode { get; init; } = OptionSet.Empty;

    public static AcState Parse(JsonDocument status)
    {
        var main = StatusReader.Main(status);
        return new AcState
        {
            IsOn = main.String("switch", "switch") == "on",
            RoomTemperature = main.Number("temperatureMeasurement", "temperature"),
            Humidity = main.Number("relativeHumidityMeasurement", "humidity"),
            TargetTemperature = main.Number("thermostatCoolingSetpoint", "coolingSetpoint"),
            TargetRange = main.Range("thermostatCoolingSetpoint", "coolingSetpointRange") ?? TemperatureRange.Default,
            Mode = main.Options("airConditionerMode", "airConditionerMode", "supportedAcModes", "availableAcModes"),
            FanMode = main.Options("airConditionerFanMode", "fanMode", "supportedAcFanModes", "availableAcFanModes"),
            WindDirection = main.Options("fanOscillationMode", "fanOscillationMode", "supportedFanOscillationModes", "availableFanOscillationModes"),
            OptionalMode = main.Options("custom.airConditionerOptionalMode", "acOptionalMode", "supportedAcOptionalMode", "availableAcOptionalMode"),
        };
    }
}
