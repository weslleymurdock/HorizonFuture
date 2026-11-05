using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Common.Responses;
using HF.Application.Enrollments.Responses;
using HF.Domain.Enrollments;
using Mediator;

namespace HF.Application.Enrollments.Requests;

/// <summary>Command to create a new enrollment.</summary>
public sealed record CreateEnrollmentRequest(
    string StudentId,
    string SubjectId,
    int AcademicYear,
    int AcademicSemester,
    DateOnly? EnrollmentDate = null,
    EnrollmentStatus Status = EnrollmentStatus.Active,
    decimal? FinalGrade = null)
    : IRequest<ResponseBase<EnrollmentResponse>>, ITransactionalRequest;

/// <summary>Query to get an enrollment by identifier.</summary>
public sealed record GetEnrollmentByIdRequest(string Id)
    : IRequest<ResponseBase<EnrollmentResponse>>;

/// <summary>Query to list enrollments matching optional filters.</summary>
public sealed record ListEnrollmentsRequest(
    string? StudentId = null,
    string? SubjectId = null,
    int? AcademicYear = null,
    int? AcademicSemester = null,
    EnrollmentStatus? Status = null)
    : IRequest<ResponseBase<IReadOnlyList<EnrollmentResponse>>>;

/// <summary>Command to update an existing enrollment.</summary>
public sealed record UpdateEnrollmentRequest(
    string Id,
    string StudentId,
    string SubjectId,
    int AcademicYear,
    int AcademicSemester,
    DateOnly EnrollmentDate,
    EnrollmentStatus Status = EnrollmentStatus.Active,
    decimal? FinalGrade = null)
    : IRequest<ResponseBase<EnrollmentResponse>>, ITransactionalRequest;

/// <summary>Command to delete an enrollment by identifier.</summary>
public sealed record DeleteEnrollmentRequest(string Id)
    : IRequest<ResponseBase<bool>>, ITransactionalRequest;

