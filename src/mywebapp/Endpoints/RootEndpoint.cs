using System;

namespace mywebapp.Endpoints;

public static class RootEndpoints
{
	public static IEndpointRouteBuilder MapRootEndpoints(this IEndpointRouteBuilder app) 
	{
		app.MapGet("/", (HttpRequest request) => 
		{
			var accepts  = request.GetTypedHeaders().Accept;
			var acceptsHtml = accepts.Any(h => h.MediaType.Value == "text/html");

			if (!acceptsHtml)
			{
				return Results.StatusCode(StatusCodes.Status406NotAcceptable);
			}

			var html = """
				<html>
				<body>
					<h1>API Endpoints</h1>
					<table border="1" cellpadding="5">
						<thead>
							<tr>
								<th>Method</th>
								<th>Path</th>
								<th>Description</th>
							</tr>
						</thead>
						<tbody>
							<tr>
								<td>GET</td>
								<td>/tasks</td>
								<td>Вивести усі задачі (id, title, status, created_at)</td>
							</tr>
							<tr>
								<td>POST</td>
								<td>/tasks</td>
								<td>Створити нову задачу (body: { "title": "..." })</td>
							</tr>
							<tr>
								<td>POST</td>
								<td>/tasks/{id}/done</td>
								<td>Змінити статус задачі на виконано</td>
							</tr>
						</tbody>
					</table>
				</body>
				</html>
				""";
			return Results.Content(html, "text/html; charset=utf-8");
		});
		return app;
	}
}
