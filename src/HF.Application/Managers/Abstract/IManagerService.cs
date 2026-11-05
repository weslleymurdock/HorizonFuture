using HF.Application.Managers.Requests;
using HF.Application.Managers.Responses;

namespace HF.Application.Managers.Abstract;

/// <summary>
/// Defines application use-case operations for managers.
/// </summary>
public interface IManagerService
{
    /// <summary>Creates a new manager record.</summary>
    Task<ManagerResponse> CreateAsync(CreateManagerRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a manager by identifier.</summary>
    Task<ManagerResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists managers matching optional criteria.</summary>
    Task<IReadOnlyList<ManagerResponse>> ListAsync(ListManagersRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing manager record.</summary>
    Task<ManagerResponse?> UpdateAsync(UpdateManagerRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a manager record.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether an employee number is already in use.</summary>
    Task<bool> EmployeeNumberExistsAsync(string employeeNumber, string? excludeId, CancellationToken cancellationToken);

    /// <summary>Checks whether a user exists and is not already associated with another manager.</summary>
    Task<bool> UserAvailableAsync(string userId, string? excludeId, CancellationToken cancellationToken);
}

