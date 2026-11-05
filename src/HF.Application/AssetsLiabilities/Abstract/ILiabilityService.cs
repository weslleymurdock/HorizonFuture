using HF.Application.AssetsLiabilities.Requests;
using HF.Application.AssetsLiabilities.Responses;

namespace HF.Application.AssetsLiabilities.Abstract;

/// <summary>
/// Defines application use-case operations for liabilities and financial obligations.
/// </summary>
public interface ILiabilityService
{
    /// <summary>Creates a new liability record.</summary>
    Task<LiabilityResponse> CreateAsync(CreateLiabilityRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a liability by identifier.</summary>
    Task<LiabilityResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists liabilities matching optional criteria.</summary>
    Task<IReadOnlyList<LiabilityResponse>> ListAsync(ListLiabilitiesRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing liability record.</summary>
    Task<LiabilityResponse?> UpdateAsync(UpdateLiabilityRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a liability record.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);
}

