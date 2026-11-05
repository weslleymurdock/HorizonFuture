using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Students.Models;

namespace HF.Shared.Students;

/// <summary>
/// Client service contract for all REST endpoints of StudentsController.
/// </summary>
public interface IStudentService
{
    /// <summary>
    /// Lists student records matching optional filters (GET /api/v1/students).
    /// </summary>
    Task<StudentResult<IReadOnlyList<StudentItemDto>>> ListAsync(StudentFilterDto? filter = null, CancellationToken ct = default);

    /// <summary>
    /// Gets a student record by unique identifier (GET /api/v1/students/{id}).
    /// </summary>
    Task<StudentResult<StudentItemDto>> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new student record (POST /api/v1/students).
    /// </summary>
    Task<StudentResult<StudentItemDto>> CreateAsync(CreateStudentDto request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing student record (PUT /api/v1/students/{id}).
    /// </summary>
    Task<StudentResult<StudentItemDto>> UpdateAsync(UpdateStudentDto request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a student record by unique identifier (DELETE /api/v1/students/{id}).
    /// </summary>
    Task<StudentResult> DeleteAsync(string id, CancellationToken ct = default);
}

