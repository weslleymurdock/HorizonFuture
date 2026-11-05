using HF.Domain.Collaborators;

namespace HF.Application.Collaborators.Responses;

/// <summary>
/// Data contract representing a collaborator.
/// </summary>
public sealed record CollaboratorResponse(
    string Id,
    string UserId,
    string EmployeeNumber,
    string JobTitle,
    string Department,
    string? SchoolUnitId,
    ContractType ContractType,
    DateOnly AdmissionDate,
    CollaboratorStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

