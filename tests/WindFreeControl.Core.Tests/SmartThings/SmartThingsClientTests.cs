using System.Net;
using WindFreeControl.Core.SmartThings;
using WindFreeControl.Core.Tests.Fakes;

namespace WindFreeControl.Core.Tests.SmartThings;

public class SmartThingsClientTests
{
    [Fact(DisplayName = "An expired or invalid token maps to Unauthorized so the app returns to setup")]
    public async Task SendAsync_With401_ReturnsUnauthorized()
    {
        // arrange
        var handler = new FakeHttpHandler().Respond(HttpStatusCode.Unauthorized);
        var client = new SmartThingsClient(new HttpClient(handler), "test-token");

        // act
        var result = await client.SendAsync("device", [Commands.Power(true)], TestContext.Current.CancellationToken);

        // assert
        Assert.False(result.IsSuccess);
        Assert.Equal(SmartThingsErrorKind.Unauthorized, result.Error.Kind);
        Assert.Equal("Bearer test-token", handler.Requests[0].Authorization);
    }
}
