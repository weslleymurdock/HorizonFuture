using HF.Domain.ClassSessions;
using HF.Domain.Common.Entities;
using HF.Domain.Courses;
using HF.Domain.Teachers;

namespace HF.Domain.TeacherAssignments;

/// <summary>
/// Represents the assignment of an educator to teach a specific subject in an academic period.
/// </summary>
public class TeacherAssignment(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Foreign key referencing the assigned teacher.
    /// </summary>
    public string TeacherId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the assigned teacher.
    /// </summary>
    public Teacher? Teacher { get; set; }

    /// <summary>
    /// Foreign key referencing the subject being taught.
    /// </summary>
    public string SubjectId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the subject being taught.
    /// </summary>
    public Subject? Subject { get; set; }

    /// <summary>
    /// Academic year of the assignment (e.g., 2026).
    /// </summary>
    public int AcademicYear { get; set; }

    /// <summary>
    /// Academic semester or term (e.g., 1 for 1st semester, 2 for 2nd semester).
    /// </summary>
    public int AcademicSemester { get; set; }

    /// <summary>
    /// Role of the teacher in this assignment (e.g., primary titular instructor or substitute).
    /// </summary>
    public TeacherAssignmentRole Role { get; set; } = TeacherAssignmentRole.Primary;

    /// <summary>
    /// Class sessions conducted or scheduled under this assignment.
    /// </summary>
    public ICollection<ClassSession> ClassSessions { get; set; } = [];
}

