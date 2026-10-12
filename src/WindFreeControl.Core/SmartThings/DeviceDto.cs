namespace WindFreeControl.Core.SmartThings;

public sealed record DeviceDto(
    string DeviceId,
    string? Name,
    string? Label,
    OcfDto? Ocf,
    IReadOnlyList<ComponentDto>? Components)
{
    public const string AirConditionerOcfType = "oic.d.airconditioner";

    public bool IsAirConditioner =>
        Ocf?.OcfDeviceType == AirConditionerOcfType
        || (Components ?? []).Any(c => (c.Capabilities ?? []).Any(cap => cap.Id == "airConditionerMode"));

    public string DisplayName => Label ?? Name ?? DeviceId;
}
