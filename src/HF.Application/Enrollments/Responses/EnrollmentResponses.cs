using HF.Domain.Enrollments;

namespace HF.Application.Enrollments.Responses;

/// <summary>
/// Data contract representing an enrollment.
/// </summary>
public sealed record EnrollmentResponse(
    string Id,
    string StudentId,
    string SubjectId,
    int AcademicYear,
    int AcademicSemester,
    DateOnly EnrollmentDate,
    EnrollmentStatus Status,
    decimal? FinalGrade,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

