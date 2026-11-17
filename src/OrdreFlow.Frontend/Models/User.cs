namespace OrdreFlow.Frontend.Models;

public sealed record AssignManagerRequest(Guid ManagerId);

public sealed record ChangeUserRoleRequest(string Role);

// A displayed title only. It never grants access; access is controlled by the role (system access).
public sealed record ChangeJobTitleRequest(string? JobTitle);

public sealed record CreateUserRequest(
    string Name,
    string Email,
    string Password,
    string Role,
    Guid? ManagerId = null,
    string? JobTitle = null);

public sealed record UpdateUserRequest(
    string Name,
    string Email);

public sealed record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string Status,
    DateTime CreatedAt,
    Guid? ManagerId = null,
    string? JobTitle = null);

public sealed record ViewUsersResponse(
    IReadOnlyCollection<UserResponse> Items);
