using HF.Domain.ClassSessions;
using HF.Domain.Common.Entities;
using HF.Domain.Enrollments;

namespace HF.Domain.Attendances;

/// <summary>
/// Records the attendance of an enrolled student in a specific class session.
/// </summary>
public class Attendance(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Foreign key referencing the class session.
    /// </summary>
    public string ClassSessionId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the class session.
    /// </summary>
    public ClassSession? ClassSession { get; set; }

    /// <summary>
    /// Foreign key referencing the enrollment of the student.
    /// </summary>
    public string EnrollmentId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the student's enrollment.
    /// </summary>
    public Enrollment? Enrollment { get; set; }

    /// <summary>
    /// Attendance result for the session (e.g., Present, Absent, Late, ExcusedAbsence).
    /// </summary>
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    /// <summary>
    /// Optional pedagogical observation or note (e.g., reason for absence or tardiness).
    /// </summary>
    public string? Notes { get; set; }
}

