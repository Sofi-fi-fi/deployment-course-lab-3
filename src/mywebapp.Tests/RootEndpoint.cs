using System.Net;
using Xunit;

namespace mywebapp.Tests;

public class RootEndpoint(TestFactory factory) : IClassFixture<TestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Root_WithHtmlAccept_ReturnsHtmlPage()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("Accept", "text/html");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("<table", body);
    }

    [Fact]
    public async Task Root_WithoutHtmlAccept_Returns406()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
    }
}