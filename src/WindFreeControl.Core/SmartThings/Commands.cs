namespace WindFreeControl.Core.SmartThings;

public static class Commands
{
    public static DeviceCommand Power(bool on) =>
        new("switch", on ? "on" : "off", []);

    public static DeviceCommand TargetTemperature(double celsius) =>
        new("thermostatCoolingSetpoint", "setCoolingSetpoint", [celsius]);

    public static DeviceCommand Mode(string mode) =>
        new("airConditionerMode", "setAirConditionerMode", [mode]);

    public static DeviceCommand FanMode(string fanMode) =>
        new("airConditionerFanMode", "setFanMode", [fanMode]);

    public static DeviceCommand WindDirection(string direction) =>
        new("fanOscillationMode", "setFanOscillationMode", [direction]);

    public static DeviceCommand OptionalMode(string mode) =>
        new("custom.airConditionerOptionalMode", "setAcOptionalMode", [mode]);
}
