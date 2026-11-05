using HF.Application.Attendances.Responses;
using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Common.Responses;
using HF.Domain.Attendances;
using Mediator;

namespace HF.Application.Attendances.Requests;

/// <summary>Command to create a new attendance record.</summary>
public sealed record CreateAttendanceRequest(
    string ClassSessionId,
    string EnrollmentId,
    AttendanceStatus Status = AttendanceStatus.Present,
    string? Notes = null)
    : IRequest<ResponseBase<AttendanceResponse>>, ITransactionalRequest;

/// <summary>Query to get an attendance record by identifier.</summary>
public sealed record GetAttendanceByIdRequest(string Id)
    : IRequest<ResponseBase<AttendanceResponse>>;

/// <summary>Query to list attendance records matching optional filters.</summary>
public sealed record ListAttendancesRequest(
    string? ClassSessionId = null,
    string? EnrollmentId = null,
    AttendanceStatus? Status = null)
    : IRequest<ResponseBase<IReadOnlyList<AttendanceResponse>>>;

/// <summary>Command to update an existing attendance record.</summary>
public sealed record UpdateAttendanceRequest(
    string Id,
    string ClassSessionId,
    string EnrollmentId,
    AttendanceStatus Status = AttendanceStatus.Present,
    string? Notes = null)
    : IRequest<ResponseBase<AttendanceResponse>>, ITransactionalRequest;

/// <summary>Command to delete an attendance record by identifier.</summary>
public sealed record DeleteAttendanceRequest(string Id)
    : IRequest<ResponseBase<bool>>, ITransactionalRequest;

