using System.Net.Http.Json;
using System.Text.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class BackendHealthClient(HttpClient httpClient)
{
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync("api/health", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var health = await response.Content.ReadFromJsonAsync<BackendHealthResponse>(cancellationToken: cancellationToken);
            return string.Equals(health?.Status, "ok", StringComparison.Ordinal);
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return false;
        }
    }
}
