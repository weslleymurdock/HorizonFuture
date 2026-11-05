using HF.Domain.Courses;

namespace HF.Application.Courses.Responses;

/// <summary>
/// Data contract representing a course.
/// </summary>
public sealed record CourseResponse(
    string Id,
    string Name,
    string Code,
    string? Description,
    int WorkloadHours,
    string? SchoolUnitId,
    string? TargetAudience,
    CourseStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

