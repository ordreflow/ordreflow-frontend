using System.Net.Http.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public sealed class DepartmentsApiClient(HttpClient httpClient)
{
    public Task<ApiResult<ItemsResponse<DepartmentResponse>>> GetDepartmentsAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ItemsResponse<DepartmentResponse>>(
            () => httpClient.GetAsync("api/departments", cancellationToken), cancellationToken);

    public Task<ApiResult<ItemsResponse<DepartmentPersonResponse>>> GetManagedEmployeesAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ItemsResponse<DepartmentPersonResponse>>(
            () => httpClient.GetAsync("api/departments/managed-employees", cancellationToken), cancellationToken);

    public Task<ApiResult<CreatedResponse>> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<CreatedResponse>(
            () => httpClient.PostAsJsonAsync("api/departments", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> AddEmployeeAsync(Guid departmentId, Guid employeeId, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsync($"api/departments/{departmentId:D}/employees/{employeeId:D}", content: null, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> RemoveEmployeeAsync(Guid departmentId, Guid employeeId, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.DeleteAsync($"api/departments/{departmentId:D}/employees/{employeeId:D}", cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> AppointManagerAsync(Guid departmentId, Guid employeeId, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsync($"api/departments/{departmentId:D}/managers/{employeeId:D}", content: null, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> RemoveManagerAsync(Guid departmentId, Guid employeeId, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.DeleteAsync($"api/departments/{departmentId:D}/managers/{employeeId:D}", cancellationToken), cancellationToken);
}
