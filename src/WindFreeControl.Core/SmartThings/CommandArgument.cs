using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WindFreeControl.Core.SmartThings;

public readonly record struct CommandArgument
{
    private readonly string? _text;
    private readonly double _number;
    private readonly JsonNode? _json;

    private CommandArgument(string? text, double number, JsonNode? json = null)
    {
        _text = text;
        _number = number;
        _json = json;
    }

    public static implicit operator CommandArgument(string text) => new(text, 0);

    public static implicit operator CommandArgument(double number) => new(null, number);

    public static CommandArgument FromJson(JsonNode json) => new(null, 0, json);

    public void WriteTo(Utf8JsonWriter writer)
    {
        if (_json is not null)
        {
            _json.WriteTo(writer);
        }
        else if (_text is not null)
        {
            writer.WriteStringValue(_text);
        }
        else
        {
            writer.WriteNumberValue(_number);
        }
    }

    public JsonNode ToJsonNode() =>
        _json?.DeepClone() ?? (_text is not null ? JsonValue.Create(_text) : JsonValue.Create(_number));

    public override string ToString() =>
        _json?.ToJsonString() ?? _text ?? _number.ToString(CultureInfo.InvariantCulture);
}
