using System.Globalization;
using System.Net;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Pages;
using OrdreFlow.Frontend.Services;
using Xunit;

namespace OrdreFlow.Frontend.Tests;

public sealed class TimeEntriesPageTests : PageTestContext
{
    private static readonly DateTimeOffset AcceptedAt = new(2026, 1, 15, 14, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ReturnedAt = new(2026, 1, 16, 9, 45, 0, TimeSpan.FromHours(1));

    private readonly TimeEntryResponse entry = new(
        Guid.NewGuid(), Guid.NewGuid(), DateTime.Today, 1.5m, null, Guid.NewGuid(), "pending");

    private string ReviewsPath => $"api/time_entries/{entry.Id:D}/reviews";

    public TimeEntriesPageTests()
    {
        Services.AddScoped<TasksApiClient>();
        Services.AddScoped<ITimeEntriesApiClient, TimeEntriesApiClient>();
        Api.Json("api/orders", new ViewOrdersResponse([]));
        Api.Json("api/tasks", new ViewTasksResponse([]));
        Api.Json("api/time_entries", new ViewTimeEntriesResponse([entry]));
    }

    [Theory]
    [InlineData("pending", "Afventer godkendelse")]
    [InlineData("returned", "Returneret")]
    [InlineData("unrecognized", "unrecognized")]
    public void ListShowsEntryStatusAndEditLinkWithoutLoadingReviews(string status, string label)
    {
        Api.Json("api/time_entries", new ViewTimeEntriesResponse([entry with { Status = status }]));

        var page = RenderPage();

        Assert.Equal($"Status: {label}", page.Find(".entry-status").TextContent.Trim());
        Assert.Equal($"/register-time/{entry.Id:D}", page.Find(".entry-actions a").GetAttribute("href"));
        Assert.DoesNotContain(Api.Requests, request => request.Path == ReviewsPath);
    }

    [Fact]
    public async Task GetReviewsUsesGetWithNoBodyAndDeserializesNullableReasonsAndDates()
    {
        Api.Json(ReviewsPath, SampleReviews(entry.Id));

        var result = await Services.GetRequiredService<ITimeEntriesApiClient>().GetReviewsAsync(entry.Id);

        Assert.True(result.IsSuccess);
        Assert.Collection(result.Value!.Items,
            accepted =>
            {
                Assert.Equal(entry.Id, accepted.TimeEntryId);
                Assert.Equal("accepted", accepted.Decision);
                Assert.Null(accepted.Reason);
                Assert.Equal(AcceptedAt, accepted.ReviewedAt);
            },
            returned =>
            {
                Assert.Equal(entry.Id, returned.TimeEntryId);
                Assert.Equal("returned", returned.Decision);
                Assert.Equal("Ret venligst timerne", returned.Reason);
                Assert.Equal(ReturnedAt, returned.ReviewedAt);
            });
        var request = Assert.Single(Api.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(ReviewsPath, request.Path);
        Assert.Null(request.Body);
    }

    [Fact]
    public void ExpandingReviewsFetchesAndRendersDecisionsReasonsAndDates()
    {
        Api.Json(ReviewsPath, SampleReviews(entry.Id));
        var page = RenderPage();

        Assert.True(page.Find(".entry-reviews").HasAttribute("hidden"));
        Assert.DoesNotContain(Api.Requests, request => request.Path == ReviewsPath);

        page.Find(".entry-actions button").Click();

        page.WaitForAssertion(() =>
        {
            var reviews = page.FindAll(".review-item");
            Assert.Equal(2, reviews.Count);
            Assert.Equal("Godkendt", reviews[0].QuerySelector(".review-decision")!.TextContent);
            Assert.Equal("Returneret", reviews[1].QuerySelector(".review-decision")!.TextContent);
            Assert.Empty(reviews[0].QuerySelectorAll(".review-reason"));
            Assert.Contains("Ret venligst timerne", reviews[1].QuerySelector(".review-reason")!.TextContent);
            AssertReviewDate(reviews[0], AcceptedAt);
            AssertReviewDate(reviews[1], ReturnedAt);
            Assert.Equal("true", page.Find(".entry-actions button").GetAttribute("aria-expanded"));
        });
        var request = Assert.Single(Api.Requests, request => request.Path == ReviewsPath);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Null(request.Body);
    }

    [Fact]
    public void EmptyReviewsAreShownOnlyAfterSuccessfulFetch()
    {
        Api.Json(ReviewsPath, new ViewTimeEntryReviewsResponse([]));
        var page = RenderPage();

        Assert.DoesNotContain("Ingen vurderinger endnu.", page.Markup);
        page.Find(".entry-actions button").Click();

        page.WaitForAssertion(() =>
            Assert.Equal("Ingen vurderinger endnu.", page.Find(".entry-reviews [role='status']").TextContent));
        Assert.Empty(page.FindAll(".review-item"));
    }

    [Fact]
    public void RefreshReloadsEmptyAndPopulatedReviewsWithoutTogglingPanel()
    {
        Api.Json(ReviewsPath, new ViewTimeEntryReviewsResponse([]));
        var page = RenderPage();
        page.Find(".entry-actions button").Click();
        page.WaitForAssertion(() => Assert.Contains("Ingen vurderinger endnu.", page.Markup));

        Api.Json(ReviewsPath, SampleReviews(entry.Id));
        Assert.Contains("Opdater vurderinger", page.Find(".entry-reviews button").TextContent);
        page.Find(".entry-reviews button").Click();

        page.WaitForAssertion(() =>
        {
            Assert.Equal(2, page.FindAll(".review-item").Count);
            Assert.DoesNotContain("Ingen vurderinger endnu.", page.Markup);
            Assert.Equal(2, Api.Requests.Count(request => request.Path == ReviewsPath));
        });

        Api.Json(ReviewsPath, new ViewTimeEntryReviewsResponse([]));
        page.Find(".entry-reviews button").Click();

        page.WaitForAssertion(() =>
        {
            Assert.Contains("Ingen vurderinger endnu.", page.Markup);
            Assert.Empty(page.FindAll(".review-item"));
            Assert.Equal(3, Api.Requests.Count(request => request.Path == ReviewsPath));
        });
        Assert.All(Api.Requests.Where(request => request.Path == ReviewsPath), request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Null(request.Body);
        });
    }

    [Fact]
    public void FailedRefreshKeepsPreviousReviewsAndRetryReplacesThem()
    {
        Api.Json(ReviewsPath, SampleReviews(entry.Id));
        var page = RenderPage();
        page.Find(".entry-actions button").Click();
        page.WaitForAssertion(() => Assert.Equal(2, page.FindAll(".review-item").Count));

        Api.Fail(HttpMethod.Get, ReviewsPath, HttpStatusCode.ServiceUnavailable,
            new ApiError("reviews_unavailable", "Vurderinger er midlertidigt utilgængelige."));
        page.Find(".entry-reviews button").Click();

        page.WaitForAssertion(() =>
        {
            var alert = page.Find(".entry-reviews [role='alert']").TextContent;
            Assert.Contains("Vurderinger er midlertidigt utilgængelige.", alert);
            Assert.Contains("Tidligere hentede vurderinger vises nedenfor.", alert);
            Assert.Equal(2, page.FindAll(".review-item").Count);
            Assert.Equal("Godkendt", page.Find(".review-decision").TextContent);
            Assert.Contains("Prøv igen", page.Find(".entry-reviews button").TextContent);
            Assert.Equal(2, Api.Requests.Count(request => request.Path == ReviewsPath));
        });

        Api.Json(ReviewsPath, new ViewTimeEntryReviewsResponse([]));
        page.Find(".entry-reviews button").Click();

        page.WaitForAssertion(() =>
        {
            Assert.Empty(page.FindAll(".entry-reviews [role='alert']"));
            Assert.Empty(page.FindAll(".review-item"));
            Assert.Equal("Ingen vurderinger endnu.", page.Find(".entry-reviews [role='status']").TextContent);
            Assert.Equal(3, Api.Requests.Count(request => request.Path == ReviewsPath));
        });
    }

    [Fact]
    public void FailedRefreshOfCachedEmptyReviewsDoesNotClaimTheyAreCurrent()
    {
        Api.Json(ReviewsPath, new ViewTimeEntryReviewsResponse([]));
        var page = RenderPage();
        page.Find(".entry-actions button").Click();
        page.WaitForAssertion(() => Assert.Contains("Ingen vurderinger endnu.", page.Markup));

        Api.Fail(HttpMethod.Get, ReviewsPath, HttpStatusCode.ServiceUnavailable,
            new ApiError("reviews_unavailable", "Vurderinger er midlertidigt utilgængelige."));
        page.Find(".entry-reviews button").Click();

        page.WaitForAssertion(() =>
        {
            Assert.Contains("Tidligere hentede vurderinger vises nedenfor.",
                page.Find(".entry-reviews [role='alert']").TextContent);
            Assert.Equal("Ingen vurderinger ved sidste hentning.",
                page.Find(".entry-reviews [role='status']").TextContent);
            Assert.DoesNotContain("Ingen vurderinger endnu.", page.Markup);
            Assert.Contains("Prøv igen", page.Find(".entry-reviews button").TextContent);
        });
    }

    [Fact]
    public void FailedReviewFetchShowsErrorInsteadOfEmptyAndRetryRecovers()
    {
        Api.Fail(HttpMethod.Get, ReviewsPath, HttpStatusCode.ServiceUnavailable,
            new ApiError("reviews_unavailable", "Vurderinger er midlertidigt utilgængelige."));
        var page = RenderPage();

        page.Find(".entry-actions button").Click();

        page.WaitForAssertion(() =>
            Assert.Contains("Vurderinger er midlertidigt utilgængelige.",
                page.Find(".entry-reviews [role='alert']").TextContent));
        Assert.DoesNotContain("Ingen vurderinger endnu.", page.Markup);

        Api.Json(ReviewsPath, SampleReviews(entry.Id));
        page.Find(".entry-reviews button").Click();

        page.WaitForAssertion(() => Assert.Equal(2, page.FindAll(".review-item").Count));
        Assert.Empty(page.FindAll(".entry-reviews [role='alert']"));
        Assert.Equal(2, Api.Requests.Count(request => request.Path == ReviewsPath));
    }

    [Fact]
    public void CollapsingAndReopeningUsesCachedReviewsButChangingWeekResetsThem()
    {
        Api.Json(ReviewsPath, SampleReviews(entry.Id));
        var page = RenderPage();

        page.Find(".entry-actions button").Click();
        page.WaitForAssertion(() => Assert.Equal(2, page.FindAll(".review-item").Count));
        page.Find(".entry-actions button").Click();
        Assert.Equal("false", page.Find(".entry-actions button").GetAttribute("aria-expanded"));
        page.Find(".entry-actions button").Click();
        Assert.Equal(2, page.FindAll(".review-item").Count);
        Assert.Single(Api.Requests, request => request.Path == ReviewsPath);

        page.Find("button[aria-label='Næste uge']").Click();

        page.WaitForAssertion(() =>
        {
            Assert.Equal(2, Api.Requests.Count(request => request.Path == "api/time_entries"));
            Assert.Equal("false", page.Find(".entry-actions button").GetAttribute("aria-expanded"));
            Assert.Empty(page.FindAll(".review-item"));
        });
        Assert.Single(Api.Requests, request => request.Path == ReviewsPath);

        page.Find(".entry-actions button").Click();
        page.WaitForAssertion(() =>
        {
            Assert.Equal(2, page.FindAll(".review-item").Count);
            Assert.Equal(2, Api.Requests.Count(request => request.Path == ReviewsPath));
        });
    }

    private IRenderedComponent<TimeEntries> RenderPage()
    {
        var page = RenderComponent<TimeEntries>();
        page.WaitForElement(".entry-card");
        return page;
    }

    private static object SampleReviews(Guid entryId) => new
    {
        items = new[]
        {
            new
            {
                id = Guid.NewGuid(), timeEntryId = entryId, decision = "accepted",
                reason = (string?)null, reviewedAt = AcceptedAt.ToString("O", CultureInfo.InvariantCulture)
            },
            new
            {
                id = Guid.NewGuid(), timeEntryId = entryId, decision = "returned",
                reason = (string?)"Ret venligst timerne", reviewedAt = ReturnedAt.ToString("O", CultureInfo.InvariantCulture)
            }
        }
    };

    private static void AssertReviewDate(AngleSharp.Dom.IElement review, DateTimeOffset expected)
    {
        var time = review.QuerySelector("time")!;
        Assert.Equal(expected.ToString("O", CultureInfo.InvariantCulture), time.GetAttribute("datetime"));
        Assert.Equal(expected.ToLocalTime().ToString("dd/MM/yyyy 'kl.' HH:mm", CultureInfo.InvariantCulture),
            time.TextContent);
    }
}
