using HF.Domain.Common.Entities;
using HF.Domain.TeacherAssignments;

namespace HF.Domain.Teachers;

/// <summary>
/// Represents a teacher, instructor, or educator in the NGO.
/// Has a mandatory 1-to-1 relationship with the authentication user.
/// </summary>
public class Teacher(string? id = null) : EntityBase(id)
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
    /// Area of specialty or academic discipline (e.g., Mathematics, Literacy, Arts, Music, IT).
    /// </summary>
    public string Specialty { get; set; } = string.Empty;

    /// <summary>
    /// Academic degree or qualification summary.
    /// </summary>
    public string? AcademicDegree { get; set; }

    /// <summary>
    /// Brief pedagogical biography or profile.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Primary school unit where the teacher is assigned.
    /// </summary>
    public string? SchoolUnitId { get; set; }

    /// <summary>
    /// Date of admission or start of pedagogical activities.
    /// </summary>
    public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Current activity status.
    /// </summary>
    public TeacherStatus Status { get; set; } = TeacherStatus.Active;

    /// <summary>
    /// Teaching assignments associated with this teacher.
    /// </summary>
    public ICollection<TeacherAssignment> Assignments { get; set; } = [];
}

