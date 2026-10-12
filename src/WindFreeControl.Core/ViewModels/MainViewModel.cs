using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindFreeControl.Core.Device;
using WindFreeControl.Core.Localization;
using WindFreeControl.Core.Setup;
using WindFreeControl.Core.SmartThings;

namespace WindFreeControl.Core.ViewModels;

public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan ConfirmDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan TargetDebounce = TimeSpan.FromMilliseconds(600);

    private readonly ISmartThingsClient _client;
    private readonly CancellationTokenSource _lifetime = new();
    private CancellationTokenSource? _targetDebounce;
    private string? _deviceId;
    private AcState? _state;
    private double? _pendingTarget;
    private int _commandsInFlight;
    private bool _applying;
    private DateTimeOffset _lastUpdate;

    public MainViewModel(ISmartThingsClient client)
    {
        _client = client;
        Mode = new OptionGroup(Strings.Control_Mode, "Mode_", v => Fire(SendAsync(Commands.Mode(v), s => s with { Mode = s.Mode with { Current = v } })));
        FanMode = new OptionGroup(Strings.Control_Fan, "Fan_", v => Fire(SendAsync(Commands.FanMode(v), s => s with { FanMode = s.FanMode with { Current = v } })));
        WindDirection = new OptionGroup(Strings.Control_Wind, "Wind_", v => Fire(SendAsync(Commands.WindDirection(v), s => s with { WindDirection = s.WindDirection with { Current = v } })));
        OptionalMode = new OptionGroup(Strings.Control_Special, "Optional_", v => Fire(SendAsync(Commands.OptionalMode(v), s => s with { OptionalMode = s.OptionalMode with { Current = v } })));
    }

    public event EventHandler<string>? Unauthorized;

    public OptionGroup Mode { get; }

    public OptionGroup FanMode { get; }

    public OptionGroup WindDirection { get; }

    public OptionGroup OptionalMode { get; }

    [ObservableProperty]
    public partial string DeviceName { get; set; } = "";

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial bool IsOn { get; set; }

    [ObservableProperty]
    public partial string PowerText { get; set; } = "";

    [ObservableProperty]
    public partial string AccentKey { get; set; } = "off";

    [ObservableProperty]
    public partial string TargetText { get; set; } = "--";

    [ObservableProperty]
    public partial bool CanIncreaseTarget { get; set; }

    [ObservableProperty]
    public partial bool CanDecreaseTarget { get; set; }

    [ObservableProperty]
    public partial string RoomText { get; set; } = "";

    [ObservableProperty]
    public partial string HumidityText { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? Error { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStale))]
    public partial string? StaleText { get; set; }

    [ObservableProperty]
    public partial string PowerNowText { get; set; } = "";

    [ObservableProperty]
    public partial string EnergyTotalText { get; set; } = "";

    [ObservableProperty]
    public partial bool HasEnergy { get; set; }

    [ObservableProperty]
    public partial double TropicalNightHours { get; set; }

    [ObservableProperty]
    public partial string TropicalNightText { get; set; } = "";

    [ObservableProperty]
    public partial bool HasTropicalNight { get; set; }

    [ObservableProperty]
    public partial bool SoundOn { get; set; }

    [ObservableProperty]
    public partial bool HasSound { get; set; }

    [ObservableProperty]
    public partial bool AutoCleanOn { get; set; }

    [ObservableProperty]
    public partial string AutoCleanText { get; set; } = "";

    [ObservableProperty]
    public partial bool HasAutoClean { get; set; }

    [ObservableProperty]
    public partial double FilterUsage { get; set; }

    [ObservableProperty]
    public partial string FilterText { get; set; } = "";

    [ObservableProperty]
    public partial string FilterStatusText { get; set; } = "";

    [ObservableProperty]
    public partial bool FilterNeedsAttention { get; set; }

    [ObservableProperty]
    public partial bool HasFilter { get; set; }

    [ObservableProperty]
    public partial string SelfCheckText { get; set; } = "";

    [ObservableProperty]
    public partial bool IsSelfChecking { get; set; }

    [ObservableProperty]
    public partial bool HasSelfCheck { get; set; }

    [ObservableProperty]
    public partial string FirmwareText { get; set; } = "";

    [ObservableProperty]
    public partial string ModuleText { get; set; } = "";

    [ObservableProperty]
    public partial string YearText { get; set; } = "";

    public bool HasError => Error is not null;

    public bool IsStale => StaleText is not null;

    public async Task StartAsync()
    {
        var devices = await _client.GetDevicesAsync(_lifetime.Token);
        if (!devices.IsSuccess)
        {
            HandleError(devices.Error);
            return;
        }

        var device = DeviceDiscovery.PickAirConditioner(devices.Value);
        if (device is null)
        {
            Error = Strings.Setup_ErrorNoDevice;
            return;
        }

        _deviceId = device.DeviceId;
        DeviceName = device.DisplayName;
        await RefreshAsync();
        Fire(RunRefreshLoopAsync());
    }

    public void Dispose()
    {
        _lifetime.Cancel();
    }

    [RelayCommand]
    private Task TogglePowerAsync()
    {
        var on = !IsOn;
        return SendAsync(Commands.Power(on), s => s with { IsOn = on });
    }

    [RelayCommand]
    private Task ResetFilterAsync() =>
        SendAsync(Commands.ResetFilter(), s => s with { FilterUsagePercent = 0, FilterStatus = "normal" });

    [RelayCommand]
    private Task StartSelfCheckAsync() =>
        SendAsync(Commands.SelfCheck(true), s => s with { SelfCheckStatus = "run", SelfCheckProgress = 0 });

    [RelayCommand]
    private Task CancelSelfCheckAsync() =>
        SendAsync(Commands.SelfCheck(false), s => s with { SelfCheckStatus = "ready" });

    partial void OnTropicalNightHoursChanged(double value)
    {
        if (_applying || double.IsNaN(value))
        {
            return;
        }

        var hours = (int)Math.Clamp(Math.Round(value), 0, 35);
        Fire(SendAsync(Commands.TropicalNight(hours), s => s with { TropicalNightHours = hours }));
    }

    partial void OnSoundOnChanged(bool value)
    {
        if (!_applying)
        {
            var volume = value ? 100 : 0;
            Fire(SendAsync(Commands.Volume(volume), s => s with { Volume = volume }));
        }
    }

    partial void OnAutoCleanOnChanged(bool value)
    {
        if (!_applying)
        {
            Fire(SendAsync(Commands.AutoClean(value), s => s with { AutoClean = value }));
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task IncreaseTargetAsync() => ChangeTargetAsync(+1);

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task DecreaseTargetAsync() => ChangeTargetAsync(-1);

    private async Task ChangeTargetAsync(int direction)
    {
        if (_state is null)
        {
            return;
        }

        var range = _state.TargetRange;
        var target = range.Clamp((_pendingTarget ?? _state.TargetTemperature ?? range.Minimum) + (direction * range.Step));
        if (target == (_pendingTarget ?? _state.TargetTemperature))
        {
            return;
        }

        _pendingTarget = target;
        Apply(_state with { TargetTemperature = target });

        if (_targetDebounce is not null)
        {
            await _targetDebounce.CancelAsync();
            _targetDebounce.Dispose();
        }

        _targetDebounce = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        try
        {
            await Task.Delay(TargetDebounce, _targetDebounce.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        _pendingTarget = null;
        await SendAsync(Commands.TargetTemperature(target), s => s with { TargetTemperature = target });
    }

    private async Task SendAsync(DeviceCommand command, Func<AcState, AcState> optimistic)
    {
        if (_state is null || _deviceId is null)
        {
            return;
        }

        _commandsInFlight++;
        try
        {
            Apply(optimistic(_state));
            var result = await _client.SendAsync(_deviceId, [command], _lifetime.Token);
            if (result.IsSuccess)
            {
                Error = null;
                await Task.Delay(ConfirmDelay, _lifetime.Token);
            }
            else
            {
                HandleError(result.Error);
            }
        }
        finally
        {
            _commandsInFlight--;
        }

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_deviceId is null)
        {
            return;
        }

        var result = await _client.GetStatusAsync(_deviceId, _lifetime.Token);
        if (!result.IsSuccess)
        {
            HandleError(result.Error);
            if (_lastUpdate != default)
            {
                StaleText = Labels.Format(Strings.Control_Stale, _lastUpdate.LocalDateTime);
            }

            return;
        }

        using var document = result.Value;
        var state = AcState.Parse(document);
        _lastUpdate = DateTimeOffset.Now;
        StaleText = null;
        IsLoading = false;
        if (_commandsInFlight == 0 && _pendingTarget is null)
        {
            Apply(state);
        }
    }

    private async Task RunRefreshLoopAsync()
    {
        using var timer = new PeriodicTimer(RefreshInterval);
        while (await timer.WaitForNextTickAsync(_lifetime.Token))
        {
            await RefreshAsync();
        }
    }

    private void Apply(AcState state)
    {
        _applying = true;
        try
        {
            ApplyControls(state);
            ApplyReadings(state);
        }
        finally
        {
            _applying = false;
        }
    }

    private void ApplyControls(AcState state)
    {
        _state = state;
        IsOn = state.IsOn;
        PowerText = state.IsOn && state.Mode.Current is { } mode
            ? $"{Strings.Control_On} · {Labels.ForValue("Mode_", mode)}"
            : state.IsOn ? Strings.Control_On : Strings.Control_Off;
        AccentKey = state.IsOn ? state.Mode.Current ?? "auto" : "off";
        TargetText = state.TargetTemperature is { } target ? Labels.Temperature(target) : "--";
        CanIncreaseTarget = state.IsOn && state.TargetTemperature < state.TargetRange.Maximum;
        CanDecreaseTarget = state.IsOn && state.TargetTemperature > state.TargetRange.Minimum;
        RoomText = state.RoomTemperature is { } room ? Labels.Format(Strings.Control_Room, Labels.Temperature(room)) : "";
        HumidityText = state.Humidity is { } humidity ? Labels.Format(Strings.Control_Humidity, humidity) : "";
        Mode.Apply(state.Mode, state.IsOn);
        FanMode.Apply(state.FanMode, state.IsOn);
        WindDirection.Apply(state.WindDirection, state.IsOn);
        OptionalMode.Apply(state.OptionalMode, state.IsOn);
    }

    private void ApplyReadings(AcState state)
    {
        HasEnergy = state.PowerWatts is not null || state.EnergyTotalWh is not null;
        PowerNowText = state.PowerWatts is { } watts ? Labels.Format("{0:0} W", watts) : "--";
        EnergyTotalText = state.EnergyTotalWh is { } wh ? Labels.Format("{0:#,0.#} kWh", wh / 1000) : "--";

        HasTropicalNight = state.TropicalNightHours is not null;
        TropicalNightHours = state.TropicalNightHours ?? 0;
        TropicalNightText = state.TropicalNightRemainingMinutes is > 0 and var minutes
            ? Labels.Format(Strings.TropicalNight_Remaining, minutes)
            : Strings.TropicalNight_Description;

        HasSound = state.Volume is not null;
        SoundOn = state.Volume > 0;

        HasAutoClean = state.AutoClean is not null;
        AutoCleanOn = state.AutoClean == true;
        AutoCleanText = state.IsAutoCleaning ? Strings.AutoClean_Running : Strings.AutoClean_Description;

        HasFilter = state.FilterUsagePercent is not null;
        FilterUsage = state.FilterUsagePercent ?? 0;
        FilterText = Labels.Format(Strings.Filter_Usage, $"{state.FilterUsagePercent:0}", state.FilterCapacityHours);
        FilterStatusText = state.FilterStatus is { } status ? Labels.ForValue("Filter_Status_", status) : "";
        FilterNeedsAttention = state.FilterStatus is "wash" or "replace";

        HasSelfCheck = state.SelfCheckStatus is not null;
        IsSelfChecking = state.SelfCheckStatus is not null and not "ready";
        SelfCheckText = IsSelfChecking
            ? Labels.Format(Strings.SelfCheck_Running, state.SelfCheckProgress ?? 0)
            : state.SelfCheckErrors.Count > 0
                ? Labels.Format(Strings.SelfCheck_Errors, string.Join(", ", state.SelfCheckErrors))
                : Strings.SelfCheck_NoErrors;

        FirmwareText = state.Firmware ?? "--";
        ModuleText = state.WifiModule ?? "--";
        YearText = state.ReleaseYear is { } year ? $"{(year < 100 ? 2000 + year : year):0}" : "--";
    }

    private void HandleError(SmartThingsError error)
    {
        if (error.Kind == SmartThingsErrorKind.Unauthorized)
        {
            Unauthorized?.Invoke(this, Labels.ForError(error));
            return;
        }

        Error = Labels.ForError(error);
        IsLoading = false;
    }

    private async void Fire(Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
        }
    }
}
