using HF.Domain.Teachers;

namespace HF.Application.Teachers.Responses;

/// <summary>
/// Data contract representing a teacher.
/// </summary>
public sealed record TeacherResponse(
    string Id,
    string UserId,
    string EmployeeNumber,
    string Specialty,
    string? AcademicDegree,
    string? Bio,
    string? SchoolUnitId,
    DateOnly HireDate,
    TeacherStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

