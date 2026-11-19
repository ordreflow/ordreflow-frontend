using System.Net.Http.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public sealed class EmployeesApiClient(HttpClient httpClient)
{
    public Task<ApiResult<EmployeeResponse>> GetMeAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<EmployeeResponse>(
            () => httpClient.GetAsync("api/employees/me", cancellationToken), cancellationToken);

    public Task<ApiResult<ItemsResponse<EmployeeResponse>>> GetEmployeesAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ItemsResponse<EmployeeResponse>>(
            () => httpClient.GetAsync("api/employees", cancellationToken), cancellationToken);

    public Task<ApiResult<EmployeeResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<EmployeeResponse>(
            () => httpClient.GetAsync($"api/employees/{id:D}", cancellationToken), cancellationToken);

    public Task<ApiResult<CreatedResponse>> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<CreatedResponse>(
            () => httpClient.PostAsJsonAsync("api/employees", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsJsonAsync($"api/employees/{id:D}", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> SetPermissionsAsync(
        Guid id,
        SetEmployeePermissionsRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsJsonAsync($"api/employees/{id:D}/permissions", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsync($"api/employees/{id:D}/activate", content: null, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsync($"api/employees/{id:D}/deactivate", content: null, cancellationToken), cancellationToken);
}
