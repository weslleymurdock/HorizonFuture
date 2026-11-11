using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Common.Responses;
using HF.Application.Students.Responses;
using HF.Domain.Students;
using Mediator;

namespace HF.Application.Students.Requests;

/// <summary>Command to create a new student.</summary>
public sealed record CreateStudentRequest(
    string UserId,
    string RegistrationNumber,
    DateOnly DateOfBirth,
    string? GuardianName = null,
    string? GuardianRelationship = null,
    string? GuardianPhone = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null,
    string? SchoolUnitId = null,
    StudentStatus Status = StudentStatus.Active,
    string? SpecialNeeds = null,
    string? MedicalNotes = null,
    DateOnly? EnrollmentDate = null,
    string? PersonId = null)
    : IRequest<ResponseBase<StudentResponse>>, ITransactionalRequest;

/// <summary>Query to get a student by identifier.</summary>
public sealed record GetStudentByIdRequest(string Id)
    : IRequest<ResponseBase<StudentResponse>>;

/// <summary>Query to list students matching optional filters.</summary>
public sealed record ListStudentsRequest(
    string? SchoolUnitId = null,
    StudentStatus? Status = null)
    : IRequest<ResponseBase<IReadOnlyList<StudentResponse>>>;

/// <summary>Command to update an existing student.</summary>
public sealed record UpdateStudentRequest(
    string Id,
    string RegistrationNumber,
    DateOnly DateOfBirth,
    string? GuardianName = null,
    string? GuardianRelationship = null,
    string? GuardianPhone = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null,
    string? SchoolUnitId = null,
    StudentStatus Status = StudentStatus.Active,
    string? SpecialNeeds = null,
    string? MedicalNotes = null,
    string? PersonId = null)
    : IRequest<ResponseBase<StudentResponse>>, ITransactionalRequest;

/// <summary>Command to delete a student by identifier.</summary>
public sealed record DeleteStudentRequest(string Id)
    : IRequest<ResponseBase<bool>>, ITransactionalRequest;
