using HF.Application.AssetsLiabilities.Abstract;
using HF.Application.AssetsLiabilities.Requests;
using HF.Application.AssetsLiabilities.Responses;
using HF.Application.Common.Responses;
using Mediator;

namespace HF.Application.AssetsLiabilities.Handlers;

/// <summary>
/// Mediator handlers for asset operations.
/// </summary>
public sealed class AssetHandlers(IAssetService assetService)
    : IRequestHandler<CreateAssetRequest, ResponseBase<AssetResponse>>,
      IRequestHandler<GetAssetByIdRequest, ResponseBase<AssetResponse>>,
      IRequestHandler<ListAssetsRequest, ResponseBase<IReadOnlyList<AssetResponse>>>,
      IRequestHandler<UpdateAssetRequest, ResponseBase<AssetResponse>>,
      IRequestHandler<DeleteAssetRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<AssetResponse>> Handle(
        CreateAssetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await assetService.CreateAsync(request, cancellationToken);
        return ResponseBase<AssetResponse>.SuccessResponse(result, "Asset created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<AssetResponse>> Handle(
        GetAssetByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await assetService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<AssetResponse>.FailureResponse("Asset not found.")
            : ResponseBase<AssetResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<AssetResponse>>> Handle(
        ListAssetsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await assetService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<AssetResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<AssetResponse>> Handle(
        UpdateAssetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await assetService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<AssetResponse>.FailureResponse("Asset not found.")
            : ResponseBase<AssetResponse>.SuccessResponse(result, "Asset updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteAssetRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await assetService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Asset deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete asset.");
    }
}

