using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mywebapp.Models;
using TaskEntity = mywebapp.Models.Entities.Task;

namespace mywebapp.Endpoints;

public static class TaskEndpoints
{
	private record CreateTaskRequest(string Title);
	
	public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app) 
	{
		app.MapGet("/tasks", async (HttpRequest request, AppDbContext db) => 
		{
			var tasks = await db.Tasks
				.Select(t => new 
				{
					t.Id,
					t.Title,
					t.Status,
					t.Created_At
				})
				.ToListAsync();

			var accepts = request.GetTypedHeaders().Accept
				.Any(h => h.MediaType.Value == "text/html");

			if (accepts)
			{
				var rows = string.Join("", tasks.Select(t => $"""
					<tr>
						<td>{t.Id}</td>
						<td>{t.Title}</td>
						<td>{t.Status}</td>
						<td>{t.Created_At:yyyy-MM-dd HH:mm:ss}</td>
					</tr>
				"""));

				var html = $"""
					<html>
					<body>
						<h1>Tasks</h1>
						<table border="1" cellpadding="5">
							<thead>
								<tr>
									<th>ID</th>
									<th>Title</th>
									<th>Status</th>
									<th>Created At</th>
								</tr>
							</thead>
							<tbody>
								{rows}
							</tbody>
						</table>
					</body>
					</html>
					""";
				return Results.Content(html, "text/html; charset=utf-8");
			}

			return Results.Ok(tasks);
		});
		
		app.MapPost("/tasks", async (HttpRequest request, [FromBody] CreateTaskRequest body, AppDbContext db) => 
		{
			if (string.IsNullOrWhiteSpace(body.Title))
			{
				var accept = request.GetTypedHeaders().Accept
					.Any(h => h.MediaType.Value == "text/html");

				if (accept)
					return Results.Content("<html><body><p>Error: Title is required</p></body></html>", "text/html; charset=utf-8", statusCode: StatusCodes.Status400BadRequest);

				return Results.BadRequest(new { error = "Title is required" });
			}

			var task = new TaskEntity
			{
				Title = body.Title,
				Status = "pending",
				Created_At = DateTime.UtcNow
			};

			db.Tasks.Add(task);
			await db.SaveChangesAsync();

			var accepts = request.GetTypedHeaders().Accept
				.Any(h => h.MediaType.Value == "text/html");

			if (accepts)
			{
				var html = $"""
					<html>
					<body>
						<h1>Task Created</h1>
						<table border="1" cellpadding="5">
							<tr><th>ID</th><td>{task.Id}</td></tr>
							<tr><th>Title</th><td>{task.Title}</td></tr>
							<tr><th>Status</th><td>{task.Status}</td></tr>
							<tr><th>Created At</th><td>{task.Created_At:yyyy-MM-dd HH:mm:ss}</td></tr>
						</table>
					</body>
					</html>
					""";
				return Results.Content(html, "text/html", statusCode: StatusCodes.Status201Created);
			}

			return Results.Created($"/tasks/{task.Id}", new 
			{
				id = task.Id,
				title = task.Title,
				status  = task.Status,
				created_at = task.Created_At
			}); 
		});

		app.MapPost("/tasks/{id}/done", async (HttpRequest request, int id, AppDbContext db) => 
		{
			var task = await db.Tasks.FindAsync(id);
			if (task is null)
			{
				var accept = request.GetTypedHeaders().Accept
					.Any(h => h.MediaType.Value == "text/html");

				if (accept)
					return Results.Content("<html><body><p>Error: Task not found</p></body></html>", "text/html", statusCode: StatusCodes.Status404NotFound);

				return Results.NotFound(new { error = "Task not found" });
			}

			task.Status = "done";
			await db.SaveChangesAsync();

			var accepts = request.GetTypedHeaders().Accept
				.Any(h => h.MediaType.Value == "text/html");

			if (accepts)
			{
				var html = $"""
					<html>
					<body>
						<h1>Task Updated</h1>
						<table border="1" cellpadding="5">
							<tr><th>ID</th><td>{task.Id}</td></tr>
							<tr><th>Title</th><td>{task.Title}</td></tr>
							<tr><th>Status</th><td>{task.Status}</td></tr>
							<tr><th>Created At</th><td>{task.Created_At:yyyy-MM-dd HH:mm:ss}</td></tr>
						</table>
					</body>
					</html>
					""";
				return Results.Content(html, "text/html");
			}

			return Results.Ok(new 
			{
				id = task.Id,
				title = task.Title,
				status  = task.Status,
				created_at = task.Created_At
			});
		});
		return app;
	}
}
