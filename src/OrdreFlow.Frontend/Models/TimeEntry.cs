namespace OrdreFlow.Frontend.Models;

public record CreateTimeEntryRequest(
    int WorkItemId,
    DateTime Date,
    decimal Hours,
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    string? Comment);

public record TimeEntry(
    int Id,
    int WorkItemId,
    DateTime Date,
    decimal Hours,
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    string? Comment);
