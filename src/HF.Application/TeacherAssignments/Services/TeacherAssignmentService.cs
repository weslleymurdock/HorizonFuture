using HF.Application.Common.Abstractions.Repositories;
using HF.Application.TeacherAssignments.Abstract;
using HF.Application.TeacherAssignments.Requests;
using HF.Application.TeacherAssignments.Responses;
using HF.Domain.ClassSessions;
using HF.Domain.Courses;
using HF.Domain.TeacherAssignments;
using HF.Domain.Teachers;

namespace HF.Application.TeacherAssignments.Services;

/// <summary>
/// Implements application use cases and business rules for teacher assignments.
/// </summary>
public sealed class TeacherAssignmentService(
    IRepository<TeacherAssignment> assignmentRepository,
    IRepository<Teacher> teacherRepository,
    IRepository<Subject> subjectRepository,
    IRepository<ClassSession> classSessionRepository)
    : ITeacherAssignmentService
{
    /// <inheritdoc />
    public async Task<TeacherAssignmentResponse> CreateAsync(CreateTeacherAssignmentRequest request, CancellationToken cancellationToken)
    {
        var teacherExists = await teacherRepository.ExistsAsync(t => t.Id == request.TeacherId, cancellationToken);
        if (!teacherExists)
        {
            throw new KeyNotFoundException("Referenced teacher does not exist.");
        }

        var subjectExists = await subjectRepository.ExistsAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            throw new KeyNotFoundException("Referenced subject does not exist.");
        }

        var assignment = new TeacherAssignment
        {
            TeacherId = request.TeacherId.Trim(),
            SubjectId = request.SubjectId.Trim(),
            AcademicYear = request.AcademicYear,
            AcademicSemester = request.AcademicSemester,
            Role = request.Role
        };

        await assignmentRepository.AddAsync(assignment, cancellationToken);
        return MapToResponse(assignment);
    }

    /// <inheritdoc />
    public async Task<TeacherAssignmentResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetByIdAsync(id, [], cancellationToken);
        return assignment is null ? null : MapToResponse(assignment);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TeacherAssignmentResponse>> ListAsync(ListTeacherAssignmentsRequest request, CancellationToken cancellationToken)
    {
        var assignments = await assignmentRepository.ListAsync(
            ta => (request.TeacherId == null || ta.TeacherId == request.TeacherId) &&
                  (request.SubjectId == null || ta.SubjectId == request.SubjectId) &&
                  (request.AcademicYear == null || ta.AcademicYear == request.AcademicYear) &&
                  (request.AcademicSemester == null || ta.AcademicSemester == request.AcademicSemester),
            [],
            cancellationToken);

        return assignments.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<TeacherAssignmentResponse?> UpdateAsync(UpdateTeacherAssignmentRequest request, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (assignment is null)
        {
            return null;
        }

        var teacherExists = await teacherRepository.ExistsAsync(t => t.Id == request.TeacherId, cancellationToken);
        if (!teacherExists)
        {
            throw new KeyNotFoundException("Referenced teacher does not exist.");
        }

        var subjectExists = await subjectRepository.ExistsAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            throw new KeyNotFoundException("Referenced subject does not exist.");
        }

        assignment.TeacherId = request.TeacherId.Trim();
        assignment.SubjectId = request.SubjectId.Trim();
        assignment.AcademicYear = request.AcademicYear;
        assignment.AcademicSemester = request.AcademicSemester;
        assignment.Role = request.Role;
        assignment.UpdatedAt = DateTime.UtcNow;

        await assignmentRepository.UpdateAsync(assignment, cancellationToken);
        return MapToResponse(assignment);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (assignment is null)
        {
            return (false, "Teacher assignment not found.");
        }

        bool hasSessions = await classSessionRepository.ExistsAsync(cs => cs.TeacherAssignmentId == id, cancellationToken);
        if (hasSessions)
        {
            return (false, "Cannot delete teacher assignment with scheduled or conducted class sessions.");
        }

        await assignmentRepository.RemoveAsync(assignment, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> AssignmentExistsAsync(string teacherId, string subjectId, int academicYear, int academicSemester, string? excludeId, CancellationToken cancellationToken)
    {
        return await assignmentRepository.ExistsAsync(
            ta => ta.TeacherId == teacherId &&
                  ta.SubjectId == subjectId &&
                  ta.AcademicYear == academicYear &&
                  ta.AcademicSemester == academicSemester &&
                  (excludeId == null || ta.Id != excludeId),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
    {
        return await assignmentRepository.ExistsAsync(ta => ta.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasClassSessionsAsync(string assignmentId, CancellationToken cancellationToken)
    {
        return await classSessionRepository.ExistsAsync(cs => cs.TeacherAssignmentId == assignmentId, cancellationToken);
    }

    private static TeacherAssignmentResponse MapToResponse(TeacherAssignment assignment) =>
        new(
            assignment.Id,
            assignment.TeacherId,
            assignment.SubjectId,
            assignment.AcademicYear,
            assignment.AcademicSemester,
            assignment.Role,
            assignment.CreatedAt,
            assignment.UpdatedAt,
            assignment.CreatedBy,
            assignment.UpdatedBy);
}

