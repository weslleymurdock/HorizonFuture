using HF.Application.Collaborators.Responses;
using HF.Application.Common.Responses;
using HF.Domain.Collaborators;
using Mediator;

namespace HF.Application.Collaborators.Requests;

/// <summary>Command to create a new collaborator.</summary>
public sealed record CreateCollaboratorRequest(
    string UserId,
    string EmployeeNumber,
    string JobTitle,
    string Department,
    string? SchoolUnitId = null,
    ContractType ContractType = ContractType.PermanentClt,
    DateOnly? AdmissionDate = null,
    CollaboratorStatus Status = CollaboratorStatus.Active)
    : IRequest<ResponseBase<CollaboratorResponse>>;

/// <summary>Query to get a collaborator by identifier.</summary>
public sealed record GetCollaboratorByIdRequest(string Id)
    : IRequest<ResponseBase<CollaboratorResponse>>;

/// <summary>Query to list collaborators matching optional filters.</summary>
public sealed record ListCollaboratorsRequest(
    string? SchoolUnitId = null,
    CollaboratorStatus? Status = null,
    string? Department = null)
    : IRequest<ResponseBase<IReadOnlyList<CollaboratorResponse>>>;

/// <summary>Command to update an existing collaborator.</summary>
public sealed record UpdateCollaboratorRequest(
    string Id,
    string EmployeeNumber,
    string JobTitle,
    string Department,
    string? SchoolUnitId = null,
    ContractType ContractType = ContractType.PermanentClt,
    CollaboratorStatus Status = CollaboratorStatus.Active)
    : IRequest<ResponseBase<CollaboratorResponse>>;

/// <summary>Command to delete a collaborator by identifier.</summary>
public sealed record DeleteCollaboratorRequest(string Id)
    : IRequest<ResponseBase<bool>>;

