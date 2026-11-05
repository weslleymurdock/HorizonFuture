using HF.Application.Courses.Requests;
using HF.Application.Courses.Responses;

namespace HF.Application.Courses.Abstract;

/// <summary>
/// Defines application use-case operations for subjects.
/// </summary>
public interface ISubjectService
{
    /// <summary>Creates a new subject record.</summary>
    Task<SubjectResponse> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a subject by identifier.</summary>
    Task<SubjectResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists subjects matching optional criteria.</summary>
    Task<IReadOnlyList<SubjectResponse>> ListAsync(ListSubjectsRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing subject record.</summary>
    Task<SubjectResponse?> UpdateAsync(UpdateSubjectRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a subject record when no academic history prevents it.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether a subject code is already in use.</summary>
    Task<bool> CodeExistsAsync(string code, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Checks whether a subject exists by identifier.</summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether a subject has teacher assignments or student enrollments.</summary>
    Task<bool> HasAcademicHistoryAsync(string subjectId, CancellationToken cancellationToken);
}

