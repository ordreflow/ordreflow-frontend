using System.Net.Http.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class DepartmentsApiClient(HttpClient httpClient)
{
    public Task<ApiResult<ViewDepartmentsResponse>> GetDepartmentsAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ViewDepartmentsResponse>(
            () => httpClient.GetAsync("api/departments", cancellationToken), cancellationToken);

    public Task<ApiResult<ViewUsersResponse>> GetManagedEmployeesAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ViewUsersResponse>(
            () => httpClient.GetAsync("api/departments/managed-employees", cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsJsonAsync("api/departments", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> RenameAsync(Guid id, RenameDepartmentRequest request, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsJsonAsync($"api/departments/{id}", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> SetMemberAsync(
        Guid departmentId,
        Guid userId,
        SetDepartmentMemberRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsJsonAsync($"api/departments/{departmentId}/members/{userId}", request, cancellationToken),
            cancellationToken);

    public Task<ApiResult<bool>> RemoveMemberAsync(Guid departmentId, Guid userId, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.DeleteAsync($"api/departments/{departmentId}/members/{userId}", cancellationToken),
            cancellationToken);
}
