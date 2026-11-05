using System;

namespace HF.Shared.Enrollments.Models;

/// <summary>
/// Status of an enrollment.
/// </summary>
public enum EnrollmentStatus
{
    Active = 1,
    Locked = 2,
    Completed = 3,
    Cancelled = 4
}

/// <summary>
/// DTO representing an enrollment item for UI presentation.
/// </summary>
public sealed record EnrollmentItemDto(
    string Id,
    string StudentId,
    string SubjectId,
    int AcademicYear,
    int AcademicSemester,
    DateOnly EnrollmentDate,
    EnrollmentStatus Status,
    decimal? FinalGrade,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy);

/// <summary>
/// DTO for creating a new enrollment.
/// </summary>
public sealed record CreateEnrollmentDto(
    string StudentId,
    string SubjectId,
    int AcademicYear,
    int AcademicSemester,
    DateOnly? EnrollmentDate = null,
    EnrollmentStatus Status = EnrollmentStatus.Active,
    decimal? FinalGrade = null);

/// <summary>
/// DTO for updating an existing enrollment.
/// </summary>
public sealed record UpdateEnrollmentDto(
    string Id,
    string StudentId,
    string SubjectId,
    int AcademicYear,
    int AcademicSemester,
    DateOnly EnrollmentDate,
    EnrollmentStatus Status = EnrollmentStatus.Active,
    decimal? FinalGrade = null);

/// <summary>
/// Filter criteria for querying enrollments.
/// </summary>
public sealed record EnrollmentFilterDto(
    string? StudentId = null,
    string? SubjectId = null,
    int? AcademicYear = null,
    int? AcademicSemester = null,
    EnrollmentStatus? Status = null);

