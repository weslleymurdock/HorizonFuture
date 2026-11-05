using HF.Application.ClassSessions.Abstract;
using HF.Application.ClassSessions.Requests;
using HF.Application.ClassSessions.Responses;
using HF.Application.Common.Abstractions.Repositories;
using HF.Domain.Attendances;
using HF.Domain.ClassSessions;
using HF.Domain.TeacherAssignments;

namespace HF.Application.ClassSessions.Services;

/// <summary>
/// Implements application use cases and business rules for class sessions.
/// </summary>
public sealed class ClassSessionService(
    IRepository<ClassSession> classSessionRepository,
    IRepository<TeacherAssignment> assignmentRepository,
    IRepository<Attendance> attendanceRepository)
    : IClassSessionService
{
    /// <inheritdoc />
    public async Task<ClassSessionResponse> CreateAsync(CreateClassSessionRequest request, CancellationToken cancellationToken)
    {
        var assignmentExists = await assignmentRepository.ExistsAsync(ta => ta.Id == request.TeacherAssignmentId, cancellationToken);
        if (!assignmentExists)
        {
            throw new KeyNotFoundException("Referenced teacher assignment does not exist.");
        }

        var session = new ClassSession
        {
            TeacherAssignmentId = request.TeacherAssignmentId.Trim(),
            Date = request.Date,
            StartTime = request.StartTime,
            DurationMinutes = request.DurationMinutes,
            Notes = request.Notes?.Trim()
        };

        await classSessionRepository.AddAsync(session, cancellationToken);
        return MapToResponse(session);
    }

    /// <inheritdoc />
    public async Task<ClassSessionResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var session = await classSessionRepository.GetByIdAsync(id, [], cancellationToken);
        return session is null ? null : MapToResponse(session);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ClassSessionResponse>> ListAsync(ListClassSessionsRequest request, CancellationToken cancellationToken)
    {
        var sessions = await classSessionRepository.ListAsync(
            cs => (request.TeacherAssignmentId == null || cs.TeacherAssignmentId == request.TeacherAssignmentId) &&
                  (request.FromDate == null || cs.Date >= request.FromDate) &&
                  (request.ToDate == null || cs.Date <= request.ToDate),
            [],
            cancellationToken);

        return sessions.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<ClassSessionResponse?> UpdateAsync(UpdateClassSessionRequest request, CancellationToken cancellationToken)
    {
        var session = await classSessionRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (session is null)
        {
            return null;
        }

        var assignmentExists = await assignmentRepository.ExistsAsync(ta => ta.Id == request.TeacherAssignmentId, cancellationToken);
        if (!assignmentExists)
        {
            throw new KeyNotFoundException("Referenced teacher assignment does not exist.");
        }

        session.TeacherAssignmentId = request.TeacherAssignmentId.Trim();
        session.Date = request.Date;
        session.StartTime = request.StartTime;
        session.DurationMinutes = request.DurationMinutes;
        session.Notes = request.Notes?.Trim();
        session.UpdatedAt = DateTime.UtcNow;

        await classSessionRepository.UpdateAsync(session, cancellationToken);
        return MapToResponse(session);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var session = await classSessionRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (session is null)
        {
            return (false, "Class session not found.");
        }

        bool hasAttendances = await attendanceRepository.ExistsAsync(a => a.ClassSessionId == id, cancellationToken);
        if (hasAttendances)
        {
            return (false, "Cannot delete class session with associated student attendance records.");
        }

        await classSessionRepository.RemoveAsync(session, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
    {
        return await classSessionRepository.ExistsAsync(cs => cs.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasAttendancesAsync(string sessionId, CancellationToken cancellationToken)
    {
        return await attendanceRepository.ExistsAsync(a => a.ClassSessionId == sessionId, cancellationToken);
    }

    private static ClassSessionResponse MapToResponse(ClassSession session) =>
        new(
            session.Id,
            session.TeacherAssignmentId,
            session.Date,
            session.StartTime,
            session.DurationMinutes,
            session.Notes,
            session.CreatedAt,
            session.UpdatedAt,
            session.CreatedBy,
            session.UpdatedBy);
}

