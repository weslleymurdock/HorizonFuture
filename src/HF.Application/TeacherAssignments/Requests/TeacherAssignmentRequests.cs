using HF.Application.Common.Responses;
using HF.Application.TeacherAssignments.Responses;
using HF.Domain.TeacherAssignments;
using Mediator;

namespace HF.Application.TeacherAssignments.Requests;

/// <summary>Command to create a new teacher assignment.</summary>
public sealed record CreateTeacherAssignmentRequest(
    string TeacherId,
    string SubjectId,
    int AcademicYear,
    int AcademicSemester,
    TeacherAssignmentRole Role = TeacherAssignmentRole.Primary)
    : IRequest<ResponseBase<TeacherAssignmentResponse>>;

/// <summary>Query to get a teacher assignment by identifier.</summary>
public sealed record GetTeacherAssignmentByIdRequest(string Id)
    : IRequest<ResponseBase<TeacherAssignmentResponse>>;

/// <summary>Query to list teacher assignments matching optional filters.</summary>
public sealed record ListTeacherAssignmentsRequest(
    string? TeacherId = null,
    string? SubjectId = null,
    int? AcademicYear = null,
    int? AcademicSemester = null)
    : IRequest<ResponseBase<IReadOnlyList<TeacherAssignmentResponse>>>;

/// <summary>Command to update an existing teacher assignment.</summary>
public sealed record UpdateTeacherAssignmentRequest(
    string Id,
    string TeacherId,
    string SubjectId,
    int AcademicYear,
    int AcademicSemester,
    TeacherAssignmentRole Role = TeacherAssignmentRole.Primary)
    : IRequest<ResponseBase<TeacherAssignmentResponse>>;

/// <summary>Command to delete a teacher assignment by identifier.</summary>
public sealed record DeleteTeacherAssignmentRequest(string Id)
    : IRequest<ResponseBase<bool>>;

