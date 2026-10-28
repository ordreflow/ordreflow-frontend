using System.Text.Json;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Pages;
using OrdreFlow.Frontend.Services;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

public sealed class ReviewTimeEntriesPageTests : PageTestContext
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    private readonly UserResponse anna;
    private readonly UserResponse bo;
    private readonly TimeEntryResponse annaDraft;
    private readonly TimeEntryResponse boDraft;
    private readonly TimeEntryResponse annaAccepted;

    public ReviewTimeEntriesPageTests()
    {
        Services.AddScoped<TasksApiClient>();
        Services.AddScoped<ITimeEntriesApiClient, TimeEntriesApiClient>();

        anna = new UserResponse(Guid.NewGuid(), "Anna", "anna@example.com", "Employee", "Active", DateTime.UtcNow, Manager.Id);
        bo = new UserResponse(Guid.NewGuid(), "Bo", "bo@example.com", "Employee", "Active", DateTime.UtcNow, Manager.Id);

        var order = new OrderResponse(Guid.NewGuid(), "Order A", "Open", DateTime.UtcNow, null);
        var task = new TaskResponse(Guid.NewGuid(), order.Id, "Task A", "");
        annaDraft = new TimeEntryResponse(Guid.NewGuid(), task.Id, DateTime.Today, 2m, "Draft comment", anna.Id, "Draft");
        boDraft = new TimeEntryResponse(Guid.NewGuid(), task.Id, DateTime.Today, 3m, null, bo.Id, "Draft");
        annaAccepted = new TimeEntryResponse(Guid.NewGuid(), task.Id, DateTime.Today, 4m, null, anna.Id, "Accepted");

        Api.Json("api/users", new ViewUsersResponse([Manager, anna, bo]));
        Api.Json("api/orders", new ViewOrdersResponse([order]));
        Api.Json("api/tasks", new ViewTasksResponse([task]));
        Api.Json("api/time_entries/manager", new ViewTimeEntriesResponse([annaDraft, boDraft, annaAccepted]));
    }

    [Fact]
    public async Task ShowsOnlyDraftsWithEmployeeNamesByDefault()
    {
        var page = await RenderAsync(Manager);

        page.WaitForAssertion(() =>
        {
            var names = page.FindAll(".review-employee").Select(element => element.TextContent).ToArray();
            Assert.Equal(["Anna", "Bo"], names);
            Assert.All(page.FindAll(".status-badge"), badge => Assert.Equal("Kladde", badge.TextContent));
        });
    }

    [Fact]
    public async Task EmployeeFilterShowsOneEmployeesEntriesInAllStatuses()
    {
        var page = await RenderAsync(Manager);
        page.WaitForElement(".review-entry");

        page.Find("#employee-filter").Change(anna.Id.ToString());
        page.Find("#status-filter").Change("");

        page.WaitForAssertion(() =>
        {
            Assert.All(page.FindAll(".review-employee"), name => Assert.Equal("Anna", name.TextContent));
            Assert.Equal(["Godkendt", "Kladde"], page.FindAll(".status-badge").Select(badge => badge.TextContent).Order());
        });
    }

    [Fact]
    public async Task AcceptSendsAcceptDecision()
    {
        Api.Accept(HttpMethod.Post, $"api/time_entries/{annaDraft.Id:D}/review");
        var page = await RenderAsync(Manager);

        page.WaitForElement(".review-entry .btn-success").Click();

        page.WaitForAssertion(() =>
        {
            var request = Assert.Single(Api.Requests, request => request.Method == HttpMethod.Post);
            var payload = JsonSerializer.Deserialize<ReviewTimeEntryRequest>(request.Body!, WebJson);
            Assert.Equal(new ReviewTimeEntryRequest("accept", null), payload);
            Assert.Contains("Anna er godkendt", page.Find(".alert-success").TextContent);
        });
    }

    [Fact]
    public async Task ReturnRequiresReasonAndSendsIt()
    {
        Api.Accept(HttpMethod.Post, $"api/time_entries/{annaDraft.Id:D}/review");
        var page = await RenderAsync(Manager);

        page.WaitForElement(".review-entry .btn-outline-danger").Click();
        Assert.True(page.Find(".return-form .btn-danger").HasAttribute("disabled"));

        page.Find(".return-form textarea").Input("Forkert opgave");
        page.Find(".return-form .btn-danger").Click();

        page.WaitForAssertion(() =>
        {
            var request = Assert.Single(Api.Requests, request => request.Method == HttpMethod.Post);
            var payload = JsonSerializer.Deserialize<ReviewTimeEntryRequest>(request.Body!, WebJson);
            Assert.Equal(new ReviewTimeEntryRequest("return", "Forkert opgave"), payload);
        });
    }

    [Fact]
    public async Task EmployeeCannotReviewOrLoadManagerEntries()
    {
        var page = await RenderAsync(Manager with { Role = "Employee" });

        Assert.Contains("kun tilgængelig", page.Find(".alert-info").TextContent);
        Assert.DoesNotContain(Api.Requests, request => request.Path == "api/time_entries/manager");
    }

    private async Task<IRenderedComponent<ReviewTimeEntries>> RenderAsync(UserResponse user)
    {
        await Services.GetRequiredService<AuthState>().SetUserAsync(user);
        return RenderComponent<ReviewTimeEntries>();
    }
}
