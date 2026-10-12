namespace WindFreeControl.Core.SmartThings;

public sealed record DeviceCommand(string Capability, string Command, IReadOnlyList<CommandArgument> Arguments)
{
    public const string MainComponent = "main";

    public string Component { get; init; } = MainComponent;
}
