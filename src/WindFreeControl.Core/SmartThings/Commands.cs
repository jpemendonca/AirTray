using System.Text.Json.Nodes;

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

    public static DeviceCommand TropicalNight(int hours) =>
        new("custom.airConditionerTropicalNightMode", "setAcTropicalNightModeLevel", [hours]);

    public static DeviceCommand AutoClean(bool on) =>
        new("custom.autoCleaningMode", "setAutoCleaningMode", [on ? "on" : "off"]);

    public static DeviceCommand Volume(int percent) =>
        new("audioVolume", "setVolume", [percent]);

    public static DeviceCommand ResetFilter() =>
        new("custom.dustFilter", "resetDustFilter", []);

    public static DeviceCommand Lighting(bool on) =>
        new("samsungce.airConditionerLighting", on ? "on" : "off", []);

    // OCF fallback for units that ignore the lighting capability. Reported on Samsung AC forums;
    // a few models invert the meaning of Light_On / Light_Off.
    public static DeviceCommand LightingViaExecute(bool on) =>
        new("execute", "execute", ["mode/vs/0", CommandArgument.FromJson(new JsonObject
        {
            ["x.com.samsung.da.options"] = new JsonArray(on ? "Light_On" : "Light_Off"),
        })]);

    public static DeviceCommand SelfCheck(bool start) =>
        new("samsungce.selfCheck", start ? "startSelfCheck" : "cancelSelfCheck", []);
}
