namespace OrdreFlow.Frontend.Models;

public sealed record CreateTaskRequest(
    Guid OrderId,
    string Title,
    string Description);

public sealed record UpdateTaskRequest(
    string Title,
    string Description);

public sealed record ViewTasksRequest(
    Guid? OrderId);

public sealed record ViewTasksResponse(
    IReadOnlyCollection<TaskResponse> Items);

public sealed record TaskResponse(
    Guid Id,
    Guid OrderId,
    string Title,
    string Description);
