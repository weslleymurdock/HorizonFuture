using HF.Application.Attendances.Abstract;
using HF.Application.Attendances.Requests;
using HF.Application.Attendances.Responses;
using HF.Application.Common.Abstractions.Repositories;
using HF.Domain.Attendances;
using HF.Domain.ClassSessions;
using HF.Domain.Enrollments;
using HF.Domain.TeacherAssignments;

namespace HF.Application.Attendances.Services;

/// <summary>
/// Implements application use cases and business rules for attendance records.
/// </summary>
public sealed class AttendanceService(
    IRepository<Attendance> attendanceRepository,
    IRepository<ClassSession> classSessionRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<TeacherAssignment> assignmentRepository)
    : IAttendanceService
{
    /// <inheritdoc />
    public async Task<AttendanceResponse> CreateAsync(CreateAttendanceRequest request, CancellationToken cancellationToken)
    {
        var (isCompatible, reason) = await ValidateAcademicCompatibilityAsync(
            request.ClassSessionId,
            request.EnrollmentId,
            cancellationToken);

        if (!isCompatible)
        {
            throw new InvalidOperationException(reason ?? "The attendance record violates academic relationship invariants.");
        }

        var attendance = new Attendance
        {
            ClassSessionId = request.ClassSessionId.Trim(),
            EnrollmentId = request.EnrollmentId.Trim(),
            Status = request.Status,
            Notes = request.Notes?.Trim()
        };

        await attendanceRepository.AddAsync(attendance, cancellationToken);
        return MapToResponse(attendance);
    }

    /// <inheritdoc />
    public async Task<AttendanceResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var attendance = await attendanceRepository.GetByIdAsync(id, [], cancellationToken);
        return attendance is null ? null : MapToResponse(attendance);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AttendanceResponse>> ListAsync(ListAttendancesRequest request, CancellationToken cancellationToken)
    {
        var attendances = await attendanceRepository.ListAsync(
            a => (request.ClassSessionId == null || a.ClassSessionId == request.ClassSessionId) &&
                 (request.EnrollmentId == null || a.EnrollmentId == request.EnrollmentId) &&
                 (request.Status == null || a.Status == request.Status),
            [],
            cancellationToken);

        return attendances.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<AttendanceResponse?> UpdateAsync(UpdateAttendanceRequest request, CancellationToken cancellationToken)
    {
        var attendance = await attendanceRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (attendance is null)
        {
            return null;
        }

        var (isCompatible, reason) = await ValidateAcademicCompatibilityAsync(
            request.ClassSessionId,
            request.EnrollmentId,
            cancellationToken);

        if (!isCompatible)
        {
            throw new InvalidOperationException(reason ?? "The attendance record violates academic relationship invariants.");
        }

        attendance.ClassSessionId = request.ClassSessionId.Trim();
        attendance.EnrollmentId = request.EnrollmentId.Trim();
        attendance.Status = request.Status;
        attendance.Notes = request.Notes?.Trim();
        attendance.UpdatedAt = DateTime.UtcNow;

        await attendanceRepository.UpdateAsync(attendance, cancellationToken);
        return MapToResponse(attendance);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var attendance = await attendanceRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (attendance is null)
        {
            return (false, "Attendance record not found.");
        }

        await attendanceRepository.RemoveAsync(attendance, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> AttendanceExistsAsync(string classSessionId, string enrollmentId, string? excludeId, CancellationToken cancellationToken)
    {
        return await attendanceRepository.ExistsAsync(
            a => a.ClassSessionId == classSessionId &&
                 a.EnrollmentId == enrollmentId &&
                 (excludeId == null || a.Id != excludeId),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(bool IsCompatible, string? Reason)> ValidateAcademicCompatibilityAsync(
        string classSessionId,
        string enrollmentId,
        CancellationToken cancellationToken)
    {
        var session = await classSessionRepository.GetByIdAsync(classSessionId, [], cancellationToken);
        if (session is null)
        {
            return (false, "Referenced class session does not exist.");
        }

        var assignment = await assignmentRepository.GetByIdAsync(session.TeacherAssignmentId, [], cancellationToken);
        if (assignment is null)
        {
            return (false, "Teacher assignment associated with class session does not exist.");
        }

        var enrollment = await enrollmentRepository.GetByIdAsync(enrollmentId, [], cancellationToken);
        if (enrollment is null)
        {
            return (false, "Referenced enrollment does not exist.");
        }

        if (enrollment.SubjectId != assignment.SubjectId)
        {
            return (false, $"Enrollment subject ({enrollment.SubjectId}) does not match class session subject ({assignment.SubjectId}).");
        }

        if (enrollment.AcademicYear != assignment.AcademicYear || enrollment.AcademicSemester != assignment.AcademicSemester)
        {
            return (false, $"Enrollment academic period ({enrollment.AcademicYear}/{enrollment.AcademicSemester}) does not match class session academic period ({assignment.AcademicYear}/{assignment.AcademicSemester}).");
        }

        return (true, null);
    }

    private static AttendanceResponse MapToResponse(Attendance attendance) =>
        new(
            attendance.Id,
            attendance.ClassSessionId,
            attendance.EnrollmentId,
            attendance.Status,
            attendance.Notes,
            attendance.CreatedAt,
            attendance.UpdatedAt,
            attendance.CreatedBy,
            attendance.UpdatedBy);
}

