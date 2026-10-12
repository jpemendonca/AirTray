namespace WindFreeControl.App.Services;

public static class CrashLog
{
    public static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WindFreeControl", "crash.log");

    public static void Write(Exception exception)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.AppendAllText(FilePath, $"{DateTimeOffset.Now:O}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}");
    }
}
