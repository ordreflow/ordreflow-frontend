using System.Net;
using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Pages;
using OrdreFlow.Frontend.Services;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

public sealed class ExportTimeEntriesPageTests : PageTestContext
{
    private const string ExportPath = "api/time_entries/export";
    private const string Header = "Date,Employee,Order,Task,Hours,Comment,Status,EmployeeId,TaskId,TimeEntryId\r\n";

    private readonly BunitJSModuleInterop download;

    public ExportTimeEntriesPageTests()
    {
        Services.AddScoped<TasksApiClient>();
        Services.AddScoped<ITimeEntriesApiClient, TimeEntriesApiClient>();
        download = JSInterop.SetupModule("./js/download-file.js");
        download.SetupVoid("downloadFile", _ => true).SetVoidResult();
    }

    [Fact]
    public async Task DownloadsCsvForTheChosenPeriod()
    {
        var csv = Encoding.UTF8.GetBytes(Header +
            "2026-10-05,Anna,Order,Task,7.5,\"Two\nlines\",Accepted,a,b,c\r\n" +
            "2026-10-06,Bo,Order,Task,2,,Accepted,a,b,d\r\n");
        Api.File(ExportPath, csv, "text/csv", "time-entries-20261001-20261031.csv");
        var page = await RenderAsync(Manager);

        page.Find("#export-from").Change("2026-10-01");
        page.Find("#export-to").Change("2026-10-31");
        page.Find(".export-btn").Click();

        page.WaitForAssertion(() =>
        {
            Assert.Contains("/api/time_entries/export?FromDate=2026-10-01&ToDate=2026-10-31", Api.RequestUris);
            var invocation = Assert.Single(download.Invocations, item => item.Identifier == "downloadFile");
            Assert.Equal("time-entries-20261001-20261031.csv", invocation.Arguments[0]);
            Assert.Equal("text/csv", invocation.Arguments[1]);
            Assert.Equal(csv, invocation.Arguments[2]);
            Assert.Contains("med 2 registreringer", page.Find(".alert-info").TextContent);
        });
    }

    [Fact]
    public async Task EmptyExportShowsMessageInsteadOfDownloading()
    {
        Api.File(ExportPath, Encoding.UTF8.GetBytes(Header), "text/csv", "empty.csv");
        var page = await RenderAsync(Manager);

        page.Find(".export-btn").Click();

        page.WaitForAssertion(() =>
        {
            Assert.Contains("ingen godkendte tidsregistreringer", page.Find(".alert-info").TextContent);
            Assert.Empty(download.Invocations);
        });
    }

    [Fact]
    public async Task ApiErrorIsShown()
    {
        Api.Fail(HttpMethod.Get, ExportPath, HttpStatusCode.Forbidden,
            new ApiError("ExportForbidden", "The user is not authorized to export time entries."));
        var page = await RenderAsync(Manager);

        page.Find(".export-btn").Click();

        page.WaitForAssertion(() =>
            Assert.Contains("not authorized", page.Find(".alert-danger").TextContent));
    }

    [Fact]
    public async Task EndBeforeStartDisablesDownload()
    {
        var page = await RenderAsync(Manager);

        page.Find("#export-from").Change("2026-10-31");
        page.Find("#export-to").Change("2026-10-01");

        Assert.True(page.Find(".export-btn").HasAttribute("disabled"));
        Assert.Contains("Til-datoen", page.Find(".validation-message").TextContent);
    }

    [Fact]
    public async Task EmployeeCannotExport()
    {
        var page = await RenderAsync(Manager with { Role = "Employee" });

        Assert.Contains("kun tilgængelig", page.Find(".alert-info").TextContent);
        Assert.Empty(page.FindAll(".export-btn"));
    }

    private async Task<IRenderedComponent<ExportTimeEntries>> RenderAsync(UserResponse user)
    {
        await Services.GetRequiredService<AuthState>().SetUserAsync(user);
        return RenderComponent<ExportTimeEntries>();
    }
}
