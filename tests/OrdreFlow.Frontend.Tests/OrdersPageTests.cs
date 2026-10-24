using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Pages;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

public sealed class OrdersPageTests : PageTestContext
{
    [Fact]
    public void ManagerCanCreateOrderWithExpectedPayload()
    {
        Api.Json("api/orders", new ViewOrdersResponse([]));
        Api.Accept(HttpMethod.Post, "api/orders");
        var page = RenderComponent<Orders>();

        page.WaitForAssertion(() => Assert.False(page.Find(".orders-header button").HasAttribute("disabled")));
        page.Find(".orders-header button").Click();
        page.Find("#order-name").Change("New order");
        page.Find("form").Submit();

        page.WaitForAssertion(() =>
        {
            Assert.Empty(page.FindAll("dialog"));
            var request = Assert.Single(Api.Requests.Where(request => request.Method == HttpMethod.Post));
            var payload = JsonSerializer.Deserialize<CreateOrderRequest>(request.Body!, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.Equal(new CreateOrderRequest("New order"), payload);
        });
    }

    [Fact]
    public void TaskButtonNavigatesWithSelectedOrderId()
    {
        var order = new OrderResponse(Guid.NewGuid(), "Order", "Open", DateTime.UtcNow, null);
        Api.Json("api/orders", new ViewOrdersResponse([order]));
        Api.Json($"api/orders/{order.Id}", order);
        var page = RenderComponent<Orders>();

        page.WaitForElement(".order-row").Click();
        page.WaitForAssertion(() => Assert.NotEmpty(page.FindAll(".order-metadata")));
        page.FindAll("dialog button").Single(button => button.TextContent.Trim() == "Gå til tasks").Click();

        Assert.EndsWith($"/tasks?orderId={order.Id}", Services.GetRequiredService<NavigationManager>().Uri);
    }

    [Fact]
    public void EmployeeCannotLoadOrdersOrCreateThem()
    {
        Api.Json("api/users/me", Manager with { Role = "Employee" });

        var page = RenderComponent<Orders>();

        page.WaitForAssertion(() =>
        {
            Assert.Contains("kun tilgængelig", page.Find(".alert-info").TextContent);
            Assert.Empty(page.FindAll(".orders-header button"));
        });
        Assert.DoesNotContain(Api.Requests, request => request.Path == "api/orders");
    }
}
