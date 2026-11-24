using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Pages;
using OrdreFlow.Frontend.Services;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

public sealed class TimeRegistrationConfirmationTests : PageTestContext
{
    private readonly Guid workId = Guid.NewGuid();
    private readonly Guid entryId = Guid.NewGuid();
    private readonly AccessibleWorkResponse work;

    public TimeRegistrationConfirmationTests()
    {
        Services.AddScoped<ITimeEntriesApiClient, TimeEntriesApiClient>();
        Services.AddScoped<TimeEntrySaveNotice>();

        work = new AccessibleWorkResponse(
            workId, "Montage", null, "Work", false, true, null, null,
            Guid.NewGuid(), "Produktion", Guid.NewGuid(), "Værksted");
        Api.Json("api/work-items", new ItemsResponse<AccessibleWorkResponse>([work]));
        Api.Json($"api/work-items/{workId:D}", work);
        Api.Json($"api/time_entries/{entryId:D}", Entry());
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Created, "Tiden er registreret.")]
    [InlineData(true, HttpStatusCode.OK, "Ændringerne er gemt.")]
    public void SuccessfulSaveShowsOneTimeConfirmationOnSavedDay(bool edit, HttpStatusCode status, string message)
    {
        var method = edit ? HttpMethod.Put : HttpMethod.Post;
        var path = edit ? $"api/time_entries/{entryId:D}" : "api/time_entries";
        Api.Accept(method, path, status);
        Api.Json("api/time_entries", new ViewTimeEntriesResponse([Entry()]));

        var form = RenderForm(edit);
        form.WaitForElement("form").Submit();

        var navigation = Services.GetRequiredService<NavigationManager>();
        form.WaitForAssertion(() =>
        {
            Assert.Contains(Api.Requests, request => request.Method == method && request.Path == path);
            Assert.EndsWith($"/?date={DateTime.Today:yyyy-MM-dd}", navigation.Uri);
        });

        var home = RenderComponent<Home>();
        home.WaitForAssertion(() =>
        {
            Assert.Contains(message, home.Find(".save-toast[role=status]").TextContent);
            Assert.Contains("Montage", home.Find(".day-panel").TextContent);
        });

        home.Find(".save-toast-close").Click();
        Assert.Empty(home.FindAll(".save-toast"));

        var revisitedHome = RenderComponent<Home>();
        Assert.Empty(revisitedHome.FindAll(".save-toast"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedSaveKeepsFormAndDoesNotQueueConfirmation(bool edit)
    {
        var method = edit ? HttpMethod.Put : HttpMethod.Post;
        var path = edit ? $"api/time_entries/{entryId:D}" : "api/time_entries";
        Api.Fail(method, path, HttpStatusCode.BadRequest, new ApiError("InvalidEntry", "Kunne ikke gemme tiden."));

        var form = RenderForm(edit);
        var navigation = Services.GetRequiredService<NavigationManager>();
        var beforeSave = navigation.Uri;
        form.WaitForElement("form").Submit();

        form.WaitForAssertion(() =>
        {
            Assert.Contains("Kunne ikke gemme tiden.", form.Find("[role=alert]").TextContent);
            Assert.Equal(beforeSave, navigation.Uri);
            Assert.Null(Services.GetRequiredService<TimeEntrySaveNotice>().Take());
        });
    }

    private IRenderedComponent<RegisterTime> RenderForm(bool edit) => edit
        ? RenderComponent<RegisterTime>(parameters => parameters.Add(page => page.EntryId, entryId))
        : RenderComponent<RegisterTime>();

    private TimeEntryResponse Entry() =>
        new(entryId, workId, DateTime.Today, 1m, null, Manager.Id, "Draft");
}
