using System.Net;
using Bunit;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Pages;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

public sealed class DepartmentsPageTests : PageTestContext
{
    private readonly UserResponse admin;
    private readonly UserResponse employee;
    private readonly UserResponse otherManager;
    private readonly DepartmentResponse service;

    public DepartmentsPageTests()
    {
        admin = Manager with { Id = Guid.NewGuid(), Name = "Alex Admin", Role = "Admin" };
        employee = Manager with { Id = Guid.NewGuid(), Name = "Casey Employee", Email = "casey@example.com", Role = "Employee", JobTitle = "Elektriker" };
        otherManager = Manager with { Id = Guid.NewGuid(), Name = "Sam Manager", Email = "sam@example.com" };
        service = new DepartmentResponse(Guid.NewGuid(), "Service", DateTime.UtcNow, false,
        [
            new DepartmentMemberResponse(Manager.Id, Manager.Name, Manager.Email, "Manager", "Active", null, true),
            new DepartmentMemberResponse(employee.Id, employee.Name, employee.Email, "Employee", "Active", "Elektriker", false)
        ]);
    }

    private void SignInAsAdmin()
    {
        Api.Json("api/users/me", admin);
        Api.Json("api/departments", new ViewDepartmentsResponse([service]));
        Api.Json("api/users", new ViewUsersResponse([admin, Manager, employee, otherManager]));
        Api.Json("api/departments/managed-employees", new ViewUsersResponse([]));
    }

    [Fact]
    public void AdminSeesDepartmentsWithManagersAndMemberCount()
    {
        SignInAsAdmin();

        var page = RenderComponent<Departments>();

        var card = page.WaitForElement(".department-card");
        Assert.Contains("Service", card.TextContent);
        Assert.Contains("2 medlemmer", card.TextContent);
        Assert.Contains("Afdelingsleder: Manager", card.TextContent);
        Assert.Empty(page.FindAll(".managed-panel"));
    }

    [Fact]
    public void AdminCreatesDepartmentWithTrimmedName()
    {
        SignInAsAdmin();
        Api.Accept(HttpMethod.Post, "api/departments");
        var page = RenderComponent<Departments>();

        page.WaitForAssertion(() => Assert.False(page.Find(".departments-header button").HasAttribute("disabled")));
        page.Find(".departments-header button").Click();
        page.Find("#department-name").Input("  Installation ");
        page.Find("form").Submit();

        page.WaitForAssertion(() =>
        {
            var request = Assert.Single(Api.Requests, request => request.Method == HttpMethod.Post);
            Assert.Equal("""{"name":"Installation"}""", request.Body);
            Assert.Contains("Installation er oprettet", page.Find(".alert-success").TextContent);
        });
    }

    [Fact]
    public void AdminAddsManagerAsDepartmentManager()
    {
        SignInAsAdmin();
        Api.Accept(HttpMethod.Put, $"api/departments/{service.Id}/members/{otherManager.Id}");
        var page = RenderComponent<Departments>();

        page.WaitForElement(".department-open").Click();
        page.Find("#add-member").Change(otherManager.Id.ToString());
        page.Find("#add-as-manager").Change(true);
        page.FindAll("button").Single(button => button.TextContent == "Tilføj til afdeling").Click();

        page.WaitForAssertion(() =>
        {
            var request = Assert.Single(Api.Requests, request => request.Method == HttpMethod.Put);
            Assert.Equal("""{"isManager":true}""", request.Body);
            Assert.Contains("Sam Manager er tilføjet som afdelingsleder", page.Find(".department-dialog .alert-success").TextContent);
        });
    }

    // FR-13/SEC-01: users with employee access cannot be made department managers, whatever their job title.
    [Fact]
    public void EmployeeAccessCannotBeMadeDepartmentManager()
    {
        var otherEmployee = employee with { Id = Guid.NewGuid(), Name = "Riley Employee", JobTitle = "Manager" };
        Api.Json("api/users/me", admin);
        Api.Json("api/departments", new ViewDepartmentsResponse([service]));
        Api.Json("api/users", new ViewUsersResponse([admin, Manager, employee, otherEmployee]));
        Api.Json("api/departments/managed-employees", new ViewUsersResponse([]));
        var page = RenderComponent<Departments>();

        page.WaitForElement(".department-open").Click();
        page.Find("#add-member").Change(otherEmployee.Id.ToString());

        Assert.True(page.Find("#add-as-manager").HasAttribute("disabled"));
        var employeeRow = page.FindAll(".member-row").Single(row => row.TextContent.Contains("Casey Employee"));
        Assert.DoesNotContain("Gør til leder", employeeRow.TextContent);
        var managerRow = page.FindAll(".member-row").Single(row => row.TextContent.Contains("Afdelingsleder"));
        Assert.Contains("Fjern som leder", managerRow.TextContent);
    }

    [Fact]
    public void AdminOnlyOffersActiveUsersWhoAreNotMembers()
    {
        var former = employee with { Id = Guid.NewGuid(), Name = "Former Employee", Status = "Inactive" };
        Api.Json("api/users/me", admin);
        Api.Json("api/departments", new ViewDepartmentsResponse([service]));
        Api.Json("api/users", new ViewUsersResponse([admin, Manager, employee, otherManager, former]));
        Api.Json("api/departments/managed-employees", new ViewUsersResponse([]));
        var page = RenderComponent<Departments>();

        page.WaitForElement(".department-open").Click();

        var options = page.FindAll("#add-member option").Select(option => option.TextContent).ToArray();
        Assert.Equal(3, options.Length);
        Assert.Contains(options, option => option.StartsWith("Alex Admin"));
        Assert.Contains(options, option => option.StartsWith("Sam Manager"));
    }

    [Fact]
    public void RemovingMemberRequiresConfirmation()
    {
        SignInAsAdmin();
        Api.Accept(HttpMethod.Delete, $"api/departments/{service.Id}/members/{employee.Id}");
        var page = RenderComponent<Departments>();

        page.WaitForElement(".department-open").Click();
        page.Find($"button[aria-label='Fjern {employee.Name} fra afdelingen']").Click();
        Assert.DoesNotContain(Api.Requests, request => request.Method == HttpMethod.Delete);

        page.FindAll("button").Single(button => button.TextContent == "Ja, fjern").Click();

        page.WaitForAssertion(() =>
            Assert.Single(Api.Requests, request => request.Method == HttpMethod.Delete));
    }

    [Fact]
    public void DuplicateNameShowsDanishMessage()
    {
        SignInAsAdmin();
        Api.Fail(HttpMethod.Post, "api/departments", HttpStatusCode.BadRequest,
            new ApiError("DepartmentNameTaken", "A department with this name already exists."));
        var page = RenderComponent<Departments>();

        page.WaitForAssertion(() => Assert.False(page.Find(".departments-header button").HasAttribute("disabled")));
        page.Find(".departments-header button").Click();
        page.Find("#department-name").Input("Service");
        page.Find("form").Submit();

        page.WaitForAssertion(() =>
            Assert.Equal("Der findes allerede en afdeling med det navn.", page.Find(".department-dialog .alert-danger").TextContent));
    }

    // FR-14: a manager sees exactly the employees they can plan work for, without admin controls.
    [Fact]
    public void ManagerSeesManagedEmployeesWithoutAdminControls()
    {
        var managed = service with { IsManagedByMe = true };
        Api.Json("api/departments", new ViewDepartmentsResponse([managed]));
        Api.Json("api/departments/managed-employees", new ViewUsersResponse([employee]));
        var page = RenderComponent<Departments>();

        var panel = page.WaitForElement(".managed-panel");
        Assert.Contains("Casey Employee", panel.TextContent);
        Assert.Contains("Elektriker", panel.TextContent);
        Assert.Contains("Du er afdelingsleder", page.Find(".department-card").TextContent);
        Assert.Empty(page.FindAll(".departments-header button"));
        Assert.DoesNotContain(Api.Requests, request => request.Path == "api/users");

        page.Find(".department-open").Click();
        Assert.Equal(2, page.FindAll(".member-row").Count);
        Assert.Empty(page.FindAll(".member-actions"));
        Assert.Empty(page.FindAll("#add-member"));
        Assert.Empty(page.FindAll("#rename-department"));
    }

    [Fact]
    public void EmployeeSeesOwnDepartmentsOnly()
    {
        Api.Json("api/users/me", employee);
        var own = service with
        {
            Members = [service.Members.Single(member => member.UserId == employee.Id)]
        };
        Api.Json("api/departments", new ViewDepartmentsResponse([own]));
        var page = RenderComponent<Departments>();

        var card = page.WaitForElement(".department-card");
        Assert.Contains("Du er medlem", card.TextContent);
        Assert.Empty(page.FindAll(".department-open"));
        Assert.Empty(page.FindAll(".managed-panel"));
        Assert.DoesNotContain(Api.Requests, request => request.Path == "api/departments/managed-employees");
    }
}
