using HF.Domain.Managers;

namespace HF.Application.Managers.Responses;

/// <summary>
/// Data contract representing a manager.
/// </summary>
public sealed record ManagerResponse(
    string Id,
    string UserId,
    string EmployeeNumber,
    string Department,
    string RoleTitle,
    string? SchoolUnitId,
    DateOnly StartDate,
    ManagerStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

