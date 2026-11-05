using HF.Domain.Common.Entities;

namespace HF.Domain.Courses;

/// <summary>
/// Represents an educational course, vocational track, or workshop series offered by the NGO.
/// </summary>
public class Course(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Official title or name of the course.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique programmatic code or acronym identifying the course.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Pedagogical description and objectives.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Total course workload in hours.
    /// </summary>
    public int WorkloadHours { get; set; }

    /// <summary>
    /// School unit offering the course.
    /// </summary>
    public string? SchoolUnitId { get; set; }

    /// <summary>
    /// Intended beneficiary audience (e.g., Youth 12-16, Adults, Early Childhood).
    /// </summary>
    public string? TargetAudience { get; set; }

    /// <summary>
    /// Current lifecycle status of the course.
    /// </summary>
    public CourseStatus Status { get; set; } = CourseStatus.Active;

    /// <summary>
    /// Subjects or curricular components belonging to this course.
    /// </summary>
    public ICollection<Subject> Subjects { get; set; } = [];
}

