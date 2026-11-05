using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Common.Responses;
using HF.Application.Courses.Responses;
using HF.Domain.Courses;
using Mediator;

namespace HF.Application.Courses.Requests;

/// <summary>Command to create a new course.</summary>
public sealed record CreateCourseRequest(
    string Name,
    string Code,
    int WorkloadHours,
    string? Description = null,
    string? SchoolUnitId = null,
    string? TargetAudience = null,
    CourseStatus Status = CourseStatus.Active)
    : IRequest<ResponseBase<CourseResponse>>, ITransactionalRequest;

/// <summary>Query to get a course by identifier.</summary>
public sealed record GetCourseByIdRequest(string Id)
    : IRequest<ResponseBase<CourseResponse>>;

/// <summary>Query to list courses matching optional filters.</summary>
public sealed record ListCoursesRequest(
    string? SchoolUnitId = null,
    CourseStatus? Status = null)
    : IRequest<ResponseBase<IReadOnlyList<CourseResponse>>>;

/// <summary>Command to update an existing course.</summary>
public sealed record UpdateCourseRequest(
    string Id,
    string Name,
    string Code,
    int WorkloadHours,
    string? Description = null,
    string? SchoolUnitId = null,
    string? TargetAudience = null,
    CourseStatus Status = CourseStatus.Active)
    : IRequest<ResponseBase<CourseResponse>>, ITransactionalRequest;

/// <summary>Command to delete a course by identifier.</summary>
public sealed record DeleteCourseRequest(string Id)
    : IRequest<ResponseBase<bool>>, ITransactionalRequest;

