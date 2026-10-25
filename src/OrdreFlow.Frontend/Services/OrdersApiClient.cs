using System.Net.Http.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class OrdersApiClient(HttpClient httpClient)
{
    public Task<ApiResult<OrderResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<OrderResponse>(
            () => httpClient.GetAsync($"api/orders/{id}", cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsJsonAsync("api/orders", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsJsonAsync($"api/orders/{id}", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> CloseAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsync($"api/orders/{id}/close", null, cancellationToken), cancellationToken);

    public Task<ApiResult<ViewOrdersResponse>> GetOrdersAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ViewOrdersResponse>(
            () => httpClient.GetAsync("api/orders", cancellationToken), cancellationToken);
}
