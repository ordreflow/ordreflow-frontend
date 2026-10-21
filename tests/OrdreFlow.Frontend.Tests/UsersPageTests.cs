using System.Text.Json;
using Bunit;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Pages;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

public sealed class UsersPageTests : PageTestContext
{
    [Fact]
    public void SearchFiltersUsersByEmail()
    {
        var employee = Manager with { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@example.com", Role = "Employee" };
        Api.Json("api/users", new ViewUsersResponse([Manager, employee]));

        var page = RenderComponent<Users>();
        page.WaitForElement("#user-search").Input("EMPLOYEE@");

        Assert.Contains("employee@example.com", Assert.Single(page.FindAll(".user-card")).TextContent);
    }

    [Fact]
    public void ManagerCanSeeUsersButCannotCreateUsers()
    {
        var employee = Manager with { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@example.com", Role = "Employee" };
        Api.Json("api/users", new ViewUsersResponse([Manager, employee]));
        var page = RenderComponent<Users>();

        page.WaitForElement(".user-card");
        Assert.Equal(2, page.FindAll(".user-card").Count);
        Assert.Empty(page.FindAll(".users-header button"));
    }

    [Fact]
    public void AdminCanCreateUserWithExpectedPayload()
    {
        var admin = Manager with { Role = "Admin" };
        Api.Json("api/users/me", admin);
        Api.Json("api/users", new ViewUsersResponse([admin]));
        Api.Accept(HttpMethod.Post, "api/users");
        var page = RenderComponent<Users>();

        page.WaitForAssertion(() => Assert.False(page.Find(".users-header button").HasAttribute("disabled")));
        page.Find(".users-header button").Click();
        page.Find("#user-name").Change("New Employee");
        page.Find("#user-email").Change("new@example.com");
        page.Find("#user-password").Change("Test-password-123!");
        page.Find("form").Submit();

        page.WaitForAssertion(() =>
        {
            Assert.Empty(page.FindAll("dialog"));
            var request = Assert.Single(Api.Requests.Where(request => request.Method == HttpMethod.Post));
            var payload = JsonSerializer.Deserialize<CreateUserRequest>(request.Body!, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.Equal(new CreateUserRequest("New Employee", "new@example.com", "Test-password-123!", "Employee"), payload);
        });
    }

    [Fact]
    public void ManagerCanViewUserDetailsButCannotEditThem()
    {
        var employee = Manager with { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@example.com", Role = "Employee" };
        Api.Json("api/users", new ViewUsersResponse([Manager, employee]));
        Api.Json($"api/users/{employee.Id}", employee);
        var page = RenderComponent<Users>();

        page.WaitForElement(".user-card").Click();
        page.WaitForElement(".user-metadata");

        Assert.Contains("employee@example.com", page.Find(".user-dialog").TextContent);
        Assert.Empty(page.FindAll(".user-dialog form"));
        Assert.Empty(page.FindAll(".admin-section"));
        Assert.Empty(page.FindAll(".danger-zone"));
    }

    [Fact]
    public void AdminCanSeeUsersAssignedManager()
    {
        var admin = Manager with { Role = "Admin" };
        var employee = Manager with
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@example.com",
            Role = "Employee",
            ManagerId = Manager.Id
        };
        Api.Json("api/users/me", admin);
        Api.Json("api/users", new ViewUsersResponse([admin, Manager, employee]));
        Api.Json($"api/users/{employee.Id}", employee);
        var page = RenderComponent<Users>();

        page.WaitForElement(".user-card").Click();

        page.WaitForAssertion(() =>
            Assert.Contains(Manager.Name, page.Find(".user-metadata").TextContent));
    }

    [Fact]
    public void EmployeeCannotLoadUserListOrCreateUsers()
    {
        Api.Json("api/users/me", Manager with { Role = "Employee" });

        var page = RenderComponent<Users>();

        page.WaitForAssertion(() =>
        {
            Assert.Contains("kun tilgængelig", page.Find(".alert-info").TextContent);
            Assert.Empty(page.FindAll(".users-header button"));
        });
        Assert.DoesNotContain(Api.Requests, request => request.Path == "api/users");
    }
}
