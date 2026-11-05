using HF.Application.Courses.Requests;
using HF.Application.Courses.Responses;

namespace HF.Application.Courses.Abstract;

/// <summary>
/// Defines application use-case operations for courses.
/// </summary>
public interface ICourseService
{
    /// <summary>Creates a new course record.</summary>
    Task<CourseResponse> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a course by identifier.</summary>
    Task<CourseResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists courses matching optional criteria.</summary>
    Task<IReadOnlyList<CourseResponse>> ListAsync(ListCoursesRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing course record.</summary>
    Task<CourseResponse?> UpdateAsync(UpdateCourseRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a course record.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether a course code is already in use.</summary>
    Task<bool> CodeExistsAsync(string code, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Checks whether a course exists by identifier.</summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken);
}

