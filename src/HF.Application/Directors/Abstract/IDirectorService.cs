using HF.Application.Directors.Requests;
using HF.Application.Directors.Responses;

namespace HF.Application.Directors.Abstract;

/// <summary>
/// Defines application use-case operations for directors.
/// </summary>
public interface IDirectorService
{
    /// <summary>Creates a new director record.</summary>
    Task<DirectorResponse> CreateAsync(CreateDirectorRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a director by identifier.</summary>
    Task<DirectorResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists directors matching optional criteria.</summary>
    Task<IReadOnlyList<DirectorResponse>> ListAsync(ListDirectorsRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing director record.</summary>
    Task<DirectorResponse?> UpdateAsync(UpdateDirectorRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a director record.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether a user exists and is not already associated with another director.</summary>
    Task<bool> UserAvailableAsync(string userId, string? excludeId, CancellationToken cancellationToken);
}

