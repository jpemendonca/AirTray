using System.Text.Json;
using System.Text.Json.Nodes;

namespace WindFreeControl.Core.SmartThings;

public sealed class DemoSmartThingsClient : ISmartThingsClient
{
    private const string DeviceId = "demo";

    private static readonly Dictionary<(string Capability, string Command), string> Targets = new()
    {
        [("switch", "on")] = "switch",
        [("switch", "off")] = "switch",
        [("thermostatCoolingSetpoint", "setCoolingSetpoint")] = "coolingSetpoint",
        [("airConditionerMode", "setAirConditionerMode")] = "airConditionerMode",
        [("airConditionerFanMode", "setFanMode")] = "fanMode",
        [("fanOscillationMode", "setFanOscillationMode")] = "fanOscillationMode",
        [("custom.airConditionerOptionalMode", "setAcOptionalMode")] = "acOptionalMode",
        [("custom.airConditionerTropicalNightMode", "setAcTropicalNightModeLevel")] = "acTropicalNightModeLevel",
        [("custom.autoCleaningMode", "setAutoCleaningMode")] = "autoCleaningMode",
        [("audioVolume", "setVolume")] = "volume",
        [("samsungce.airConditionerLighting", "on")] = "lighting",
        [("samsungce.airConditionerLighting", "off")] = "lighting",
        [("custom.dustFilter", "resetDustFilter")] = "dustFilterUsage",
        [("samsungce.selfCheck", "startSelfCheck")] = "status",
        [("samsungce.selfCheck", "cancelSelfCheck")] = "status",
    };

    private readonly JsonNode _status;

    public DemoSmartThingsClient()
    {
        using var stream = typeof(DemoSmartThingsClient).Assembly.GetManifestResourceStream("WindFreeControl.Core.Demo.status.json")
            ?? throw new InvalidOperationException("Demo status resource is missing.");
        _status = JsonNode.Parse(stream)!;
    }

    public Task<SmartThingsResult<IReadOnlyList<DeviceDto>>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<DeviceDto> devices =
            [new DeviceDto(DeviceId, "Samsung-Room-Air-Conditioner", "Ar-condicionado", new OcfDto(DeviceDto.AirConditionerOcfType), [])];
        return Task.FromResult(SmartThingsResult<IReadOnlyList<DeviceDto>>.Success(devices));
    }

    public Task<SmartThingsResult<JsonDocument>> GetStatusAsync(string deviceId, CancellationToken cancellationToken) =>
        Task.FromResult(SmartThingsResult<JsonDocument>.Success(JsonDocument.Parse(_status.ToJsonString())));

    public Task<SmartThingsResult<bool>> SendAsync(string deviceId, IReadOnlyList<DeviceCommand> commands, CancellationToken cancellationToken)
    {
        foreach (var command in commands)
        {
            if (!Targets.TryGetValue((command.Capability, command.Command), out var attribute))
            {
                return Task.FromResult(SmartThingsResult<bool>.Failure(
                    new SmartThingsError(SmartThingsErrorKind.Rejected, $"{command.Capability}.{command.Command}")));
            }

            var value = command.Command switch
            {
                "resetDustFilter" => JsonValue.Create(0),
                "startSelfCheck" => JsonValue.Create("run"),
                "cancelSelfCheck" => JsonValue.Create("ready"),
                _ when command.Arguments.Count > 0 => command.Arguments[0].ToJsonNode(),
                _ => JsonValue.Create(command.Command),
            };
            _status["components"]!["main"]![command.Capability]![attribute]!["value"] = value;
            if (command.Command == "resetDustFilter")
            {
                _status["components"]!["main"]!["custom.dustFilter"]!["dustFilterStatus"]!["value"] = "normal";
            }

            if (command.Command == "setAcOptionalMode")
            {
                ApplyEconomyFloor(command.Arguments[0].ToString() == "smart");
            }
        }

        return Task.FromResult(SmartThingsResult<bool>.Success(true));
    }

    // The real unit raises the minimum setpoint to 24 °C while economy (smart) mode is on.
    private void ApplyEconomyFloor(bool economy)
    {
        var main = _status["components"]!["main"]!;
        var minimum = economy ? 24 : 16;
        main["thermostatCoolingSetpoint"]!["coolingSetpointRange"]!["value"]!["minimum"] = minimum;
        var setpoint = main["thermostatCoolingSetpoint"]!["coolingSetpoint"]!["value"]!.GetValue<double>();
        if (setpoint < minimum)
        {
            main["thermostatCoolingSetpoint"]!["coolingSetpoint"]!["value"] = minimum;
        }
    }
}
