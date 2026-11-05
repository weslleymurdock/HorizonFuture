namespace HF.Domain.Enrollments;

/// <summary>
/// Status of a student's enrollment in a subject.
/// </summary>
public enum EnrollmentStatus
{
    Active = 1,
    Locked = 2,
    Completed = 3,
    Cancelled = 4
}

