namespace OrdreFlow.Frontend.Models;

public sealed record CreateTimeEntryRequest(
    Guid TaskId,
    DateTime Date,
    decimal Hours,
    string? Comment);

public sealed record ExportTimeEntriesRequest(
    DateOnly FromDate,
    DateOnly ToDate);

public sealed record TimeEntryResponse(
    Guid Id,
    Guid TaskId,
    DateTime Date,
    decimal Hours,
    string? Comment,
    Guid EmployeeId,
    string Status);

public sealed record UpdateTimeEntryRequest(
    DateTime Date,
    decimal Hours,
    string? Comment);

public sealed record ViewTimeEntriesRequest(
    DateOnly FromDate,
    DateOnly ToDate);

public sealed record ViewTimeEntriesResponse(
    IReadOnlyCollection<TimeEntryResponse> Items);

public sealed record WeeklyTotalResponse(
    DateOnly FromDate,
    DateOnly ToDate,
    decimal TotalHours);
