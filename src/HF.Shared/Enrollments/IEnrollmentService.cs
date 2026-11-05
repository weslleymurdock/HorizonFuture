using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Enrollments.Models;

namespace HF.Shared.Enrollments;

/// <summary>
/// Client service contract for all REST endpoints of EnrollmentsController.
/// </summary>
public interface IEnrollmentService
{
    /// <summary>
    /// Lists enrollment records matching optional filters (GET /api/v1/enrollments).
    /// </summary>
    Task<EnrollmentResult<IReadOnlyList<EnrollmentItemDto>>> ListAsync(EnrollmentFilterDto? filter = null, CancellationToken ct = default);

    /// <summary>
    /// Gets an enrollment record by unique identifier (GET /api/v1/enrollments/{id}).
    /// </summary>
    Task<EnrollmentResult<EnrollmentItemDto>> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new enrollment record (POST /api/v1/enrollments).
    /// </summary>
    Task<EnrollmentResult<EnrollmentItemDto>> CreateAsync(CreateEnrollmentDto request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing enrollment record (PUT /api/v1/enrollments/{id}).
    /// </summary>
    Task<EnrollmentResult<EnrollmentItemDto>> UpdateAsync(UpdateEnrollmentDto request, CancellationToken ct = default);

    /// <summary>
    /// Deletes an enrollment record by unique identifier (DELETE /api/v1/enrollments/{id}).
    /// </summary>
    Task<EnrollmentResult> DeleteAsync(string id, CancellationToken ct = default);
}

