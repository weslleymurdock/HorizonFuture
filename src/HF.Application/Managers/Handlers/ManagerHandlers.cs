using HF.Application.Common.Responses;
using HF.Application.Managers.Abstract;
using HF.Application.Managers.Requests;
using HF.Application.Managers.Responses;
using Mediator;

namespace HF.Application.Managers.Handlers;

/// <summary>
/// Mediator handlers for manager operations.
/// </summary>
public sealed class ManagerHandlers(IManagerService managerService)
    : IRequestHandler<CreateManagerRequest, ResponseBase<ManagerResponse>>,
      IRequestHandler<GetManagerByIdRequest, ResponseBase<ManagerResponse>>,
      IRequestHandler<ListManagersRequest, ResponseBase<IReadOnlyList<ManagerResponse>>>,
      IRequestHandler<UpdateManagerRequest, ResponseBase<ManagerResponse>>,
      IRequestHandler<DeleteManagerRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<ManagerResponse>> Handle(
        CreateManagerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await managerService.CreateAsync(request, cancellationToken);
        return ResponseBase<ManagerResponse>.SuccessResponse(result, "Manager created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<ManagerResponse>> Handle(
        GetManagerByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await managerService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<ManagerResponse>.FailureResponse("Manager not found.")
            : ResponseBase<ManagerResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<ManagerResponse>>> Handle(
        ListManagersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await managerService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<ManagerResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<ManagerResponse>> Handle(
        UpdateManagerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await managerService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<ManagerResponse>.FailureResponse("Manager not found.")
            : ResponseBase<ManagerResponse>.SuccessResponse(result, "Manager updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteManagerRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await managerService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Manager deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete manager.");
    }
}

