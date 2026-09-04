using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

// Backend has no orders/work-items endpoint yet, so this stands in until it does.
// Swap this registration for a real API-backed IOrdersProvider once that endpoint exists.
public class DummyOrdersProvider : IOrdersProvider
{
    private static readonly IReadOnlyList<Order> Orders =
    [
        new Order(1, "Install shelving unit", "Netto Vestergade"),
        new Order(2, "Repair conveyor belt", "VS Automatic Plant 2"),
        new Order(3, "Quarterly maintenance check", "Bilka Logistikcenter"),
        new Order(4, "Replace hydraulic pump", "VS Automatic Plant 1")
    ];

    public Task<IReadOnlyList<Order>> GetOrdersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Orders);
}
