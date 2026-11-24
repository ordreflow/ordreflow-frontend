using System.Net.Http.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public sealed class WorkPlanningApiClient(HttpClient httpClient)
{
    public Task<ApiResult<ItemsResponse<PlannableCategoryResponse>>> GetPlannableCategoriesAsync(CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ItemsResponse<PlannableCategoryResponse>>(
            () => httpClient.GetAsync("api/categories/plannable", cancellationToken), cancellationToken);

    public Task<ApiResult<CreatedResponse>> CreateCategoryAsync(
        Guid departmentId,
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<CreatedResponse>(
            () => httpClient.PostAsJsonAsync($"api/departments/{departmentId:D}/categories", request, cancellationToken), cancellationToken);

    public Task<ApiResult<CreatedResponse>> CreateWorkItemAsync(
        Guid categoryId,
        CreateWorkItemRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<CreatedResponse>(
            () => httpClient.PostAsJsonAsync($"api/categories/{categoryId:D}/work-items", request, cancellationToken), cancellationToken);

    public Task<ApiResult<bool>> UpdateCategoryAsync(
        Guid categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsJsonAsync($"api/categories/{categoryId:D}", request, cancellationToken), cancellationToken);

    /// <summary>Deactivated categories and all their work can no longer be selected for time registration.</summary>
    public Task<ApiResult<bool>> SetCategoryActiveAsync(Guid categoryId, bool active, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsync($"api/categories/{categoryId:D}/{(active ? "activate" : "deactivate")}", null, cancellationToken),
            cancellationToken);

    /// <summary>Deactivated work can no longer be selected for time registration.</summary>
    public Task<ApiResult<bool>> SetWorkItemActiveAsync(Guid workItemId, bool active, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsync($"api/work-items/{workItemId:D}/{(active ? "activate" : "deactivate")}", null, cancellationToken),
            cancellationToken);

    public Task<ApiResult<bool>> UpdateWorkItemAsync(
        Guid workItemId,
        UpdateWorkItemRequest request,
        CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PutAsJsonAsync($"api/work-items/{workItemId:D}", request, cancellationToken), cancellationToken);
}
