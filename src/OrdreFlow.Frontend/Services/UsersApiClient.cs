using System.Net.Http.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class UsersApiClient(HttpClient httpClient)
{
    public Task<ApiResult<ViewUsersResponse>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ViewUsersResponse>(
            () => httpClient.GetAsync("api/users", cancellationToken), cancellationToken);

    public Task<ApiResult<UserResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<UserResponse>(
            () => httpClient.GetAsync($"api/users/{id}", cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsJsonAsync("api/users", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsJsonAsync($"api/users/{id}", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> ChangeRoleAsync(Guid id, ChangeUserRoleRequest request, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsJsonAsync($"api/users/{id}/role", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> AssignManagerAsync(Guid id, AssignManagerRequest request, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsJsonAsync($"api/users/{id}/manager", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.DeleteAsync($"api/users/{id}", cancellationToken), cancellationToken);

    public Task<ApiResult<UserResponse>> GetMeAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<UserResponse>(
            () => httpClient.GetAsync("api/users/me", cancellationToken), cancellationToken);
}
