using HF.Application.AssetsLiabilities.Responses;
using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Common.Responses;
using HF.Domain.AssetsLiabilities;
using Mediator;

namespace HF.Application.AssetsLiabilities.Requests;

/// <summary>Command to create a new asset.</summary>
public sealed record CreateAssetRequest(
    string Name,
    string AssetTag,
    decimal AcquisitionValue,
    AssetCategory Category = AssetCategory.Other,
    AssetCondition Condition = AssetCondition.Good,
    DateOnly? AcquisitionDate = null,
    string? SchoolUnitId = null,
    string? LocationDescription = null,
    string? SerialOrInvoiceNumber = null,
    AssetStatus Status = AssetStatus.InUse,
    string? Notes = null)
    : IRequest<ResponseBase<AssetResponse>>, ITransactionalRequest;

/// <summary>Query to get an asset by identifier.</summary>
public sealed record GetAssetByIdRequest(string Id)
    : IRequest<ResponseBase<AssetResponse>>;

/// <summary>Query to list assets matching optional filters.</summary>
public sealed record ListAssetsRequest(
    string? SchoolUnitId = null,
    AssetStatus? Status = null,
    AssetCategory? Category = null)
    : IRequest<ResponseBase<IReadOnlyList<AssetResponse>>>;

/// <summary>Command to update an existing asset.</summary>
public sealed record UpdateAssetRequest(
    string Id,
    string Name,
    string AssetTag,
    decimal AcquisitionValue,
    AssetCategory Category = AssetCategory.Other,
    AssetCondition Condition = AssetCondition.Good,
    DateOnly? AcquisitionDate = null,
    string? SchoolUnitId = null,
    string? LocationDescription = null,
    string? SerialOrInvoiceNumber = null,
    AssetStatus Status = AssetStatus.InUse,
    string? Notes = null)
    : IRequest<ResponseBase<AssetResponse>>, ITransactionalRequest;

/// <summary>Command to delete an asset by identifier.</summary>
public sealed record DeleteAssetRequest(string Id)
    : IRequest<ResponseBase<bool>>, ITransactionalRequest;

