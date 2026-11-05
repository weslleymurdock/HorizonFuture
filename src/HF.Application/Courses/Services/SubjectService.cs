using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Courses.Abstract;
using HF.Application.Courses.Requests;
using HF.Application.Courses.Responses;
using HF.Domain.Courses;
using HF.Domain.Enrollments;
using HF.Domain.TeacherAssignments;

namespace HF.Application.Courses.Services;

/// <summary>
/// Implements application use cases and business rules for subjects.
/// </summary>
public sealed class SubjectService(
    IRepository<Subject> subjectRepository,
    IRepository<Course> courseRepository,
    IRepository<TeacherAssignment> teacherAssignmentRepository,
    IRepository<Enrollment> enrollmentRepository)
    : ISubjectService
{
    /// <inheritdoc />
    public async Task<SubjectResponse> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.CourseId))
        {
            var courseExists = await courseRepository.ExistsAsync(c => c.Id == request.CourseId, cancellationToken);
            if (!courseExists)
            {
                throw new KeyNotFoundException("Referenced course does not exist.");
            }
        }

        var subject = new Subject
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Description = request.Description?.Trim(),
            WorkloadHours = request.WorkloadHours,
            CourseId = request.CourseId?.Trim(),
            SchoolUnitId = request.SchoolUnitId?.Trim(),
            Status = request.Status
        };

        await subjectRepository.AddAsync(subject, cancellationToken);
        return MapToResponse(subject);
    }

    /// <inheritdoc />
    public async Task<SubjectResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var subject = await subjectRepository.GetByIdAsync(id, [], cancellationToken);
        return subject is null ? null : MapToResponse(subject);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SubjectResponse>> ListAsync(ListSubjectsRequest request, CancellationToken cancellationToken)
    {
        var subjects = await subjectRepository.ListAsync(
            s => (request.CourseId == null || s.CourseId == request.CourseId) &&
                 (request.SchoolUnitId == null || s.SchoolUnitId == request.SchoolUnitId) &&
                 (request.Status == null || s.Status == request.Status),
            [],
            cancellationToken);

        return subjects.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<SubjectResponse?> UpdateAsync(UpdateSubjectRequest request, CancellationToken cancellationToken)
    {
        var subject = await subjectRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (subject is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.CourseId))
        {
            var courseExists = await courseRepository.ExistsAsync(c => c.Id == request.CourseId, cancellationToken);
            if (!courseExists)
            {
                throw new KeyNotFoundException("Referenced course does not exist.");
            }
        }

        subject.Name = request.Name.Trim();
        subject.Code = request.Code.Trim().ToUpperInvariant();
        subject.Description = request.Description?.Trim();
        subject.WorkloadHours = request.WorkloadHours;
        subject.CourseId = request.CourseId?.Trim();
        subject.SchoolUnitId = request.SchoolUnitId?.Trim();
        subject.Status = request.Status;
        subject.UpdatedAt = DateTime.UtcNow;

        await subjectRepository.UpdateAsync(subject, cancellationToken);
        return MapToResponse(subject);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var subject = await subjectRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (subject is null)
        {
            return (false, "Subject not found.");
        }

        bool hasAssignments = await teacherAssignmentRepository.ExistsAsync(ta => ta.SubjectId == id, cancellationToken);
        if (hasAssignments)
        {
            return (false, "Cannot delete subject with existing teacher assignment history.");
        }

        bool hasEnrollments = await enrollmentRepository.ExistsAsync(e => e.SubjectId == id, cancellationToken);
        if (hasEnrollments)
        {
            return (false, "Cannot delete subject with existing student enrollment history.");
        }

        await subjectRepository.RemoveAsync(subject, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> CodeExistsAsync(string code, string? excludeId, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await subjectRepository.ExistsAsync(
            s => s.Code == normalized && (excludeId == null || s.Id != excludeId),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
    {
        return await subjectRepository.ExistsAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CourseExistsAsync(string courseId, CancellationToken cancellationToken)
    {
        return await courseRepository.ExistsAsync(c => c.Id == courseId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasAcademicHistoryAsync(string subjectId, CancellationToken cancellationToken)
    {
        return await teacherAssignmentRepository.ExistsAsync(ta => ta.SubjectId == subjectId, cancellationToken)
            || await enrollmentRepository.ExistsAsync(e => e.SubjectId == subjectId, cancellationToken);
    }

    private static SubjectResponse MapToResponse(Subject subject) =>
        new(
            subject.Id,
            subject.Name,
            subject.Code,
            subject.Description,
            subject.WorkloadHours,
            subject.CourseId,
            subject.SchoolUnitId,
            subject.Status,
            subject.CreatedAt,
            subject.UpdatedAt,
            subject.CreatedBy,
            subject.UpdatedBy);
}
