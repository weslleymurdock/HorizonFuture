using HF.Domain.Common.Entities;

namespace HF.Domain.Volunteers;

/// <summary>
/// Represents a volunteer contributing time and expertise to NGO activities.
/// Has a mandatory 1-to-1 relationship with the authentication user.
/// </summary>
public class Volunteer(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Foreign key referencing the authentication user in infrastructure.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Activity scope or areas of expertise (e.g., Reading Support, Art Workshop, Sports, IT Mentoring).
    /// </summary>
    public string? SkillsAndInterests { get; set; }

    /// <summary>
    /// Estimated weekly volunteer hours committed.
    /// </summary>
    public int? AvailableHoursPerWeek { get; set; }

    /// <summary>
    /// Notes regarding availability (e.g., Saturday mornings, weekday afternoons).
    /// </summary>
    public string? AvailabilityNotes { get; set; }

    /// <summary>
    /// Associated school unit where volunteer activities take place.
    /// </summary>
    public string? SchoolUnitId { get; set; }

    /// <summary>
    /// Date volunteer engagement commenced.
    /// </summary>
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Date volunteer engagement ended, if applicable.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Emergency contact name.
    /// </summary>
    public string? EmergencyContactName { get; set; }

    /// <summary>
    /// Emergency contact phone number.
    /// </summary>
    public string? EmergencyContactPhone { get; set; }

    /// <summary>
    /// Current engagement status.
    /// </summary>
    public VolunteerStatus Status { get; set; } = VolunteerStatus.Active;
}

