using HF.Application.TeacherAssignments.Requests;
using HF.Application.TeacherAssignments.Responses;

namespace HF.Application.TeacherAssignments.Abstract;

/// <summary>
/// Defines application use-case operations for teacher assignments.
/// </summary>
public interface ITeacherAssignmentService
{
    /// <summary>Creates a new teacher assignment record.</summary>
    Task<TeacherAssignmentResponse> CreateAsync(CreateTeacherAssignmentRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a teacher assignment by identifier.</summary>
    Task<TeacherAssignmentResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists teacher assignments matching optional criteria.</summary>
    Task<IReadOnlyList<TeacherAssignmentResponse>> ListAsync(ListTeacherAssignmentsRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing teacher assignment record.</summary>
    Task<TeacherAssignmentResponse?> UpdateAsync(UpdateTeacherAssignmentRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a teacher assignment record when no class sessions prevent it.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether an assignment already exists for the teacher, subject, and academic period.</summary>
    Task<bool> AssignmentExistsAsync(string teacherId, string subjectId, int academicYear, int academicSemester, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Checks whether a teacher assignment exists by identifier.</summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether a teacher assignment has conducted or scheduled class sessions.</summary>
    Task<bool> HasClassSessionsAsync(string assignmentId, CancellationToken cancellationToken);
}

