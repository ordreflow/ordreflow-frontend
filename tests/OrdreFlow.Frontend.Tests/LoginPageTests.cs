using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Pages;
using OrdreFlow.Frontend.Services;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

// UC-06 Sign in locally: the login page signs in, and shows the API's login failures in Danish.
public sealed class LoginPageTests : PageTestContext
{
    private const string LoginPath = "api/auth/login";

    public LoginPageTests()
    {
        Services.AddScoped<AuthApiClient>();
        JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);
    }

    [Fact]
    public void ValidLoginSignsInLoadsProfileAndGoesToFrontPage()
    {
        Api.PostJson(LoginPath, new LoginResponse("token-123", "Bearer", DateTime.UtcNow.AddHours(1)));
        var page = RenderComponent<Login>();

        SubmitLogin(page, "manager@example.com", "Correct-Password1!");

        page.WaitForAssertion(() =>
        {
            var auth = Services.GetRequiredService<AuthState>();
            Assert.True(auth.IsAuthenticated);
            Assert.Equal(Manager.Id, auth.User!.Id);
            var navigation = Services.GetRequiredService<NavigationManager>();
            Assert.Equal("", navigation.ToBaseRelativePath(navigation.Uri));
        });
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "InvalidCredentials", "Efter 5 forkerte forsøg spærres kontoen")]
    [InlineData(HttpStatusCode.TooManyRequests, "TooManyLoginAttempts", "For mange loginforsøg")]
    [InlineData(HttpStatusCode.Forbidden, "UserInactive", "Din bruger er deaktiveret")]
    public void LoginFailuresAreShownWithoutSigningIn(HttpStatusCode status, string code, string message)
    {
        Api.Fail(HttpMethod.Post, LoginPath, status, new ApiError(code, "From the API"));
        var page = RenderComponent<Login>();

        SubmitLogin(page, "employee@example.com", "Some-Password1!");

        page.WaitForAssertion(() =>
        {
            Assert.Contains(message, page.Find("[role=alert]").TextContent);
            Assert.False(Services.GetRequiredService<AuthState>().IsAuthenticated);
            Assert.False(page.Find(".submit-btn").HasAttribute("disabled"));
        });
    }

    private static void SubmitLogin(IRenderedComponent<Login> page, string email, string password)
    {
        page.WaitForElement("#email").Change(email);
        page.Find("#password").Change(password);
        page.Find("form").Submit();
    }
}
