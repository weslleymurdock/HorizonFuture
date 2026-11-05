using HF.Domain.Attendances;
using HF.Domain.Common.Entities;
using HF.Domain.TeacherAssignments;

namespace HF.Domain.ClassSessions;

/// <summary>
/// Represents a specific class session or lesson conducted under a teacher assignment.
/// </summary>
public class ClassSession(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Foreign key referencing the teacher assignment under which this session is held.
    /// Determines the subject, teacher, and academic period.
    /// </summary>
    public string TeacherAssignmentId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the teacher assignment.
    /// </summary>
    public TeacherAssignment? TeacherAssignment { get; set; }

    /// <summary>
    /// Date when the class session takes place.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Scheduled start time of the session.
    /// </summary>
    public TimeOnly? StartTime { get; set; }

    /// <summary>
    /// Duration of the class session in minutes.
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Pedagogical notes, topics covered, or observations regarding the session.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Attendance records registered for this class session.
    /// </summary>
    public ICollection<Attendance> Attendances { get; set; } = [];
}

