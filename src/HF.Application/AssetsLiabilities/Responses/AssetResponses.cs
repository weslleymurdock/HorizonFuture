using HF.Domain.AssetsLiabilities;

namespace HF.Application.AssetsLiabilities.Responses;

/// <summary>
/// Data contract representing a patrimonial asset.
/// </summary>
public sealed record AssetResponse(
    string Id,
    string Name,
    string AssetTag,
    AssetCategory Category,
    AssetCondition Condition,
    decimal AcquisitionValue,
    DateOnly AcquisitionDate,
    string? SchoolUnitId,
    string? LocationDescription,
    string? SerialOrInvoiceNumber,
    AssetStatus Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

