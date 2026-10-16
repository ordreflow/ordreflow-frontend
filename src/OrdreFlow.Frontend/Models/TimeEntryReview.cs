namespace OrdreFlow.Frontend.Models;

public sealed record ReturnTimeEntryRequest(string Reason);

public sealed record ReviewTimeEntryRequest(
    string Decision,
    string? Reason);
