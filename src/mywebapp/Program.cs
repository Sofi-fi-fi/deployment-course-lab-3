using mywebapp.Models;
using mywebapp.Endpoints;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
var configPath = Environment.GetEnvironmentVariable("APP_CONFIG_PATH") ?? "/etc/mywebapp/config.json";
builder.Configuration
    .AddJsonFile(configPath, optional: false, reloadOnChange: false);

var iface = builder.Configuration["App:Interface"]
    ?? throw new InvalidOperationException(
        "Config value 'App:Interface' is missing in /etc/mywebapp/config.json");

var port = builder.Configuration["App:Port"]
    ?? throw new InvalidOperationException(
        "Config value 'App:Port' is missing in /etc/mywebapp/config.json");

builder.WebHost.UseUrls($"http://{iface}:{port}");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing in /etc/mywebapp/config.json");

var useInMemory = Environment.GetEnvironmentVariable("USE_INMEMORY_DB") == "true";
if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestDatabase"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

var app = builder.Build();

app.MapTaskEndpoints();
app.MapRootEndpoints();
app.MapHealthEndpoints();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred creating the DB: {ex.Message}");
    }
}

app.Run();
public partial class Program { }
