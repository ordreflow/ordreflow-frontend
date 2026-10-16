namespace OrdreFlow.Frontend.Models;

public sealed record CreateOrderRequest(
    string Name);

public sealed record UpdateOrderRequest(
    string Name);

public sealed record ViewOrdersResponse(
    IReadOnlyCollection<OrderResponse> Items);

public sealed record OrderResponse(
    Guid Id,
    string Name,
    string Status,
    DateTime CreatedAt,
    DateTime? ClosedAt);
