using HF.Application.AssetsLiabilities.Responses;
using HF.Application.Common.Responses;
using HF.Domain.AssetsLiabilities;
using Mediator;

namespace HF.Application.AssetsLiabilities.Requests;

/// <summary>Command to create a new liability.</summary>
public sealed record CreateLiabilityRequest(
    string Title,
    decimal Amount,
    DateOnly DueDate,
    string Creditor,
    LiabilityCategory Category = LiabilityCategory.Other,
    DateOnly? PaymentDate = null,
    decimal? PaidAmount = null,
    string? DocumentNumber = null,
    string? SchoolUnitId = null,
    LiabilityStatus Status = LiabilityStatus.Pending,
    string? Notes = null)
    : IRequest<ResponseBase<LiabilityResponse>>;

/// <summary>Query to get a liability by identifier.</summary>
public sealed record GetLiabilityByIdRequest(string Id)
    : IRequest<ResponseBase<LiabilityResponse>>;

/// <summary>Query to list liabilities matching optional filters.</summary>
public sealed record ListLiabilitiesRequest(
    string? SchoolUnitId = null,
    LiabilityStatus? Status = null,
    LiabilityCategory? Category = null)
    : IRequest<ResponseBase<IReadOnlyList<LiabilityResponse>>>;

/// <summary>Command to update an existing liability.</summary>
public sealed record UpdateLiabilityRequest(
    string Id,
    string Title,
    decimal Amount,
    DateOnly DueDate,
    string Creditor,
    LiabilityCategory Category = LiabilityCategory.Other,
    DateOnly? PaymentDate = null,
    decimal? PaidAmount = null,
    string? DocumentNumber = null,
    string? SchoolUnitId = null,
    LiabilityStatus Status = LiabilityStatus.Pending,
    string? Notes = null)
    : IRequest<ResponseBase<LiabilityResponse>>;

/// <summary>Command to delete a liability by identifier.</summary>
public sealed record DeleteLiabilityRequest(string Id)
    : IRequest<ResponseBase<bool>>;

