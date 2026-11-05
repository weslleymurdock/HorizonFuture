using HF.Domain.AssetsLiabilities;

namespace HF.Application.AssetsLiabilities.Responses;

/// <summary>
/// Data contract representing a liability / financial obligation.
/// </summary>
public sealed record LiabilityResponse(
    string Id,
    string Title,
    LiabilityCategory Category,
    decimal Amount,
    DateOnly DueDate,
    DateOnly? PaymentDate,
    decimal? PaidAmount,
    string Creditor,
    string? DocumentNumber,
    string? SchoolUnitId,
    LiabilityStatus Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

