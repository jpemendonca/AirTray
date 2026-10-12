using WindFreeControl.Core.SmartThings;

namespace WindFreeControl.Core.Setup;

public static class DeviceDiscovery
{
    public static DeviceDto? PickAirConditioner(IReadOnlyList<DeviceDto> devices) =>
        devices.FirstOrDefault(d => d.IsAirConditioner);
}
