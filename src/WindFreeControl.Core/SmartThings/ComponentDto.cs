namespace WindFreeControl.Core.SmartThings;

public sealed record ComponentDto(string Id, IReadOnlyList<CapabilityRefDto>? Capabilities);
