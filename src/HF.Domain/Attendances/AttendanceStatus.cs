namespace HF.Domain.Attendances;

/// <summary>
/// Status of a student's attendance in a class session.
/// </summary>
public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    ExcusedAbsence = 4
}

