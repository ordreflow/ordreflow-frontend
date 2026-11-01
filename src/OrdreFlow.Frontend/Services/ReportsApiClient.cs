using System.Globalization;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class ReportsApiClient(HttpClient httpClient)
{
    public Task<ApiResult<GetTopOrdersResponse>> GetTopOrdersAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var to = toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return ApiResponse.ReadAsync<GetTopOrdersResponse>(
            () => httpClient.GetAsync($"api/reports/top-orders?fromDate={from}&toDate={to}", cancellationToken),
            cancellationToken);
    }
}
