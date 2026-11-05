using HF.Application.Common.Responses;
using HF.Application.Courses.Responses;
using HF.Domain.Courses;
using Mediator;

namespace HF.Application.Courses.Requests;

/// <summary>Command to create a new subject.</summary>
public sealed record CreateSubjectRequest(
    string Name,
    string Code,
    int WorkloadHours,
    string? Description = null,
    string? CourseId = null,
    string? SchoolUnitId = null,
    SubjectStatus Status = SubjectStatus.Active)
    : IRequest<ResponseBase<SubjectResponse>>;

/// <summary>Query to get a subject by identifier.</summary>
public sealed record GetSubjectByIdRequest(string Id)
    : IRequest<ResponseBase<SubjectResponse>>;

/// <summary>Query to list subjects matching optional filters.</summary>
public sealed record ListSubjectsRequest(
    string? CourseId = null,
    string? SchoolUnitId = null,
    SubjectStatus? Status = null)
    : IRequest<ResponseBase<IReadOnlyList<SubjectResponse>>>;

/// <summary>Command to update an existing subject.</summary>
public sealed record UpdateSubjectRequest(
    string Id,
    string Name,
    string Code,
    int WorkloadHours,
    string? Description = null,
    string? CourseId = null,
    string? SchoolUnitId = null,
    SubjectStatus Status = SubjectStatus.Active)
    : IRequest<ResponseBase<SubjectResponse>>;

/// <summary>Command to delete a subject by identifier.</summary>
public sealed record DeleteSubjectRequest(string Id)
    : IRequest<ResponseBase<bool>>;

