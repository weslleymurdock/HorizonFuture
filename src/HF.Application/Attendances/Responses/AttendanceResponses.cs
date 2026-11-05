using HF.Domain.Attendances;

namespace HF.Application.Attendances.Responses;

/// <summary>
/// Data contract representing an attendance record.
/// </summary>
public sealed record AttendanceResponse(
    string Id,
    string ClassSessionId,
    string EnrollmentId,
    AttendanceStatus Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

