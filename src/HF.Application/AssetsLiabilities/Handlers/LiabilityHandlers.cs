using HF.Application.AssetsLiabilities.Abstract;
using HF.Application.AssetsLiabilities.Requests;
using HF.Application.AssetsLiabilities.Responses;
using HF.Application.Common.Responses;
using Mediator;

namespace HF.Application.AssetsLiabilities.Handlers;

/// <summary>
/// Mediator handlers for liability operations.
/// </summary>
public sealed class LiabilityHandlers(ILiabilityService liabilityService)
    : IRequestHandler<CreateLiabilityRequest, ResponseBase<LiabilityResponse>>,
      IRequestHandler<GetLiabilityByIdRequest, ResponseBase<LiabilityResponse>>,
      IRequestHandler<ListLiabilitiesRequest, ResponseBase<IReadOnlyList<LiabilityResponse>>>,
      IRequestHandler<UpdateLiabilityRequest, ResponseBase<LiabilityResponse>>,
      IRequestHandler<DeleteLiabilityRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<LiabilityResponse>> Handle(
        CreateLiabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await liabilityService.CreateAsync(request, cancellationToken);
        return ResponseBase<LiabilityResponse>.SuccessResponse(result, "Liability created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<LiabilityResponse>> Handle(
        GetLiabilityByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await liabilityService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<LiabilityResponse>.FailureResponse("Liability not found.")
            : ResponseBase<LiabilityResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<LiabilityResponse>>> Handle(
        ListLiabilitiesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await liabilityService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<LiabilityResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<LiabilityResponse>> Handle(
        UpdateLiabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await liabilityService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<LiabilityResponse>.FailureResponse("Liability not found.")
            : ResponseBase<LiabilityResponse>.SuccessResponse(result, "Liability updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteLiabilityRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await liabilityService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Liability deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete liability.");
    }
}

