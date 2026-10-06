using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient.Timeout elapsed: the API address is most likely unreachable from this machine.
            return ApiResult<TimeEntry>.Failure(
                [new ApiError("timeout", $"The server did not respond ({httpClient.BaseAddress}). Check that the API is running and that ApiBaseUrl is correct.")]);
        }

        // Accept any 2xx (e.g. 201 Created), not just 200 OK.
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var timeEntry = await response.Content.ReadFromJsonAsync<TimeEntry>(cancellationToken: cancellationToken);
                if (timeEntry is not null)
                {
                    return ApiResult<TimeEntry>.Success(timeEntry);
                }
            }
            catch (JsonException)
            {
            }

            return ApiResult<TimeEntry>.Failure(
                [new ApiError("invalid_response", $"The server saved the entry but returned an unexpected response ({(int)response.StatusCode}).")]);
        }

        // Error bodies are not always the expected list (HTML, empty body, ProblemDetails), so don't crash on them.
        try
        {
            var errors = await response.Content.ReadFromJsonAsync<List<ApiError>>(cancellationToken: cancellationToken);
            if (errors is { Count: > 0 })
            {
                return ApiResult<TimeEntry>.Failure(errors);
            }
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
        }

        return ApiResult<TimeEntry>.Failure(
            [new ApiError("http_error", $"The server rejected the request ({(int)response.StatusCode} {response.ReasonPhrase}).")]);
    }
}
