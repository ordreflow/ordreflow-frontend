using System.Globalization;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class ReportsApiClient(HttpClient httpClient)
{
    public Task<ApiResult<ItemsResponse<TopCategoryResponse>>> GetTopCategoriesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var to = toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return ApiResponse.ReadAsync<ItemsResponse<TopCategoryResponse>>(
            () => httpClient.GetAsync($"api/reports/top-categories?fromDate={from}&toDate={to}", cancellationToken),
            cancellationToken);
    }
}
