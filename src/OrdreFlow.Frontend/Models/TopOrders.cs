namespace OrdreFlow.Frontend.Models;

public sealed record TopOrdersResponse(
    Guid OrderId,
    string OrderName,
    decimal TotalHours);

public sealed record GetTopOrdersResponse(
    IReadOnlyList<TopOrdersResponse> Items);
