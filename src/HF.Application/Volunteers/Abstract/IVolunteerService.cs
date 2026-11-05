using HF.Application.Volunteers.Requests;
using HF.Application.Volunteers.Responses;

namespace HF.Application.Volunteers.Abstract;

/// <summary>
/// Defines application use-case operations for volunteers.
/// </summary>
public interface IVolunteerService
{
    /// <summary>Creates a new volunteer record.</summary>
    Task<VolunteerResponse> CreateAsync(CreateVolunteerRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a volunteer by identifier.</summary>
    Task<VolunteerResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists volunteers matching optional criteria.</summary>
    Task<IReadOnlyList<VolunteerResponse>> ListAsync(ListVolunteersRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing volunteer record.</summary>
    Task<VolunteerResponse?> UpdateAsync(UpdateVolunteerRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a volunteer record.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether a user exists and is not already associated with another volunteer.</summary>
    Task<bool> UserAvailableAsync(string userId, string? excludeId, CancellationToken cancellationToken);
}

