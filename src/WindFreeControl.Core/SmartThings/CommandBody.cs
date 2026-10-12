using System.Text.Json;

namespace WindFreeControl.Core.SmartThings;

public static class CommandBody
{
    public static byte[] Write(IReadOnlyList<DeviceCommand> commands)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteStartArray("commands");
            foreach (var command in commands)
            {
                writer.WriteStartObject();
                writer.WriteString("component", command.Component);
                writer.WriteString("capability", command.Capability);
                writer.WriteString("command", command.Command);
                writer.WriteStartArray("arguments");
                foreach (var argument in command.Arguments)
                {
                    argument.WriteTo(writer);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return buffer.ToArray();
    }
}
