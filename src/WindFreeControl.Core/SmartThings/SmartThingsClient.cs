using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WindFreeControl.Core.SmartThings;

public sealed class SmartThingsClient(HttpClient http, string token) : ISmartThingsClient
{
    public static readonly Uri BaseAddress = new("https://api.smartthings.com/v1/");

    public async Task<SmartThingsResult<IReadOnlyList<DeviceDto>>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var result = await SendRequestAsync(HttpMethod.Get, "devices", null, cancellationToken);
        if (!result.IsSuccess)
        {
            return SmartThingsResult<IReadOnlyList<DeviceDto>>.Failure(result.Error);
        }

        using var response = result.Value;
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var list = await JsonSerializer.DeserializeAsync(stream, SmartThingsJsonContext.Default.DeviceListDto, cancellationToken);
        return SmartThingsResult<IReadOnlyList<DeviceDto>>.Success(list?.Items ?? []);
    }

    public async Task<SmartThingsResult<JsonDocument>> GetStatusAsync(string deviceId, CancellationToken cancellationToken)
    {
        var result = await SendRequestAsync(HttpMethod.Get, $"devices/{Uri.EscapeDataString(deviceId)}/status", null, cancellationToken);
        if (!result.IsSuccess)
        {
            return SmartThingsResult<JsonDocument>.Failure(result.Error);
        }

        using var response = result.Value;
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return SmartThingsResult<JsonDocument>.Success(document);
    }

    public async Task<SmartThingsResult<bool>> SendAsync(string deviceId, IReadOnlyList<DeviceCommand> commands, CancellationToken cancellationToken)
    {
        using var body = new ByteArrayContent(CommandBody.Write(commands));
        body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var result = await SendRequestAsync(HttpMethod.Post, $"devices/{Uri.EscapeDataString(deviceId)}/commands", body, cancellationToken);
        if (!result.IsSuccess)
        {
            return SmartThingsResult<bool>.Failure(result.Error);
        }

        result.Value.Dispose();
        return SmartThingsResult<bool>.Success(true);
    }

    private async Task<SmartThingsResult<HttpResponseMessage>> SendRequestAsync(
        HttpMethod method, string path, HttpContent? content, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, new Uri(BaseAddress, path)) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return SmartThingsResult<HttpResponseMessage>.Failure(new(SmartThingsErrorKind.Network, ex.Message));
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return SmartThingsResult<HttpResponseMessage>.Failure(new(SmartThingsErrorKind.Network, ex.Message));
        }

        if (response.IsSuccessStatusCode)
        {
            return SmartThingsResult<HttpResponseMessage>.Success(response);
        }

        using (response)
        {
            var message = await ReadErrorMessageAsync(response, cancellationToken);
            return SmartThingsResult<HttpResponseMessage>.Failure(new(KindOf(response.StatusCode), message));
        }
    }

    private static SmartThingsErrorKind KindOf(HttpStatusCode status) => status switch
    {
        HttpStatusCode.Unauthorized => SmartThingsErrorKind.Unauthorized,
        HttpStatusCode.Forbidden => SmartThingsErrorKind.Forbidden,
        HttpStatusCode.UnprocessableEntity or HttpStatusCode.BadRequest => SmartThingsErrorKind.Rejected,
        HttpStatusCode.TooManyRequests => SmartThingsErrorKind.RateLimited,
        _ => SmartThingsErrorKind.Unexpected,
    };

    private static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var text = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        try
        {
            var envelope = JsonSerializer.Deserialize(text, SmartThingsJsonContext.Default.ErrorEnvelopeDto);
            return Shorten(envelope?.Error?.Message ?? text);
        }
        catch (JsonException)
        {
            return Shorten(text);
        }
    }

    // A proxy or captive portal can answer with a whole HTML page; show only a short, clean prefix.
    private static string Shorten(string text)
    {
        const int MaxLength = 200;
        var clean = new string([.. text.Where(c => !char.IsControl(c))]).Trim();
        return clean.Length <= MaxLength ? clean : clean[..MaxLength] + "…";
    }
}
