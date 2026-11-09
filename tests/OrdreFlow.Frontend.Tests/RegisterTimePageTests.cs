using System.Text.Json;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Pages;
using OrdreFlow.Frontend.Services;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

public sealed class RegisterTimePageTests : PageTestContext
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    private readonly OrderResponse order = new(Guid.NewGuid(), "Netto Vestergade", "Open", DateTime.UtcNow, null);
    private readonly OrderResponse closedOrder = new(Guid.NewGuid(), "Afsluttet sag", "Closed", DateTime.UtcNow, DateTime.UtcNow);
    private readonly TaskResponse shelves;
    private readonly TaskResponse signs;
    private readonly TaskResponse closedTask;
    private readonly TaskResponse oldOrderTask;

    public RegisterTimePageTests()
    {
        Services.AddScoped<TasksApiClient>();
        Services.AddScoped<ITimeEntriesApiClient, TimeEntriesApiClient>();

        shelves = new TaskResponse(Guid.NewGuid(), order.Id, "Opsæt reoler", "");
        signs = new TaskResponse(Guid.NewGuid(), order.Id, "Montér skilte", "");
        closedTask = new TaskResponse(Guid.NewGuid(), order.Id, "Slutrengøring", "", "Closed", DateTime.UtcNow);
        oldOrderTask = new TaskResponse(Guid.NewGuid(), closedOrder.Id, "Gammel opgave", "");

        Api.Json("api/orders", new ViewOrdersResponse([order, closedOrder]));
        Api.Json("api/tasks", new ViewTasksResponse([shelves, signs, closedTask, oldOrderTask]));
        Api.Json("api/time_entries", new ViewTimeEntriesResponse(
        [
            Entry(signs, daysAgo: 1),
            Entry(shelves, daysAgo: 3),
            Entry(signs, daysAgo: 4),
            Entry(closedTask, daysAgo: 2),
            Entry(oldOrderTask, daysAgo: 2)
        ]));
        Api.Accept(HttpMethod.Post, "api/time_entries");
    }

    [Fact]
    public void RecentTasksShowOpenTasksNewestFirstWithoutDuplicates()
    {
        var page = RenderComponent<RegisterTime>();

        page.WaitForAssertion(() =>
            Assert.Equal(["Montér skilte", "Opsæt reoler"],
                page.FindAll(".recent-task").Select(task => task.TextContent)));
    }

    [Fact]
    public void QuickRegistrationWithRecentTaskAndShortcutsSendsExpectedEntry()
    {
        var page = RenderComponent<RegisterTime>();
        page.WaitForElement(".recent-btn");

        page.FindAll(".recent-btn").Single(button => button.TextContent.Contains("Opsæt reoler")).Click();
        page.FindAll(".chip").Single(chip => chip.TextContent.Trim() == "I går").Click();
        page.FindAll(".chip").Single(chip => chip.TextContent.Trim() == "2").Click();

        Assert.Equal("Registrer 2 timer", page.Find(".submit-btn").TextContent.Trim());
        Assert.Equal("true", page.FindAll(".recent-btn").Single(button => button.TextContent.Contains("Opsæt reoler"))
            .GetAttribute("aria-pressed"));

        page.Find("form").Submit();

        page.WaitForAssertion(() =>
        {
            var request = Assert.Single(Api.Requests, request => request.Method == HttpMethod.Post);
            var payload = JsonSerializer.Deserialize<CreateTimeEntryRequest>(request.Body!, WebJson);
            Assert.Equal(shelves.Id, payload!.TaskId);
            Assert.Equal(DateTime.Today.AddDays(-1), payload.Date.Date);
            Assert.Equal(2m, payload.Hours);
            // A plain calendar date without offset, so a UTC backend doesn't store it as the previous day.
            Assert.Contains($"\"date\":\"{DateTime.Today.AddDays(-1):yyyy-MM-dd}T00:00:00\"", request.Body);
        });
    }

    [Fact]
    public void ClosedTasksAndOrdersAreNotOffered()
    {
        var page = RenderComponent<RegisterTime>();
        page.WaitForElement(".recent-btn");

        // With recent tasks, the order/task dropdowns sit behind a toggle to keep the form short.
        Assert.Empty(page.FindAll("#order"));
        page.Find(".manual-toggle").Click();

        Assert.DoesNotContain(page.FindAll("#order option"), option => option.TextContent == "Afsluttet sag");
        page.FindAll(".recent-btn")[0].Click();
        page.WaitForAssertion(() => Assert.DoesNotContain(page.FindAll("#task option"),
            option => option.TextContent == "Slutrengøring"));
    }

    private static TimeEntryResponse Entry(TaskResponse task, int daysAgo) =>
        new(Guid.NewGuid(), task.Id, DateTime.Today.AddDays(-daysAgo), 1m, null, Guid.NewGuid(), "Draft");
}
