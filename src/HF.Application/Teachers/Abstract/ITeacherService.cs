using HF.Application.Teachers.Requests;
using HF.Application.Teachers.Responses;

namespace HF.Application.Teachers.Abstract;

/// <summary>
/// Defines application use-case operations for teachers.
/// </summary>
public interface ITeacherService
{
    /// <summary>Creates a new teacher record.</summary>
    Task<TeacherResponse> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a teacher by identifier.</summary>
    Task<TeacherResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists teachers matching optional criteria.</summary>
    Task<IReadOnlyList<TeacherResponse>> ListAsync(ListTeachersRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing teacher record.</summary>
    Task<TeacherResponse?> UpdateAsync(UpdateTeacherRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a teacher record when no academic history prevents it.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether an employee number is already in use.</summary>
    Task<bool> EmployeeNumberExistsAsync(string employeeNumber, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Checks whether a user exists and is not already associated with another teacher.</summary>
    Task<bool> UserAvailableAsync(string userId, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Checks whether a teacher has assignments in their history.</summary>
    Task<bool> HasAssignmentsAsync(string teacherId, CancellationToken cancellationToken);
}

