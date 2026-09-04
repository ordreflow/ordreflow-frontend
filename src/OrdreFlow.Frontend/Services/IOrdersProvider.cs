using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public interface IOrdersProvider
{
    Task<IReadOnlyList<Order>> GetOrdersAsync(CancellationToken cancellationToken = default);
}
