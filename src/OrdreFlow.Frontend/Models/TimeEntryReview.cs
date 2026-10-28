namespace OrdreFlow.Frontend.Models;

public sealed record ReturnTimeEntryRequest(string Reason);

public sealed record ReviewTimeEntryRequest(
    string Decision,
    string? Reason);

public sealed record TimeEntryReviewResponse(
    Guid Id,
    Guid TimeEntryId,
    string Decision,
    string? Reason,
    DateTimeOffset ReviewedAt);

public sealed record ViewTimeEntryReviewsResponse(
    IReadOnlyCollection<TimeEntryReviewResponse> Items);
