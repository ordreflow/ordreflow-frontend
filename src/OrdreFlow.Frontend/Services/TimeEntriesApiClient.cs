using System.Globalization;
using System.Net.Http.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class TimeEntriesApiClient(HttpClient httpClient) : ITimeEntriesApiClient
{
    public Task<ApiResult<ViewTimeEntriesResponse>> GetEntriesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var to = toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return ApiResponse.ReadAsync<ViewTimeEntriesResponse>(
            () => httpClient.GetAsync($"api/time_entries?FromDate={from}&ToDate={to}", cancellationToken),
            cancellationToken);
    }

    public Task<ApiResult<ViewTimeEntriesResponse>> GetManagerEntriesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var to = toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return ApiResponse.ReadAsync<ViewTimeEntriesResponse>(
            () => httpClient.GetAsync($"api/time_entries/manager?FromDate={from}&ToDate={to}", cancellationToken),
            cancellationToken);
    }

    public Task<ApiResult<bool>> ReviewAsync(
        Guid id,
        ReviewTimeEntryRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsJsonAsync($"api/time_entries/{id:D}/review", request, cancellationToken), cancellationToken);

    public Task<ApiResult<TimeEntryResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<TimeEntryResponse>(
            () => httpClient.GetAsync($"api/time_entries/{id:D}", cancellationToken), cancellationToken);

    public Task<ApiResult<ViewTimeEntryReviewsResponse>> GetReviewsAsync(
        Guid timeEntryId,
        CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ViewTimeEntryReviewsResponse>(
            () => httpClient.GetAsync($"api/time_entries/{timeEntryId:D}/reviews", cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> CreateAsync(
        CreateTimeEntryRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsJsonAsync("api/time_entries", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(
        Guid id,
        UpdateTimeEntryRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsJsonAsync($"api/time_entries/{id:D}", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.DeleteAsync($"api/time_entries/{id:D}", cancellationToken), cancellationToken);
}
