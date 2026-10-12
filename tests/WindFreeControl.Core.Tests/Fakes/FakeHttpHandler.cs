using System.Net;
using System.Text;

namespace WindFreeControl.Core.Tests.Fakes;

internal sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpResponseMessage>> _responses = new();

    public List<RecordedRequest> Requests { get; } = [];

    public FakeHttpHandler Respond(HttpStatusCode status, string body = "")
    {
        _responses.Enqueue(() => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        });
        return this;
    }

    public FakeHttpHandler Throw(Exception exception)
    {
        _responses.Enqueue(() => throw exception);
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add(new RecordedRequest(request.Method, request.RequestUri!, request.Headers.Authorization?.ToString(), body));
        return _responses.Dequeue()();
    }
}

internal sealed record RecordedRequest(HttpMethod Method, Uri Uri, string? Authorization, string? Body);
