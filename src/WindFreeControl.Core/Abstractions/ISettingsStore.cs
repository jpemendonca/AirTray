namespace WindFreeControl.Core.Abstractions;

public interface ISettingsStore
{
    AppSettings Load();

    void Save(AppSettings settings);
}
