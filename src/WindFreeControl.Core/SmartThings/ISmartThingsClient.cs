using System.Text.Json;

namespace WindFreeControl.Core.SmartThings;

public interface ISmartThingsClient
{
    Task<SmartThingsResult<IReadOnlyList<DeviceDto>>> GetDevicesAsync(CancellationToken cancellationToken);

    Task<SmartThingsResult<JsonDocument>> GetStatusAsync(string deviceId, CancellationToken cancellationToken);

    Task<SmartThingsResult<bool>> SendAsync(string deviceId, IReadOnlyList<DeviceCommand> commands, CancellationToken cancellationToken);
}
