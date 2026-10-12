using System.Text.Json;

namespace WindFreeControl.Core.Tests;

internal static class Fixtures
{
    public static string Read(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    public static JsonDocument Load(string name) => JsonDocument.Parse(Read(name));
}
