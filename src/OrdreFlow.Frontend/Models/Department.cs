namespace OrdreFlow.Frontend.Models;

public sealed record CreateDepartmentRequest(string Name);

public sealed record RenameDepartmentRequest(string Name);

public sealed record SetDepartmentMemberRequest(bool IsManager);

public sealed record DepartmentResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    bool IsManagedByMe,
    IReadOnlyList<DepartmentMemberResponse> Members);

public sealed record DepartmentMemberResponse(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    string Status,
    string? JobTitle,
    bool IsManager);

public sealed record ViewDepartmentsResponse(
    IReadOnlyList<DepartmentResponse> Items);
