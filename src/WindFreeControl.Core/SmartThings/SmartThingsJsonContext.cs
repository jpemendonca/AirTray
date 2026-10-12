using System.Text.Json.Serialization;

namespace WindFreeControl.Core.SmartThings;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DeviceListDto))]
[JsonSerializable(typeof(ErrorEnvelopeDto))]
internal sealed partial class SmartThingsJsonContext : JsonSerializerContext;
