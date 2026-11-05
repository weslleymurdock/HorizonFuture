using HF.Application.AssetsLiabilities.Requests;
using HF.Application.AssetsLiabilities.Responses;

namespace HF.Application.AssetsLiabilities.Abstract;

/// <summary>
/// Defines application use-case operations for physical and patrimonial assets.
/// </summary>
public interface IAssetService
{
    /// <summary>Creates a new asset record.</summary>
    Task<AssetResponse> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken);

    /// <summary>Gets an asset by identifier.</summary>
    Task<AssetResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Lists assets matching optional criteria.</summary>
    Task<IReadOnlyList<AssetResponse>> ListAsync(ListAssetsRequest request, CancellationToken cancellationToken);

    /// <summary>Updates an existing asset record.</summary>
    Task<AssetResponse?> UpdateAsync(UpdateAssetRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes an asset record.</summary>
    Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Checks whether an asset tag is already in use.</summary>
    Task<bool> AssetTagExistsAsync(string assetTag, string? excludeId, CancellationToken cancellationToken);
}

