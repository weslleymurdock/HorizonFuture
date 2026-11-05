using HF.Domain.Attendances;
using HF.Domain.Common.Entities;
using HF.Domain.Courses;
using HF.Domain.Students;

namespace HF.Domain.Enrollments;

/// <summary>
/// Represents a student's enrollment in a subject for a specific academic period.
/// </summary>
public class Enrollment(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Foreign key referencing the enrolled student.
    /// </summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the enrolled student.
    /// </summary>
    public Student? Student { get; set; }

    /// <summary>
    /// Foreign key referencing the subject in which the student is enrolled.
    /// </summary>
    public string SubjectId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the subject.
    /// </summary>
    public Subject? Subject { get; set; }

    /// <summary>
    /// Academic year of the enrollment (e.g., 2026).
    /// </summary>
    public int AcademicYear { get; set; }

    /// <summary>
    /// Academic semester or term (e.g., 1 for 1st semester, 2 for 2nd semester).
    /// </summary>
    public int AcademicSemester { get; set; }

    /// <summary>
    /// Date when the student was enrolled in the subject.
    /// </summary>
    public DateOnly EnrollmentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Current lifecycle status of the enrollment (e.g., Active, Locked, Completed, Cancelled).
    /// </summary>
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    /// <summary>
    /// Optional final grade achieved by the student in this subject for this period.
    /// </summary>
    public decimal? FinalGrade { get; set; }

    /// <summary>
    /// Attendance records associated with this enrollment.
    /// </summary>
    public ICollection<Attendance> Attendances { get; set; } = [];
}

