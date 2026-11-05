using HF.Domain.Common.Entities;

namespace HF.Domain.Courses;

/// <summary>
/// Represents an individual subject or curricular discipline (Matéria).
/// </summary>
public class Subject(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Name of the subject (e.g., Portuguese Language, Mathematics, Digital Literacy).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Identifying code for the subject.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Curriculum syllabus or content description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Subject workload in hours.
    /// </summary>
    public int WorkloadHours { get; set; }

    /// <summary>
    /// Optional foreign key to the parent course.
    /// </summary>
    public string? CourseId { get; set; }

    /// <summary>
    /// Navigation property to the parent course.
    /// </summary>
    public Course? Course { get; set; }

    /// <summary>
    /// School unit where the subject is taught.
    /// </summary>
    public string? SchoolUnitId { get; set; }

    /// <summary>
    /// Subject operational status.
    /// </summary>
    public SubjectStatus Status { get; set; } = SubjectStatus.Active;
}

