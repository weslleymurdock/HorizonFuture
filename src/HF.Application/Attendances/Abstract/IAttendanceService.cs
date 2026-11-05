using HF.Application.Attendances.Requests;
using HF.Application.Attendances.Responses;

namespace HF.Application.Attendances.Abstract;

/// <summary>
/// Defines application use-case operations for attendance records.
/// </summary>
public interface IAttendanceService
{
    /// <summary>Creates a new attendance record.</summary>
    Task<AttendanceResponse> CreateAsync(CreateAttendanceRequest request, CancellationToken cancellationToken);

    /// <summary>Gets an attendance record by identifier.</summary>
    Task<AttendanceResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists attendance records matching optional criteria.</summary>
    Task<IReadOnlyList<AttendanceResponse>> ListAsync(ListAttendancesRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing attendance record.</summary>
    Task<AttendanceResponse?> UpdateAsync(UpdateAttendanceRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes an attendance record.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether an attendance record already exists for the class session and enrollment.</summary>
    Task<bool> AttendanceExistsAsync(string classSessionId, string enrollmentId, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Validates that the enrollment and class session belong to compatible subject and academic period.</summary>
    Task<(bool IsCompatible, string? Reason)> ValidateAcademicCompatibilityAsync(string classSessionId, string enrollmentId, CancellationToken cancellationToken);
}

