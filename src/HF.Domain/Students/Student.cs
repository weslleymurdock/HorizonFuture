using HF.Domain.Common.Entities;
using HF.Domain.Enrollments;

namespace HF.Domain.Students;

/// <summary>
/// Represents a student enrolled in the NGO's educational programs.
/// Has a mandatory 1-to-1 relationship with the authentication user.
/// </summary>
public class Student(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Foreign key referencing the authentication user in infrastructure.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Canonical person identity. UserId remains a compatibility link for the
    /// existing education flow and is not the civil identity.
    /// </summary>
    public string? PersonId { get; set; }

    /// <summary>
    /// Student registration or enrollment code within the institution.
    /// </summary>
    public string RegistrationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateOnly DateOfBirth { get; set; }

    /// <summary>
    /// Name of the legal guardian (parent, tutor).
    /// </summary>
    public string? GuardianName { get; set; }

    /// <summary>
    /// Relationship of the guardian to the student (e.g., Mother, Father, Grandmother).
    /// </summary>
    public string? GuardianRelationship { get; set; }

    /// <summary>
    /// Contact telephone of the legal guardian.
    /// </summary>
    public string? GuardianPhone { get; set; }

    /// <summary>
    /// Emergency contact name.
    /// </summary>
    public string? EmergencyContactName { get; set; }

    /// <summary>
    /// Emergency contact phone number.
    /// </summary>
    public string? EmergencyContactPhone { get; set; }

    /// <summary>
    /// Identifier of the school unit where the student is enrolled.
    /// </summary>
    public string? SchoolUnitId { get; set; }

    /// <summary>
    /// Current enrollment status.
    /// </summary>
    public StudentStatus Status { get; set; } = StudentStatus.Active;

    /// <summary>
    /// Accessibility or special educational needs requirements.
    /// </summary>
    public string? SpecialNeeds { get; set; }

    /// <summary>
    /// Medical considerations, allergies, or dietary restrictions.
    /// </summary>
    public string? MedicalNotes { get; set; }

    /// <summary>
    /// Date of first enrollment with the NGO.
    /// </summary>
    public DateOnly EnrollmentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Subject enrollments associated with this student.
    /// </summary>
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
