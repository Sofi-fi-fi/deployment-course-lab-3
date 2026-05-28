using mywebapp.Models;
using mywebapp.Endpoints;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("/etc/mywebapp/config.json", optional: false, reloadOnChange: false);

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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.MapTaskEndpoints();
app.MapRootEndpoints();
app.MapHealthEndpoints();

app.Run();
