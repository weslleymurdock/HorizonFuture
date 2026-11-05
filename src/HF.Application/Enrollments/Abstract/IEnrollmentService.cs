using HF.Application.Enrollments.Requests;
using HF.Application.Enrollments.Responses;

namespace HF.Application.Enrollments.Abstract;

/// <summary>
/// Defines application use-case operations for student enrollments.
/// </summary>
public interface IEnrollmentService
{
    /// <summary>Creates a new enrollment record.</summary>
    Task<EnrollmentResponse> CreateAsync(CreateEnrollmentRequest request, CancellationToken cancellationToken);

    /// <summary>Gets an enrollment by identifier.</summary>
    Task<EnrollmentResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists enrollments matching optional criteria.</summary>
    Task<IReadOnlyList<EnrollmentResponse>> ListAsync(ListEnrollmentsRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing enrollment record.</summary>
    Task<EnrollmentResponse?> UpdateAsync(UpdateEnrollmentRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes an enrollment record when no attendance history prevents it.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether an enrollment already exists for the student, subject, and academic period.</summary>
    Task<bool> EnrollmentExistsAsync(string studentId, string subjectId, int academicYear, int academicSemester, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Checks whether an enrollment exists by identifier.</summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether an enrollment has associated attendance records.</summary>
    Task<bool> HasAttendancesAsync(string enrollmentId, CancellationToken cancellationToken);
}

