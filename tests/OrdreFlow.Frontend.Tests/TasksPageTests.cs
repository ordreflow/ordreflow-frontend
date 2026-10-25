using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Pages;
using OrdreFlow.Frontend.Services;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

public sealed class TasksPageTests : PageTestContext
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    private readonly OrderResponse order = new(Guid.NewGuid(), "Order A", "Open", DateTime.UtcNow, null);
    private readonly OrderResponse otherOrder = new(Guid.NewGuid(), "Order B", "Open", DateTime.UtcNow, null);

    public TasksPageTests()
    {
        Services.AddScoped<TasksApiClient>();
        Api.Json("api/orders", new ViewOrdersResponse([order, otherOrder]));
    }

    [Fact]
    public async Task OrderIdFromOrdersPageShowsOnlyThatOrdersTasks()
    {
        await SignInAsync(Manager);
        Api.Json("api/tasks", new ViewTasksResponse(
        [
            new TaskResponse(Guid.NewGuid(), order.Id, "Task on A", ""),
            new TaskResponse(Guid.NewGuid(), otherOrder.Id, "Task on B", "")
        ]));
        NavigateTo($"tasks?orderId={order.Id}");

        var page = RenderComponent<Tasks>();

        page.WaitForAssertion(() =>
        {
            var title = Assert.Single(page.FindAll(".task-title"));
            Assert.Equal("Task on A", title.TextContent);
            Assert.Equal("Order A", page.Find(".tasks-subtitle").TextContent);
            Assert.Equal($"tasks/new?orderId={order.Id}", page.Find(".new-task-btn").GetAttribute("href"));
        });
    }

    [Fact]
    public async Task ManagerCreatesTaskOnPresetOrderWithExpectedPayload()
    {
        await SignInAsync(Manager);
        Api.Accept(HttpMethod.Post, "api/tasks");
        NavigateTo($"tasks/new?orderId={order.Id}");

        var page = RenderComponent<TaskForm>();

        page.WaitForElement("#title").Change("New task");
        page.Find("#description").Change("Some details");
        page.Find("form").Submit();

        page.WaitForAssertion(() =>
        {
            var request = Assert.Single(Api.Requests, request => request.Method == HttpMethod.Post);
            var payload = JsonSerializer.Deserialize<CreateTaskRequest>(request.Body!, WebJson);
            Assert.Equal(new CreateTaskRequest(order.Id, "New task", "Some details"), payload);
            Assert.EndsWith($"/tasks?orderId={order.Id:D}", Services.GetRequiredService<NavigationManager>().Uri);
        });
    }

    [Fact]
    public async Task ManagerEditsTaskWithExpectedPayload()
    {
        await SignInAsync(Manager);
        var task = new TaskResponse(Guid.NewGuid(), order.Id, "Old title", "Old description");
        Api.Json($"api/tasks/{task.Id}", task);
        Api.Accept(HttpMethod.Put, $"api/tasks/{task.Id}");

        var page = RenderComponent<TaskForm>(parameters => parameters.Add(p => p.Id, task.Id));

        page.WaitForAssertion(() => Assert.Equal("Old title", page.Find("#title").GetAttribute("value")));
        page.Find("#title").Change("New title");
        page.Find("form").Submit();

        page.WaitForAssertion(() =>
        {
            var request = Assert.Single(Api.Requests, request => request.Method == HttpMethod.Put);
            var payload = JsonSerializer.Deserialize<UpdateTaskRequest>(request.Body!, WebJson);
            Assert.Equal(new UpdateTaskRequest("New title", "Old description"), payload);
        });
    }

    [Fact]
    public async Task DetailsShowTaskWithEditAndDisabledComplete()
    {
        await SignInAsync(Manager);
        var task = new TaskResponse(Guid.NewGuid(), order.Id, "Task on A", "Details");
        Api.Json($"api/tasks/{task.Id}", task);

        var page = RenderComponent<TaskDetails>(parameters => parameters.Add(p => p.Id, task.Id));

        page.WaitForAssertion(() =>
        {
            Assert.Equal("Task on A", page.Find(".task-details-title").TextContent);
            Assert.Equal($"tasks/{task.Id:D}/edit", page.Find("a.btn-outline-primary").GetAttribute("href"));
            Assert.True(page.Find("button.btn-success").HasAttribute("disabled"));
        });
    }

    [Fact]
    public async Task EmployeeCannotCreateOrEditTasks()
    {
        await SignInAsync(Manager with { Role = "Employee" });
        Api.Json("api/tasks", new ViewTasksResponse([new TaskResponse(Guid.NewGuid(), order.Id, "Task on A", "")]));

        var list = RenderComponent<Tasks>();
        list.WaitForAssertion(() => Assert.Single(list.FindAll(".task-title")));
        Assert.Empty(list.FindAll(".new-task-btn"));

        var form = RenderComponent<TaskForm>();
        Assert.Contains("Kun ledere og administratorer", form.Markup);
        Assert.Empty(form.FindAll("form"));
        Assert.DoesNotContain(Api.Requests, request => request.Method != HttpMethod.Get);
    }

    private Task SignInAsync(UserResponse user) =>
        Services.GetRequiredService<AuthState>().SetUserAsync(user);

    private void NavigateTo(string relativeUri) =>
        Services.GetRequiredService<NavigationManager>().NavigateTo(relativeUri);
}
