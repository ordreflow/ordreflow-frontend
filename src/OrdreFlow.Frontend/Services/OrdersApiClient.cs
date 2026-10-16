using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class OrdersApiClient(HttpClient httpClient)
{
    public Task<ApiResult<ViewOrdersResponse>> GetOrdersAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ViewOrdersResponse>(
            () => httpClient.GetAsync("api/orders", cancellationToken), cancellationToken);
}
