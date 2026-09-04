using System.Net;
using System.Net.Http.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class TimeEntriesApiClient(HttpClient httpClient) : ITimeEntriesApiClient
{
    public async Task<ApiResult<TimeEntry>> CreateAsync(
        CreateTimeEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/time_entries", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var timeEntry = await response.Content.ReadFromJsonAsync<TimeEntry>(cancellationToken: cancellationToken);
            return ApiResult<TimeEntry>.Success(timeEntry!);
        }

        var errors = await response.Content.ReadFromJsonAsync<List<ApiError>>(cancellationToken: cancellationToken);
        return ApiResult<TimeEntry>.Failure(errors ?? []);
    }
}
