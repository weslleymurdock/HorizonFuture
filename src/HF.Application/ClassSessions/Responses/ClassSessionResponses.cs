namespace HF.Application.ClassSessions.Responses;

/// <summary>
/// Data contract representing a class session.
/// </summary>
public sealed record ClassSessionResponse(
    string Id,
    string TeacherAssignmentId,
    DateOnly Date,
    TimeOnly? StartTime,
    int? DurationMinutes,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

