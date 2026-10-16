using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public class TasksApiClient(HttpClient httpClient)
{
    public Task<ApiResult<ViewTasksResponse>> GetTasksAsync(
        Guid? orderId = null,
        CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ViewTasksResponse>(
            () => httpClient.GetAsync(
                orderId is Guid id ? $"api/tasks?OrderId={id:D}" : "api/tasks", cancellationToken),
            cancellationToken);

    public Task<ApiResult<TaskResponse>> GetTaskAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<TaskResponse>(
            () => httpClient.GetAsync($"api/tasks/{id:D}", cancellationToken), cancellationToken);
}
