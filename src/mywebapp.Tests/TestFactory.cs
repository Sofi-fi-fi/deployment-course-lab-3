using Microsoft.AspNetCore.Mvc.Testing;

namespace mywebapp.Tests;

public class TestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(
            "APP_CONFIG_PATH",
            Path.Combine(AppContext.BaseDirectory, "test-config.json"));

        Environment.SetEnvironmentVariable("USE_INMEMORY_DB", "true");
    }
}