using HF.Domain.Common.Entities;

namespace HF.Domain.AssetsLiabilities;

/// <summary>
/// Represents a financial obligation, account payable, or liability of the NGO (Passivo).
/// </summary>
public class Liability(string? id = null) : EntityBase(id)
{
    /// <summary>
    /// Title or brief description of the obligation (e.g., Internet Unit 1 - Oct/2026).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Liability category.
    /// </summary>
    public LiabilityCategory Category { get; set; } = LiabilityCategory.Other;

    /// <summary>
    /// Nominal amount due.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Due date for payment.
    /// </summary>
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Date when the obligation was settled.
    /// </summary>
    public DateOnly? PaymentDate { get; set; }

    /// <summary>
    /// Effective amount paid, including interest or discounts.
    /// </summary>
    public decimal? PaidAmount { get; set; }

    /// <summary>
    /// Creditor, vendor, or beneficiary name.
    /// </summary>
    public string Creditor { get; set; } = string.Empty;

    /// <summary>
    /// Document identifier (invoice number, contract number, barcode, etc.).
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Associated school unit if the obligation is specific to a unit, or null for general NGO liabilities.
    /// </summary>
    public string? SchoolUnitId { get; set; }

    /// <summary>
    /// Current settlement status.
    /// </summary>
    public LiabilityStatus Status { get; set; } = LiabilityStatus.Pending;

    /// <summary>
    /// Additional financial or administrative notes.
    /// </summary>
    public string? Notes { get; set; }
}

