using HF.Domain.Common.Entities;

namespace HF.Domain.Collaborators;

/// <summary>
/// Represents an operational or administrative collaborator/staff member supporting the NGO.
/// Has a mandatory 1-to-1 relationship with the authentication user.
/// </summary>
public class Collaborator(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Foreign key referencing the authentication user in infrastructure.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    public string? PersonId { get; set; }

    /// <summary>
    /// Employee registration number.
    /// </summary>
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>
    /// Job title or function (e.g., Social Worker, Administrative Assistant, Psychologist, Cook).
    /// </summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Department (e.g., Social Work, Administration, Operations, Nutrition).
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// School unit where the collaborator is allocated.
    /// </summary>
    public string? SchoolUnitId { get; set; }

    /// <summary>
    /// Type of employment contract.
    /// </summary>
    public ContractType ContractType { get; set; } = ContractType.PermanentClt;

    /// <summary>
    /// Date of admission.
    /// </summary>
    public DateOnly AdmissionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Current employment status.
    /// </summary>
    public CollaboratorStatus Status { get; set; } = CollaboratorStatus.Active;
}
