using WindFreeControl.Core.Abstractions;

namespace WindFreeControl.App.Services;

public sealed class NoSystemIntegration : ISystemIntegration
{
    public void SetStartWithWindows(bool enabled)
    {
    }
}
