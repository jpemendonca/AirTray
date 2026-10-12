namespace WindFreeControl.Core.Abstractions;

public sealed record AppSettings
{
    public const string SystemLanguage = "system";

    public string Language { get; init; } = SystemLanguage;

    public bool CloseToTray { get; init; } = true;

    public bool StartMinimized { get; init; }

    public bool StartWithWindows { get; init; }

    public WindowBounds? Window { get; init; }
}
