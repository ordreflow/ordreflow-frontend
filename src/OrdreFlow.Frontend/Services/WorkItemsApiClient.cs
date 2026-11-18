using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public sealed class WorkItemsApiClient(HttpClient httpClient)
{
    public Task<ApiResult<ItemsResponse<AccessibleWorkResponse>>> GetAccessibleAsync(
        bool assignedToMe = false,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = $"?assignedToMe={assignedToMe.ToString().ToLowerInvariant()}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        return ApiResponse.ReadAsync<ItemsResponse<AccessibleWorkResponse>>(
            () => httpClient.GetAsync($"api/work-items{query}", cancellationToken), cancellationToken);
    }

    public Task<ApiResult<AccessibleWorkResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<AccessibleWorkResponse>(
            () => httpClient.GetAsync($"api/work-items/{id:D}", cancellationToken), cancellationToken);
}
