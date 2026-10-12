using System.Text.Json;

namespace WindFreeControl.Core.Device;

internal readonly struct StatusReader(JsonElement component)
{
    public static StatusReader Main(JsonDocument status) =>
        status.RootElement.TryGetProperty("components", out var components)
        && components.TryGetProperty("main", out var main)
            ? new StatusReader(main)
            : new StatusReader(default);

    public JsonElement? Value(string capability, string attribute)
    {
        if (component.ValueKind != JsonValueKind.Object
            || !component.TryGetProperty(capability, out var cap)
            || !cap.TryGetProperty(attribute, out var attr)
            || !attr.TryGetProperty("value", out var value)
            || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value;
    }

    public string? String(string capability, string attribute) =>
        Value(capability, attribute) is { ValueKind: JsonValueKind.String } v ? v.GetString() : null;

    public double? Number(string capability, string attribute) =>
        Value(capability, attribute) is { ValueKind: JsonValueKind.Number } v ? v.GetDouble() : null;

    public double? ObjectNumber(string capability, string attribute, string property) =>
        Value(capability, attribute) is { ValueKind: JsonValueKind.Object } obj
        && obj.TryGetProperty(property, out var number)
        && number.ValueKind == JsonValueKind.Number
            ? number.GetDouble()
            : null;

    // Self-check errors arrive as strings or as objects with a "code" property, depending on firmware.
    public IReadOnlyList<string> ErrorCodes(string capability, string attribute)
    {
        if (Value(capability, attribute) is not { ValueKind: JsonValueKind.Array } array)
        {
            return [];
        }

        return [.. array.EnumerateArray()
            .Select(item => item.ValueKind switch
            {
                JsonValueKind.String => item.GetString(),
                JsonValueKind.Object when item.TryGetProperty("code", out var code) => code.ToString(),
                _ => item.ToString(),
            })
            .Where(code => !string.IsNullOrEmpty(code))
            .Select(code => code!)];
    }

    public IReadOnlyList<string>? StringList(string capability, string attribute)
    {
        if (Value(capability, attribute) is not { ValueKind: JsonValueKind.Array } array)
        {
            return null;
        }

        return [.. array.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)];
    }

    public OptionSet Options(string capability, string current, string supported, string available) =>
        new(String(capability, current), StringList(capability, supported) ?? [], StringList(capability, available));

    public TemperatureRange? Range(string capability, string attribute)
    {
        if (Value(capability, attribute) is not { ValueKind: JsonValueKind.Object } range
            || !range.TryGetProperty("minimum", out var min)
            || !range.TryGetProperty("maximum", out var max))
        {
            return null;
        }

        var step = range.TryGetProperty("step", out var s) && s.ValueKind == JsonValueKind.Number ? s.GetDouble() : 1;
        return new TemperatureRange(min.GetDouble(), max.GetDouble(), step);
    }
}
