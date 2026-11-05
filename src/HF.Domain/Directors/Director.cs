using HF.Domain.Common.Entities;

namespace HF.Domain.Directors;

/// <summary>
/// Represents an executive director or school director in the NGO.
/// Has a mandatory 1-to-1 relationship with the authentication user.
/// </summary>
public class Director(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Foreign key referencing the authentication user in infrastructure.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Directorate or board division (e.g., Executive Directorate, Pedagogical Directorate, Financial Directorate).
    /// </summary>
    public string Directorate { get; set; } = string.Empty;

    /// <summary>
    /// Formal title or position (e.g., Executive Director, School Director, President).
    /// </summary>
    public string PositionTitle { get; set; } = string.Empty;

    /// <summary>
    /// Associated school unit if directing a specific unit, or null for overall NGO leadership.
    /// </summary>
    public string? SchoolUnitId { get; set; }

    /// <summary>
    /// Start date of the administrative or statutory mandate.
    /// </summary>
    public DateOnly MandateStart { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// End date of the mandate, if fixed-term.
    /// </summary>
    public DateOnly? MandateEnd { get; set; }

    /// <summary>
    /// Current mandate status.
    /// </summary>
    public DirectorStatus Status { get; set; } = DirectorStatus.Active;
}

