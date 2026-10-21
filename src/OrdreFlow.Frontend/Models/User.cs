namespace OrdreFlow.Frontend.Models;

public sealed record AssignManagerRequest(Guid ManagerId);

public sealed record ChangeUserRoleRequest(string Role);

public sealed record CreateUserRequest(
    string Name,
    string Email,
    string Password,
    string Role);

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
    Guid? ManagerId = null);

public sealed record ViewUsersResponse(
    IReadOnlyCollection<UserResponse> Items);
