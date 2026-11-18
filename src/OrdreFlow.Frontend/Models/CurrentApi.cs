namespace OrdreFlow.Frontend.Models;

public sealed record ItemsResponse<T>(IReadOnlyList<T> Items);

public sealed record CreatedResponse(Guid Id);

public sealed record EmployeeResponse(
    Guid Id,
    string Name,
    string Email,
    string? Title,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<string>? Permissions = null)
{
    public bool HasPermission(string permission) =>
        Permissions?.Contains(permission, StringComparer.OrdinalIgnoreCase) == true;

    public string Role =>
        HasPermission("Administrator") ? "Admin" :
        HasPermission("DepartmentManager") ? "Manager" : "Employee";
}

public sealed record CreateEmployeeRequest(
    string Name,
    string Email,
    string Password,
    string? Title,
    IReadOnlyCollection<string> Permissions);

public sealed record UpdateEmployeeRequest(string Name, string? Title);

public sealed record SetEmployeePermissionsRequest(IReadOnlyCollection<string> Permissions);

public sealed record DepartmentPersonResponse(
    Guid EmployeeId,
    string Name,
    string Email,
    string? Title,
    string Status);

public sealed record DepartmentResponse(
    Guid Id,
    string Name,
    bool ManagedByMe,
    IReadOnlyList<DepartmentPersonResponse> Managers,
    IReadOnlyList<DepartmentPersonResponse> Members);

public sealed record CreateDepartmentRequest(string Name);

public sealed record AccessibleWorkResponse(
    Guid Id,
    string Name,
    string? Description,
    string Kind,
    bool AssignmentOnly,
    bool AssignedToMe,
    decimal? ExpectedHours,
    DateOnly? ExpectedDeadline,
    Guid CategoryId,
    string CategoryName,
    Guid DepartmentId,
    string DepartmentName);

public sealed record WorkItemAssignmentResponse(Guid EmployeeId, decimal ExpectedHours, DateOnly ExpectedDeadline);

public sealed record WorkItemResponse(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    string Kind,
    bool AssignmentOnly,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<WorkItemAssignmentResponse>? Assignments = null);

public sealed record PlannableCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid DepartmentId,
    string DepartmentName,
    bool InternalOnly,
    string Status,
    IReadOnlyList<WorkItemResponse> WorkItems);

public sealed record CreateCategoryRequest(string Name, bool InternalOnly = false, string? Description = null);

public sealed record CreateWorkItemRequest(
    string Name,
    string? Description = null,
    string? Kind = null,
    bool AssignmentOnly = false,
    IReadOnlyList<CreateWorkItemAssignmentRequest>? Assignments = null);

public sealed record CreateWorkItemAssignmentRequest(Guid EmployeeId, decimal ExpectedHours, DateOnly ExpectedDeadline);

public sealed record UpdateCategoryRequest(string Name, string? Description = null);

public sealed record UpdateWorkItemRequest(
    string Name,
    string? Description = null,
    string? Kind = null,
    bool AssignmentOnly = false,
    IReadOnlyList<CreateWorkItemAssignmentRequest>? Assignments = null);

public sealed record CreateExportRequest(
    Guid ExportId,
    DateOnly FromDate,
    DateOnly ToDate,
    Guid? EmployeeId = null);

public sealed record ExportSummaryResponse(
    Guid Id,
    Guid ExportedBy,
    DateTime CreatedAtUtc,
    DateOnly FromDate,
    DateOnly ToDate,
    Guid? EmployeeId,
    int EntryCount,
    decimal TotalHours,
    Guid? TimeEntriesClosedBy,
    DateTime? TimeEntriesClosedAtUtc);

public sealed record ExportEntryResponse(Guid TimeEntryId, Guid TimeEntryAuditId);

public sealed record ExportDetailsResponse(
    ExportSummaryResponse Export,
    IReadOnlyList<ExportEntryResponse> Entries,
    string DownloadUrl);

public sealed record ExportEmployeeResponse(Guid Id, string Name, string Status);

public sealed record ExportPageResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize);

public sealed record TopCategoryResponse(
    Guid CategoryId,
    string CategoryName,
    Guid DepartmentId,
    string DepartmentName,
    decimal TotalHours,
    IReadOnlyList<TopCategoryTaskResponse> Tasks);

public sealed record TopCategoryTaskResponse(Guid TaskId, string TaskName, decimal TotalHours);
