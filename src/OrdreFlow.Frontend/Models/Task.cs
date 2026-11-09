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
    string Description,
    string Status = "Open",
    DateTime? ClosedAt = null)
{
    // Time can't be registered on a closed task, so forms only offer open ones.
    public bool IsOpen => !string.Equals(Status, "Closed", StringComparison.OrdinalIgnoreCase);
}
