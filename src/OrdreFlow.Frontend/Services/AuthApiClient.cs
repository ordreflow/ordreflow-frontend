using System.Net.Http.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class AuthApiClient(HttpClient httpClient)
{
    public Task<ApiResult<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<LoginResponse>(
            () => httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken), cancellationToken);
}
