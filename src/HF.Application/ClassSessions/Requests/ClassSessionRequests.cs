using HF.Application.ClassSessions.Responses;
using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Common.Responses;
using Mediator;

namespace HF.Application.ClassSessions.Requests;

/// <summary>Command to create a new class session.</summary>
public sealed record CreateClassSessionRequest(
    string TeacherAssignmentId,
    DateOnly Date,
    TimeOnly? StartTime = null,
    int? DurationMinutes = null,
    string? Notes = null)
    : IRequest<ResponseBase<ClassSessionResponse>>, ITransactionalRequest;

/// <summary>Query to get a class session by identifier.</summary>
public sealed record GetClassSessionByIdRequest(string Id)
    : IRequest<ResponseBase<ClassSessionResponse>>;

/// <summary>Query to list class sessions matching optional filters.</summary>
public sealed record ListClassSessionsRequest(
    string? TeacherAssignmentId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null)
    : IRequest<ResponseBase<IReadOnlyList<ClassSessionResponse>>>;

/// <summary>Command to update an existing class session.</summary>
public sealed record UpdateClassSessionRequest(
    string Id,
    string TeacherAssignmentId,
    DateOnly Date,
    TimeOnly? StartTime = null,
    int? DurationMinutes = null,
    string? Notes = null)
    : IRequest<ResponseBase<ClassSessionResponse>>, ITransactionalRequest;

/// <summary>Command to delete a class session by identifier.</summary>
public sealed record DeleteClassSessionRequest(string Id)
    : IRequest<ResponseBase<bool>>, ITransactionalRequest;

