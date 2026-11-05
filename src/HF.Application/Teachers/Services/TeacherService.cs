using HF.Application.Auth.Abstract;
using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Teachers.Abstract;
using HF.Application.Teachers.Requests;
using HF.Application.Teachers.Responses;
using HF.Domain.TeacherAssignments;
using HF.Domain.Teachers;

namespace HF.Application.Teachers.Services;

/// <summary>
/// Implements application use cases and business rules for teachers.
/// </summary>
public sealed class TeacherService(
    IRepository<Teacher> teacherRepository,
    IRepository<TeacherAssignment> assignmentRepository,
    IIdentityService identityService)
    : ITeacherService
{
    /// <inheritdoc />
    public async Task<TeacherResponse> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken)
    {
        var teacher = new Teacher
        {
            UserId = request.UserId,
            EmployeeNumber = request.EmployeeNumber.Trim(),
            Specialty = request.Specialty.Trim(),
            AcademicDegree = request.AcademicDegree?.Trim(),
            Bio = request.Bio?.Trim(),
            SchoolUnitId = request.SchoolUnitId?.Trim(),
            HireDate = request.HireDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = request.Status
        };

        await teacherRepository.AddAsync(teacher, cancellationToken);
        return MapToResponse(teacher);
    }

    /// <inheritdoc />
    public async Task<TeacherResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var teacher = await teacherRepository.GetByIdAsync(id, [], cancellationToken);
        return teacher is null ? null : MapToResponse(teacher);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TeacherResponse>> ListAsync(ListTeachersRequest request, CancellationToken cancellationToken)
    {
        var teachers = await teacherRepository.ListAsync(
            t => (request.SchoolUnitId == null || t.SchoolUnitId == request.SchoolUnitId) &&
                 (request.Status == null || t.Status == request.Status) &&
                 (request.Specialty == null || t.Specialty == request.Specialty),
            [],
            cancellationToken);

        return teachers.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<TeacherResponse?> UpdateAsync(UpdateTeacherRequest request, CancellationToken cancellationToken)
    {
        var teacher = await teacherRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (teacher is null)
        {
            return null;
        }

        teacher.EmployeeNumber = request.EmployeeNumber.Trim();
        teacher.Specialty = request.Specialty.Trim();
        teacher.AcademicDegree = request.AcademicDegree?.Trim();
        teacher.Bio = request.Bio?.Trim();
        teacher.SchoolUnitId = request.SchoolUnitId?.Trim();
        teacher.Status = request.Status;
        teacher.UpdatedAt = DateTime.UtcNow;

        await teacherRepository.UpdateAsync(teacher, cancellationToken);
        return MapToResponse(teacher);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var teacher = await teacherRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (teacher is null)
        {
            return (false, "Teacher not found.");
        }

        bool hasAssignments = await assignmentRepository.ExistsAsync(a => a.TeacherId == id, cancellationToken);
        if (hasAssignments)
        {
            return (false, "Cannot delete teacher with existing teaching assignment history.");
        }

        await teacherRepository.RemoveAsync(teacher, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> EmployeeNumberExistsAsync(string employeeNumber, string? excludeId, CancellationToken cancellationToken)
    {
        var trimmed = employeeNumber.Trim();
        return await teacherRepository.ExistsAsync(
            t => t.EmployeeNumber == trimmed && (excludeId == null || t.Id != excludeId),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> UserAvailableAsync(string userId, string? excludeId, CancellationToken cancellationToken)
    {
        var user = await identityService.GetInfoAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        bool alreadyAssigned = await teacherRepository.ExistsAsync(
            t => t.UserId == userId && (excludeId == null || t.Id != excludeId),
            cancellationToken);

        return !alreadyAssigned;
    }

    /// <inheritdoc />
    public async Task<bool> HasAssignmentsAsync(string teacherId, CancellationToken cancellationToken)
    {
        return await assignmentRepository.ExistsAsync(a => a.TeacherId == teacherId, cancellationToken);
    }

    private static TeacherResponse MapToResponse(Teacher teacher) =>
        new(
            teacher.Id,
            teacher.UserId,
            teacher.EmployeeNumber,
            teacher.Specialty,
            teacher.AcademicDegree,
            teacher.Bio,
            teacher.SchoolUnitId,
            teacher.HireDate,
            teacher.Status,
            teacher.CreatedAt,
            teacher.UpdatedAt,
            teacher.CreatedBy,
            teacher.UpdatedBy);
}

