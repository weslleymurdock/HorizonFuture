using HF.Application.Collaborators.Abstract;
using HF.Application.Collaborators.Requests;
using HF.Application.Collaborators.Responses;
using HF.Application.Common.Responses;
using Mediator;

namespace HF.Application.Collaborators.Handlers;

/// <summary>
/// Mediator handlers for collaborator operations.
/// </summary>
public sealed class CollaboratorHandlers(ICollaboratorService collaboratorService)
    : IRequestHandler<CreateCollaboratorRequest, ResponseBase<CollaboratorResponse>>,
      IRequestHandler<GetCollaboratorByIdRequest, ResponseBase<CollaboratorResponse>>,
      IRequestHandler<ListCollaboratorsRequest, ResponseBase<IReadOnlyList<CollaboratorResponse>>>,
      IRequestHandler<UpdateCollaboratorRequest, ResponseBase<CollaboratorResponse>>,
      IRequestHandler<DeleteCollaboratorRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<CollaboratorResponse>> Handle(
        CreateCollaboratorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await collaboratorService.CreateAsync(request, cancellationToken);
        return ResponseBase<CollaboratorResponse>.SuccessResponse(result, "Collaborator created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<CollaboratorResponse>> Handle(
        GetCollaboratorByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await collaboratorService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<CollaboratorResponse>.FailureResponse("Collaborator not found.")
            : ResponseBase<CollaboratorResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<CollaboratorResponse>>> Handle(
        ListCollaboratorsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await collaboratorService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<CollaboratorResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<CollaboratorResponse>> Handle(
        UpdateCollaboratorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await collaboratorService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<CollaboratorResponse>.FailureResponse("Collaborator not found.")
            : ResponseBase<CollaboratorResponse>.SuccessResponse(result, "Collaborator updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteCollaboratorRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await collaboratorService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Collaborator deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete collaborator.");
    }
}

