using HF.Application.Collaborators.Requests;
using HF.Application.Collaborators.Responses;

namespace HF.Application.Collaborators.Abstract;

/// <summary>
/// Defines application use-case operations for collaborators.
/// </summary>
public interface ICollaboratorService
{
    /// <summary>Creates a new collaborator record.</summary>
    Task<CollaboratorResponse> CreateAsync(CreateCollaboratorRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a collaborator by identifier.</summary>
    Task<CollaboratorResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists collaborators matching optional criteria.</summary>
    Task<IReadOnlyList<CollaboratorResponse>> ListAsync(ListCollaboratorsRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing collaborator record.</summary>
    Task<CollaboratorResponse?> UpdateAsync(UpdateCollaboratorRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a collaborator record.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether an employee number is already in use.</summary>
    Task<bool> EmployeeNumberExistsAsync(string employeeNumber, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Checks whether a user exists and is not already associated with another collaborator.</summary>
    Task<bool> UserAvailableAsync(string userId, string? excludeId, CancellationToken cancellationToken);
}

