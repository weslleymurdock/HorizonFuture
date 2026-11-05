using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Subjects.Models;

namespace HF.Shared.Subjects;

/// <summary>
/// Client service contract for all REST endpoints of SubjectsController.
/// </summary>
public interface ISubjectService
{
    /// <summary>
    /// Lists subject records matching optional filters (GET /api/v1/subjects).
    /// </summary>
    Task<SubjectResult<IReadOnlyList<SubjectItemDto>>> ListAsync(SubjectFilterDto? filter = null, CancellationToken ct = default);

    /// <summary>
    /// Gets a subject record by unique identifier (GET /api/v1/subjects/{id}).
    /// </summary>
    Task<SubjectResult<SubjectItemDto>> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new subject record (POST /api/v1/subjects).
    /// </summary>
    Task<SubjectResult<SubjectItemDto>> CreateAsync(CreateSubjectDto request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing subject record (PUT /api/v1/subjects/{id}).
    /// </summary>
    Task<SubjectResult<SubjectItemDto>> UpdateAsync(UpdateSubjectDto request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a subject record by unique identifier (DELETE /api/v1/subjects/{id}).
    /// </summary>
    Task<SubjectResult> DeleteAsync(string id, CancellationToken ct = default);
}

