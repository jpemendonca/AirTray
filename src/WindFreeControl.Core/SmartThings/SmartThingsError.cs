namespace WindFreeControl.Core.SmartThings;

public sealed record SmartThingsError(SmartThingsErrorKind Kind, string? Message);
