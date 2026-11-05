using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Common.Responses;
using HF.Application.Teachers.Responses;
using HF.Domain.Teachers;
using Mediator;

namespace HF.Application.Teachers.Requests;

/// <summary>Command to create a new teacher.</summary>
public sealed record CreateTeacherRequest(
    string UserId,
    string EmployeeNumber,
    string Specialty,
    string? AcademicDegree = null,
    string? Bio = null,
    string? SchoolUnitId = null,
    DateOnly? HireDate = null,
    TeacherStatus Status = TeacherStatus.Active)
    : IRequest<ResponseBase<TeacherResponse>>, ITransactionalRequest;

/// <summary>Query to get a teacher by identifier.</summary>
public sealed record GetTeacherByIdRequest(string Id)
    : IRequest<ResponseBase<TeacherResponse>>;

/// <summary>Query to list teachers matching optional filters.</summary>
public sealed record ListTeachersRequest(
    string? SchoolUnitId = null,
    TeacherStatus? Status = null,
    string? Specialty = null)
    : IRequest<ResponseBase<IReadOnlyList<TeacherResponse>>>;

/// <summary>Command to update an existing teacher.</summary>
public sealed record UpdateTeacherRequest(
    string Id,
    string EmployeeNumber,
    string Specialty,
    string? AcademicDegree = null,
    string? Bio = null,
    string? SchoolUnitId = null,
    TeacherStatus Status = TeacherStatus.Active)
    : IRequest<ResponseBase<TeacherResponse>>, ITransactionalRequest;

/// <summary>Command to delete a teacher by identifier.</summary>
public sealed record DeleteTeacherRequest(string Id)
    : IRequest<ResponseBase<bool>>, ITransactionalRequest;

