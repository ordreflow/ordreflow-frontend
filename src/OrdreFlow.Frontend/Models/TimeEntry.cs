namespace OrdreFlow.Frontend.Models;

public record CreateTimeEntryRequest(
    int TaskId,
    DateTime Date,
    decimal Hours,
    string? Comment);

public record TimeEntry(
    int Id,
    int TaskId,
    DateTime Date,
    decimal Hours,
    string? Comment);
