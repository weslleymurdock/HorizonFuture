using HF.Domain.Directors;

namespace HF.Application.Directors.Responses;

/// <summary>
/// Data contract representing a director.
/// </summary>
public sealed record DirectorResponse(
    string Id,
    string UserId,
    string Directorate,
    string PositionTitle,
    string? SchoolUnitId,
    DateOnly MandateStart,
    DateOnly? MandateEnd,
    DirectorStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

