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
