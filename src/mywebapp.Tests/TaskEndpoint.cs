using System.Net;
using mywebapp.Models;
using Xunit;

namespace mywebapp.Tests;

public class TaskEndpoint(TestFactory factory) : IClassFixture<TestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly TestFactory _factory = factory;

    private async Task ClearDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Tasks.RemoveRange(db.Tasks);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetTasks_ReturnsOkWithEmptyList()
    {
        await ClearDatabaseAsync();

        var response = await _client.GetAsync("/tasks");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task PostTasks_ValidTitle_ReturnsCreatedTask()
    {
        await ClearDatabaseAsync();

        var body = new { title = "Тестова задача" };
        var response = await _client.PostAsJsonAsync("/tasks", body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(result);
        Assert.Equal("Тестова задача", result.Title);
        Assert.Equal("pending", result.Status);
    }

    [Fact]
    public async Task PostTasks_EmptyTitle_ReturnsBadRequest()
    {
        var body = new { title = "" };
        var response = await _client.PostAsJsonAsync("/tasks", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTasks_WhitespaceTitle_ReturnsBadRequest()
    {
        var body = new { title = "   " };
        var response = await _client.PostAsJsonAsync("/tasks", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTasks_Done_ExistingTask_ReturnsOkWithStatusDone()
    {
        await ClearDatabaseAsync();

        var createBody = new { title = "Задача для завершення" };
        var createResponse = await _client.PostAsJsonAsync("/tasks", createBody);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);

        var doneResponse = await _client.PostAsync($"/tasks/{created.Id}/done", null);

        Assert.Equal(HttpStatusCode.OK, doneResponse.StatusCode);

        var result = await doneResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(result);
        Assert.Equal("done", result.Status);
    }

    [Fact]
    public async Task PostTasks_Done_NonExistentTask_ReturnsNotFound()
    {
        var response = await _client.PostAsync("/tasks/99999/done", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_AfterCreating_ReturnsTasks()
    {
        await ClearDatabaseAsync();

        await _client.PostAsJsonAsync("/tasks", new { title = "Тестова задача 1" });
        await _client.PostAsJsonAsync("/tasks", new { title = "Тестова задача 2" });

        var response = await _client.GetAsync("/tasks");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponse>>();
        Assert.NotNull(tasks);
        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async Task PostTasks_NewTask_HasPendingStatus()
    {
        var response = await _client.PostAsJsonAsync("/tasks", new { title = "Нова задача" });
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();

        Assert.NotNull(task);
        Assert.Equal("pending", task.Status);
    }

    [Fact]
    public async Task GetTasks_WithHtmlAccept_ReturnsHtmlTable()
    {
        await ClearDatabaseAsync();
        await _client.PostAsJsonAsync("/tasks", new { title = "Тест HTML таблиця" });

        var request = new HttpRequestMessage(HttpMethod.Get, "/tasks");
        request.Headers.Add("Accept", "text/html");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("<table", body);
        Assert.Contains("Тест HTML таблиця", body);
    }

    [Fact]
    public async Task PostTasks_WithHtmlAccept_ReturnsHtmlCreated()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/tasks");
        request.Headers.Add("Accept", "text/html");
        request.Content = JsonContent.Create(new { title = "Тест створення HTML" });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Тест створення HTML", body);
    }

    [Fact]
    public async Task PostTasks_Done_WithHtmlAccept_ReturnsHtmlUpdated()
    {
        await ClearDatabaseAsync();

        var created = await (await _client.PostAsJsonAsync("/tasks", new { title = "Тест HTML done" }))
            .Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/tasks/{created.Id}/done");
        request.Headers.Add("Accept", "text/html");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("done", body);
    }

    [Fact]
    public async Task PostTasks_Done_WithHtmlAccept_NotFound_ReturnsHtml()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/tasks/99999/done");
        request.Headers.Add("Accept", "text/html");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Error", body);
    }

    [Fact]
    public async Task GetTasks_EmptyList_WithHtmlAccept_ReturnsEmptyTable()
    {
        await ClearDatabaseAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/tasks");
        request.Headers.Add("Accept", "text/html");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("<table", body);
    }

    private record TaskResponse(int Id, string Title, string Status, DateTime Created_at);
}