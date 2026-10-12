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

        var delay = int.TryParse(ValueAfter("--snapshot-delay"), out var seconds) ? seconds : 5;
        return new LaunchOptions(
            args.Contains("--demo"),
            args.Contains("--startup"),
            ValueAfter("--snapshot"),
            TimeSpan.FromSeconds(delay),
            ValueAfter("--theme"),
            ValueAfter("--lang"),
            ValueAfter("--page"),
            int.TryParse(ValueAfter("--snapshot-width"), out var width) ? width : 440,
            args.Contains("--demo-reject-token"));
    }
}
