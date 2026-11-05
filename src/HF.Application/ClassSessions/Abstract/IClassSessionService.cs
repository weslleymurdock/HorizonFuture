using HF.Application.ClassSessions.Requests;
using HF.Application.ClassSessions.Responses;

namespace HF.Application.ClassSessions.Abstract;

/// <summary>
/// Defines application use-case operations for class sessions.
/// </summary>
public interface IClassSessionService
{
    /// <summary>Creates a new class session record.</summary>
    Task<ClassSessionResponse> CreateAsync(CreateClassSessionRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a class session by identifier.</summary>
    Task<ClassSessionResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists class sessions matching optional criteria.</summary>
    Task<IReadOnlyList<ClassSessionResponse>> ListAsync(ListClassSessionsRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing class session record.</summary>
    Task<ClassSessionResponse?> UpdateAsync(UpdateClassSessionRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a class session record when no attendance history prevents it.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether a class session exists by identifier.</summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether a class session has associated attendance records.</summary>
    Task<bool> HasAttendancesAsync(string sessionId, CancellationToken cancellationToken);
}

