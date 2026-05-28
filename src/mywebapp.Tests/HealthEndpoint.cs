using System.Net;
using Xunit;

namespace mywebapp.Tests;

public class HealthEndpoint(TestFactory factory) : IClassFixture<TestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task HealthAlive_ReturnsOk()
    {
        var response = await _client.GetAsync("/health/alive");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("OK", body);
    }

    [Fact]
    public async Task HealthReady_ReturnsOk_WhenDbConnected()
    {
        var response = await _client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}