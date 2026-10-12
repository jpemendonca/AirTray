namespace WindFreeControl.App.Services;

public static class CrashLog
{
    private const long MaxBytes = 256 * 1024;

    public static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WindFreeControl", "crash.log");

    // Runs inside the unhandled-exception handler, so it must never throw itself.
    public static void Write(Exception exception)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            if (File.Exists(FilePath) && new FileInfo(FilePath).Length > MaxBytes)
            {
                File.Delete(FilePath);
            }

            File.AppendAllText(FilePath, $"{DateTimeOffset.Now:O}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }
}
