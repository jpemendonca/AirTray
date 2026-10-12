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

    public double? PowerWatts { get; init; }

    public double? EnergyTotalWh { get; init; }

    public double? TropicalNightHours { get; init; }

    public double? TropicalNightRemainingMinutes { get; init; }

    public bool? AutoClean { get; init; }

    public bool IsAutoCleaning { get; init; }

    public double? Volume { get; init; }

    public double? FilterUsagePercent { get; init; }

    public string? FilterStatus { get; init; }

    public double? FilterCapacityHours { get; init; }

    public string? SelfCheckStatus { get; init; }

    public double? SelfCheckProgress { get; init; }

    public IReadOnlyList<string> SelfCheckErrors { get; init; } = [];

    public string? Firmware { get; init; }

    public string? WifiModule { get; init; }

    public double? ReleaseYear { get; init; }

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
            PowerWatts = main.ObjectNumber("powerConsumptionReport", "powerConsumption", "power"),
            EnergyTotalWh = main.ObjectNumber("powerConsumptionReport", "powerConsumption", "energy"),
            TropicalNightHours = main.Number("custom.airConditionerTropicalNightMode", "acTropicalNightModeLevel"),
            TropicalNightRemainingMinutes = main.Number("custom.airConditionerTropicalNightMode", "remainingTime"),
            AutoClean = main.String("custom.autoCleaningMode", "autoCleaningMode") is { } clean ? clean != "off" : null,
            IsAutoCleaning = main.String("custom.autoCleaningMode", "operatingState") is { } cleaning && cleaning != "ready",
            Volume = main.Number("audioVolume", "volume"),
            FilterUsagePercent = main.Number("custom.dustFilter", "dustFilterUsage"),
            FilterStatus = main.String("custom.dustFilter", "dustFilterStatus"),
            FilterCapacityHours = main.Number("custom.dustFilter", "dustFilterCapacity"),
            SelfCheckStatus = main.String("samsungce.selfCheck", "status"),
            SelfCheckProgress = main.Number("samsungce.selfCheck", "progress"),
            SelfCheckErrors = main.ErrorCodes("samsungce.selfCheck", "errors"),
            Firmware = main.String("ocf", "mnfv"),
            WifiModule = main.String("samsungce.deviceIdentification", "description"),
            ReleaseYear = main.Number("samsungce.deviceIdentification", "releaseYear"),
        };
    }
}
