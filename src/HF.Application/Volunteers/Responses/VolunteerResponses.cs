using HF.Domain.Volunteers;

namespace HF.Application.Volunteers.Responses;

/// <summary>
/// Data contract representing a volunteer.
/// </summary>
public sealed record VolunteerResponse(
    string Id,
    string UserId,
    string? SkillsAndInterests,
    int? AvailableHoursPerWeek,
    string? AvailabilityNotes,
    string? SchoolUnitId,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    VolunteerStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

