using HF.Domain.Courses;

namespace HF.Application.Courses.Responses;

/// <summary>
/// Data contract representing a subject.
/// </summary>
public sealed record SubjectResponse(
    string Id,
    string Name,
    string Code,
    string? Description,
    int WorkloadHours,
    string? CourseId,
    string? SchoolUnitId,
    SubjectStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

