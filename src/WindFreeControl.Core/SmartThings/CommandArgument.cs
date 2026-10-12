using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WindFreeControl.Core.SmartThings;

public readonly record struct CommandArgument
{
    private readonly string? _text;
    private readonly double _number;

    private CommandArgument(string? text, double number)
    {
        _text = text;
        _number = number;
    }

    public static implicit operator CommandArgument(string text) => new(text, 0);

    public static implicit operator CommandArgument(double number) => new(null, number);

    public void WriteTo(Utf8JsonWriter writer)
    {
        if (_text is not null)
        {
            writer.WriteStringValue(_text);
        }
        else
        {
            writer.WriteNumberValue(_number);
        }
    }

    public JsonNode ToJsonNode() =>
        _text is not null ? JsonValue.Create(_text) : JsonValue.Create(_number);

    public override string ToString() =>
        _text ?? _number.ToString(CultureInfo.InvariantCulture);
}
