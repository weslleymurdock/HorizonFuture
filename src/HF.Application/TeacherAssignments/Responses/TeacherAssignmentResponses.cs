using HF.Domain.TeacherAssignments;

namespace HF.Application.TeacherAssignments.Responses;

/// <summary>
/// Data contract representing a teacher assignment.
/// </summary>
public sealed record TeacherAssignmentResponse(
    string Id,
    string TeacherId,
    string SubjectId,
    int AcademicYear,
    int AcademicSemester,
    TeacherAssignmentRole Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

