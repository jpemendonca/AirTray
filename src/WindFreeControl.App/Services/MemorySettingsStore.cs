using WindFreeControl.Core.Abstractions;

namespace WindFreeControl.App.Services;

public sealed class MemorySettingsStore : ISettingsStore
{
    private AppSettings _settings = new();

    public AppSettings Load() => _settings;

    public void Save(AppSettings settings) => _settings = settings;
}
