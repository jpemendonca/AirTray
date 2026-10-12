namespace WindFreeControl.Core.Abstractions;

public interface ITokenStore
{
    string? Load();

    void Save(string token);

    void Clear();
}
