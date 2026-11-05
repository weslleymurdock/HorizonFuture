using System;

namespace HF.Shared.Subjects.Models;

/// <summary>
/// Operational status of an educational subject or curricular component.
/// </summary>
public enum SubjectStatus
{
    Active = 1,
    Inactive = 2
}

/// <summary>
/// DTO representing a subject returned by the API for UI presentation.
/// </summary>
public sealed record SubjectItemDto(
    string Id,
    string Name,
    string Code,
    int WorkloadHours,
    string? Description,
    string? CourseId,
    string? SchoolUnitId,
    SubjectStatus Status,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy);

/// <summary>
/// DTO for creating a new subject.
/// </summary>
public sealed record CreateSubjectDto(
    string Name,
    string Code,
    int WorkloadHours,
    string? Description = null,
    string? CourseId = null,
    string? SchoolUnitId = null,
    SubjectStatus Status = SubjectStatus.Active);

/// <summary>
/// DTO for updating an existing subject.
/// </summary>
public sealed record UpdateSubjectDto(
    string Id,
    string Name,
    string Code,
    int WorkloadHours,
    string? Description = null,
    string? CourseId = null,
    string? SchoolUnitId = null,
    SubjectStatus Status = SubjectStatus.Active);

/// <summary>
/// Optional filters for listing subjects.
/// </summary>
public sealed record SubjectFilterDto(
    string? CourseId = null,
    string? SchoolUnitId = null,
    SubjectStatus? Status = null);

