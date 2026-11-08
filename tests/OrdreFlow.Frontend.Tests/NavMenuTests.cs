using Bunit;
using Microsoft.Extensions.DependencyInjection;
using OrdreFlow.Frontend.Layout;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Services;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

public sealed class NavMenuTests : PageTestContext
{
    [Fact]
    public async Task EmployeeSeesOnlyOverviewAndOwnTime()
    {
        var menu = await RenderAsync(Manager with { Role = "Employee" });

        Assert.Equal(["Min tid"], GroupTitles(menu));
        Assert.Equal(["Overblik", "Registrer tid", "Mine registreringer"], ItemLabels(menu));
        Assert.Contains("Medarbejder", menu.Find(".user-role").TextContent);
    }

    [Fact]
    public async Task ManagerSeesManagementAndAdministrationGroups()
    {
        var menu = await RenderAsync(Manager with { Name = "Mette Madsen" });

        Assert.Equal(["Min tid", "Ledelse", "Administration"], GroupTitles(menu));
        Assert.Equal(["Overblik", "Registrer tid", "Mine registreringer", "Godkend tid", "Eksporter tid",
            "Ordrer", "Rapporter", "Brugere"], ItemLabels(menu));
        Assert.Equal("MM", menu.Find(".user-avatar").TextContent);
        Assert.Equal("Leder", menu.Find(".user-role").TextContent);
    }

    [Fact]
    public async Task GroupCanBeCollapsedAndOpenedAgain()
    {
        var menu = await RenderAsync(Manager);
        var toggle = menu.FindAll(".nav-group-toggle").Single(button => button.TextContent.Contains("Ledelse"));

        toggle.Click();
        Assert.DoesNotContain("Godkend tid", ItemLabels(menu));
        Assert.Equal("false", menu.FindAll(".nav-group-toggle")[1].GetAttribute("aria-expanded"));

        menu.FindAll(".nav-group-toggle")[1].Click();
        Assert.Contains("Godkend tid", ItemLabels(menu));
    }

    private async Task<IRenderedComponent<NavMenu>> RenderAsync(UserResponse user)
    {
        await Services.GetRequiredService<AuthState>().SetUserAsync(user);
        return RenderComponent<NavMenu>();
    }

    private static string[] GroupTitles(IRenderedComponent<NavMenu> menu) =>
        menu.FindAll(".nav-group-toggle span").Select(title => title.TextContent).ToArray();

    private static string[] ItemLabels(IRenderedComponent<NavMenu> menu) =>
        menu.FindAll(".nav-item span").Select(label => label.TextContent).ToArray();
}
