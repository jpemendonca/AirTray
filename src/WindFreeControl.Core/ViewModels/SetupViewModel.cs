using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindFreeControl.Core.Abstractions;
using WindFreeControl.Core.Localization;
using WindFreeControl.Core.Setup;
using WindFreeControl.Core.SmartThings;

namespace WindFreeControl.Core.ViewModels;

public sealed partial class SetupViewModel(ITokenStore tokens, Func<string, ISmartThingsClient> clientFactory) : ObservableObject
{
    public static readonly Uri TokenPage = new("https://account.smartthings.com/tokens");

    public event EventHandler? Connected;

    [ObservableProperty]
    public partial string Token { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? Error { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public bool HasError => Error is not null;

    [RelayCommand]
    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        var problems = TokenValidation.Validate(Token);
        if (problems.Count > 0)
        {
            Error = string.Join(" ", problems.Select(p => p switch
            {
                TokenProblem.Empty => Strings.Setup_ErrorEmpty,
                _ => Strings.Setup_ErrorSpaces,
            }));
            return;
        }

        var token = TokenValidation.Normalize(Token);
        Error = null;
        IsBusy = true;
        try
        {
            var devices = await clientFactory(token).GetDevicesAsync(cancellationToken);
            if (!devices.IsSuccess)
            {
                Error = Labels.ForError(devices.Error);
                return;
            }

            if (DeviceDiscovery.PickAirConditioner(devices.Value) is null)
            {
                Error = Strings.Setup_ErrorNoDevice;
                return;
            }

            tokens.Save(token);
            Connected?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
