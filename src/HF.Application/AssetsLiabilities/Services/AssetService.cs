using HF.Application.AssetsLiabilities.Abstract;
using HF.Application.AssetsLiabilities.Requests;
using HF.Application.AssetsLiabilities.Responses;
using HF.Application.Common.Abstractions.Repositories;
using HF.Domain.AssetsLiabilities;

namespace HF.Application.AssetsLiabilities.Services;

/// <summary>
/// Implements application use cases and business rules for assets.
/// </summary>
public sealed class AssetService(IRepository<Asset> assetRepository) : IAssetService
{
    /// <inheritdoc />
    public async Task<AssetResponse> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken)
    {
        var asset = new Asset
        {
            Name = request.Name.Trim(),
            AssetTag = request.AssetTag.Trim(),
            Category = request.Category,
            Condition = request.Condition,
            AcquisitionValue = request.AcquisitionValue,
            AcquisitionDate = request.AcquisitionDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            SchoolUnitId = request.SchoolUnitId?.Trim(),
            LocationDescription = request.LocationDescription?.Trim(),
            SerialOrInvoiceNumber = request.SerialOrInvoiceNumber?.Trim(),
            Status = request.Status,
            Notes = request.Notes?.Trim()
        };

        await assetRepository.AddAsync(asset, cancellationToken);
        return MapToResponse(asset);
    }

    /// <inheritdoc />
    public async Task<AssetResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetByIdAsync(id, [], cancellationToken);
        return asset is null ? null : MapToResponse(asset);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AssetResponse>> ListAsync(ListAssetsRequest request, CancellationToken cancellationToken)
    {
        var assets = await assetRepository.ListAsync(
            a => (request.SchoolUnitId == null || a.SchoolUnitId == request.SchoolUnitId) &&
                 (request.Status == null || a.Status == request.Status) &&
                 (request.Category == null || a.Category == request.Category),
            [],
            cancellationToken);

        return assets.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<AssetResponse?> UpdateAsync(UpdateAssetRequest request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (asset is null)
        {
            return null;
        }

        asset.Name = request.Name.Trim();
        asset.AssetTag = request.AssetTag.Trim();
        asset.Category = request.Category;
        asset.Condition = request.Condition;
        asset.AcquisitionValue = request.AcquisitionValue;
        if (request.AcquisitionDate.HasValue)
        {
            asset.AcquisitionDate = request.AcquisitionDate.Value;
        }
        asset.SchoolUnitId = request.SchoolUnitId?.Trim();
        asset.LocationDescription = request.LocationDescription?.Trim();
        asset.SerialOrInvoiceNumber = request.SerialOrInvoiceNumber?.Trim();
        asset.Status = request.Status;
        asset.Notes = request.Notes?.Trim();
        asset.UpdatedAt = DateTime.UtcNow;

        await assetRepository.UpdateAsync(asset, cancellationToken);
        return MapToResponse(asset);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (asset is null)
        {
            return (false, "Asset not found.");
        }

        await assetRepository.RemoveAsync(asset, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> AssetTagExistsAsync(string assetTag, string? excludeId, CancellationToken cancellationToken)
    {
        var trimmed = assetTag.Trim();
        return await assetRepository.ExistsAsync(
            a => a.AssetTag == trimmed && (excludeId == null || a.Id != excludeId),
            cancellationToken);
    }

    private static AssetResponse MapToResponse(Asset asset) =>
        new(
            asset.Id,
            asset.Name,
            asset.AssetTag,
            asset.Category,
            asset.Condition,
            asset.AcquisitionValue,
            asset.AcquisitionDate,
            asset.SchoolUnitId,
            asset.LocationDescription,
            asset.SerialOrInvoiceNumber,
            asset.Status,
            asset.Notes,
            asset.CreatedAt,
            asset.UpdatedAt,
            asset.CreatedBy,
            asset.UpdatedBy);
}

