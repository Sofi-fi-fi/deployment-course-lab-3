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

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

    private record TaskResponse(int Id, string Title, string Status, DateTime Created_at);
}