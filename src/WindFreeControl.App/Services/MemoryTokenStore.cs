using WindFreeControl.Core.Abstractions;

namespace WindFreeControl.App.Services;

public sealed class MemoryTokenStore : ITokenStore
{
    private string? _token;

    public string? Load() => _token;

    public void Save(string token) => _token = token;

    public void Clear() => _token = null;
}
