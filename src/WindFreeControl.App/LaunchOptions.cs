namespace WindFreeControl.App;

public sealed record LaunchOptions(
    bool Demo,
    bool Startup,
    string? SnapshotPath,
    TimeSpan SnapshotDelay,
    string? Theme,
    string? Language,
    string? Page,
    int SnapshotWidth,
    bool DemoRejectToken)
{
    public static LaunchOptions Parse(IReadOnlyList<string> args)
    {
        string? ValueAfter(string name)
        {
            var index = args.ToList().IndexOf(name);
            return index >= 0 && index + 1 < args.Count ? args[index + 1] : null;
        }

        var delay = int.TryParse(ValueAfter("--snapshot-delay"), out var seconds) ? Math.Clamp(seconds, 1, 60) : 5;

        // Snapshots only ever write a PNG, given as a full path, so the flag cannot overwrite other files.
        var snapshot = ValueAfter("--snapshot");
        if (snapshot is not null && (!Path.IsPathFullyQualified(snapshot) || !snapshot.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
        {
            snapshot = null;
        }
        return new LaunchOptions(
            args.Contains("--demo"),
            args.Contains("--startup"),
            snapshot,
            TimeSpan.FromSeconds(delay),
            ValueAfter("--theme"),
            ValueAfter("--lang"),
            ValueAfter("--page"),
            int.TryParse(ValueAfter("--snapshot-width"), out var width) ? Math.Clamp(width, 320, 2400) : 440,
            args.Contains("--demo-reject-token"));
    }
}
