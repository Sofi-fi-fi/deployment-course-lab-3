using mywebapp.Models;

namespace mywebapp.Endpoints;

public static class HealthEndpoints
{
public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
	{
		app.MapGet("/health/alive", () => Results.Text("OK", "text/plain"))
			.WithName("HealthAlive");

		app.MapGet("/health/ready", async (AppDbContext db) =>
        {
            try
            {
                await db.Database.CanConnectAsync();
                return Results.Content("OK", "text/plain");
            }
            catch (Exception ex)
            {
                return Results.Content(
                    $"Service unavailable: cannot connect to database. {ex.Message}",
                    "text/plain",
                    statusCode: 500);
            }
        }).WithName("HealthReady");
		
		return app;
	}
}
