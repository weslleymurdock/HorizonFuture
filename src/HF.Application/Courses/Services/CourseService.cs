using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Courses.Abstract;
using HF.Application.Courses.Requests;
using HF.Application.Courses.Responses;
using HF.Domain.Courses;

namespace HF.Application.Courses.Services;

/// <summary>
/// Implements application use cases and business rules for courses.
/// </summary>
public sealed class CourseService(IRepository<Course> courseRepository) : ICourseService
{
    /// <inheritdoc />
    public async Task<CourseResponse> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var course = new Course
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Description = request.Description?.Trim(),
            WorkloadHours = request.WorkloadHours,
            SchoolUnitId = request.SchoolUnitId?.Trim(),
            TargetAudience = request.TargetAudience?.Trim(),
            Status = request.Status
        };

        await courseRepository.AddAsync(course, cancellationToken);
        return MapToResponse(course);
    }

    /// <inheritdoc />
    public async Task<CourseResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetByIdAsync(id, [], cancellationToken);
        return course is null ? null : MapToResponse(course);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CourseResponse>> ListAsync(ListCoursesRequest request, CancellationToken cancellationToken)
    {
        var courses = await courseRepository.ListAsync(
            c => (request.SchoolUnitId == null || c.SchoolUnitId == request.SchoolUnitId) &&
                 (request.Status == null || c.Status == request.Status),
            [],
            cancellationToken);

        return courses.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<CourseResponse?> UpdateAsync(UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (course is null)
        {
            return null;
        }

        course.Name = request.Name.Trim();
        course.Code = request.Code.Trim().ToUpperInvariant();
        course.Description = request.Description?.Trim();
        course.WorkloadHours = request.WorkloadHours;
        course.SchoolUnitId = request.SchoolUnitId?.Trim();
        course.TargetAudience = request.TargetAudience?.Trim();
        course.Status = request.Status;
        course.UpdatedAt = DateTime.UtcNow;

        await courseRepository.UpdateAsync(course, cancellationToken);
        return MapToResponse(course);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (course is null)
        {
            return (false, "Course not found.");
        }

        await courseRepository.RemoveAsync(course, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> CodeExistsAsync(string code, string? excludeId, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await courseRepository.ExistsAsync(
            c => c.Code == normalized && (excludeId == null || c.Id != excludeId),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
    {
        return await courseRepository.ExistsAsync(c => c.Id == id, cancellationToken);
    }

    private static CourseResponse MapToResponse(Course course) =>
        new(
            course.Id,
            course.Name,
            course.Code,
            course.Description,
            course.WorkloadHours,
            course.SchoolUnitId,
            course.TargetAudience,
            course.Status,
            course.CreatedAt,
            course.UpdatedAt,
            course.CreatedBy,
            course.UpdatedBy);
}

