namespace WindFreeControl.Core.Device;

public sealed record OptionSet(string? Current, IReadOnlyList<string> Supported, IReadOnlyList<string>? Available)
{
    public static readonly OptionSet Empty = new(null, [], null);

    public bool IsAvailable(string value) => Available is null || Available.Contains(value);
}
