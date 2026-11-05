using HF.Domain.Common.Entities;

namespace HF.Domain.AssetsLiabilities;

/// <summary>
/// Represents a physical or patrimonial asset belonging to or in use by the NGO (Ativo).
/// </summary>
public class Asset(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Asset name or description.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Institutional patrimonial tag / asset inventory code (número de tombamento).
    /// </summary>
    public string AssetTag { get; set; } = string.Empty;

    /// <summary>
    /// Classification category.
    /// </summary>
    public AssetCategory Category { get; set; } = AssetCategory.Other;

    /// <summary>
    /// Physical state / conservation condition.
    /// </summary>
    public AssetCondition Condition { get; set; } = AssetCondition.Good;

    /// <summary>
    /// Acquisition cost or declared donation value.
    /// </summary>
    public decimal AcquisitionValue { get; set; }

    /// <summary>
    /// Date of acquisition or donation.
    /// </summary>
    public DateOnly AcquisitionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// School unit where the asset is located.
    /// </summary>
    public string? SchoolUnitId { get; set; }

    /// <summary>
    /// Specific room or location within the unit (e.g., Computer Lab 1, Library).
    /// </summary>
    public string? LocationDescription { get; set; }

    /// <summary>
    /// Serial number or invoice reference.
    /// </summary>
    public string? SerialOrInvoiceNumber { get; set; }

    /// <summary>
    /// Operational status of the asset.
    /// </summary>
    public AssetStatus Status { get; set; } = AssetStatus.InUse;

    /// <summary>
    /// General observations or maintenance notes.
    /// </summary>
    public string? Notes { get; set; }
}

