namespace WindFreeControl.Core.Device;

public sealed record TemperatureRange(double Minimum, double Maximum, double Step)
{
    public static readonly TemperatureRange Default = new(16, 30, 1);

    public double Clamp(double value) => Math.Clamp(value, Minimum, Maximum);
}
