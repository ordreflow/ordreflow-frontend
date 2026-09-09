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
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("api/time_entries", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return ApiResult<TimeEntry>.Failure(
                [new ApiError("network_error", "Could not reach the server. Check that the API is running and reachable.")]);
        }

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var timeEntry = await response.Content.ReadFromJsonAsync<TimeEntry>(cancellationToken: cancellationToken);
            return ApiResult<TimeEntry>.Success(timeEntry!);
        }

        var errors = await response.Content.ReadFromJsonAsync<List<ApiError>>(cancellationToken: cancellationToken);
        return ApiResult<TimeEntry>.Failure(errors ?? []);
    }
}
