using System.Net.Http.Json;
using System.Text.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

internal static class ApiResponse
{
    public static async Task<ApiResult<T>> ReadAsync<T>(
        Func<Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken) where T : class
    {
        try
        {
            using var response = await send();
            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<T>.Failure(await ReadErrorsAsync(response, cancellationToken));
            }

            var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return value is not null
                ? ApiResult<T>.Success(value)
                : ApiResult<T>.Failure([new ApiError("invalid_response", "The server returned an empty response.")]);
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Failure([new ApiError("network_error", "Could not reach the server.")]);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ApiResult<T>.Failure([new ApiError("timeout", "The server did not respond in time.")]);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return ApiResult<T>.Failure([new ApiError("invalid_response", "The server returned an unexpected response.")]);
        }
    }

    public static async Task<ApiResult<bool>> WriteAsync(
        Func<Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await send();
            return response.IsSuccessStatusCode
                ? ApiResult<bool>.Success(true)
                : ApiResult<bool>.Failure(await ReadErrorsAsync(response, cancellationToken));
        }
        catch (HttpRequestException)
        {
            return ApiResult<bool>.Failure([new ApiError("network_error", "Could not reach the server.")]);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ApiResult<bool>.Failure([new ApiError("timeout", "The server did not respond in time.")]);
        }
    }

    private static async Task<IReadOnlyList<ApiError>> ReadErrorsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var errors = await response.Content.ReadFromJsonAsync<List<ApiError>>(cancellationToken: cancellationToken);
            if (errors is { Count: > 0 })
            {
                return errors;
            }
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // Some failures (such as 401) return ProblemDetails or no body.
        }

        return [new ApiError("http_error", $"The server rejected the request ({(int)response.StatusCode} {response.ReasonPhrase}).")];
    }
}
