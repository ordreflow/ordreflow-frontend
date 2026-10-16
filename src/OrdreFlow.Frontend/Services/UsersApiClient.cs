using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class UsersApiClient(HttpClient httpClient)
{
    public Task<ApiResult<UserResponse>> GetMeAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<UserResponse>(
            () => httpClient.GetAsync("api/users/me", cancellationToken), cancellationToken);
}
