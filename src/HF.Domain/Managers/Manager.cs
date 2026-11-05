using HF.Domain.Common.Entities;

namespace HF.Domain.Managers;

/// <summary>
/// Represents a pedagogical, operational, or project manager in the NGO.
/// Has a mandatory 1-to-1 relationship with the authentication user.
/// </summary>
public class Manager(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Foreign key referencing the authentication user in infrastructure.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Institutional registry or employee number.
    /// </summary>
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>
    /// Management department or area (e.g., Pedagogical Coordination, Project Operations, Social Assistance).
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Formal job title or position (e.g., Pedagogical Coordinator, Operations Manager).
    /// </summary>
    public string RoleTitle { get; set; } = string.Empty;

    /// <summary>
    /// School unit under the manager's oversight, or null for general/organization-wide management.
    /// </summary>
    public string? SchoolUnitId { get; set; }

    /// <summary>
    /// Date when management duties started.
    /// </summary>
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Current management status.
    /// </summary>
    public ManagerStatus Status { get; set; } = ManagerStatus.Active;
}

