using HF.Application.Common.Responses;
using HF.Application.Directors.Abstract;
using HF.Application.Directors.Requests;
using HF.Application.Directors.Responses;
using Mediator;

namespace HF.Application.Directors.Handlers;

/// <summary>
/// Mediator handlers for director operations.
/// </summary>
public sealed class DirectorHandlers(IDirectorService directorService)
    : IRequestHandler<CreateDirectorRequest, ResponseBase<DirectorResponse>>,
      IRequestHandler<GetDirectorByIdRequest, ResponseBase<DirectorResponse>>,
      IRequestHandler<ListDirectorsRequest, ResponseBase<IReadOnlyList<DirectorResponse>>>,
      IRequestHandler<UpdateDirectorRequest, ResponseBase<DirectorResponse>>,
      IRequestHandler<DeleteDirectorRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<DirectorResponse>> Handle(
        CreateDirectorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await directorService.CreateAsync(request, cancellationToken);
        return ResponseBase<DirectorResponse>.SuccessResponse(result, "Director created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<DirectorResponse>> Handle(
        GetDirectorByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await directorService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<DirectorResponse>.FailureResponse("Director not found.")
            : ResponseBase<DirectorResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<DirectorResponse>>> Handle(
        ListDirectorsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await directorService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<DirectorResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<DirectorResponse>> Handle(
        UpdateDirectorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await directorService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<DirectorResponse>.FailureResponse("Director not found.")
            : ResponseBase<DirectorResponse>.SuccessResponse(result, "Director updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteDirectorRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await directorService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Director deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete director.");
    }
}

