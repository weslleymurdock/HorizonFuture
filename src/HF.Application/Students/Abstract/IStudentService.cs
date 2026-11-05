using HF.Application.Students.Requests;
using HF.Application.Students.Responses;

namespace HF.Application.Students.Abstract;

/// <summary>
/// Defines application use-case operations for students.
/// </summary>
public interface IStudentService
{
    /// <summary>Creates a new student record.</summary>
    Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a student by identifier.</summary>
    Task<StudentResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists students matching optional criteria.</summary>
    Task<IReadOnlyList<StudentResponse>> ListAsync(ListStudentsRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing student record.</summary>
    Task<StudentResponse?> UpdateAsync(UpdateStudentRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a student record when no academic history prevents it.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether a registration number is already in use.</summary>
    Task<bool> RegistrationNumberExistsAsync(string registrationNumber, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Checks whether a user exists and is not already associated with another student.</summary>
    Task<bool> UserAvailableAsync(string userId, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Checks whether a student has enrolled subjects in their history.</summary>
    Task<bool> HasEnrollmentsAsync(string studentId, CancellationToken cancellationToken);
}

