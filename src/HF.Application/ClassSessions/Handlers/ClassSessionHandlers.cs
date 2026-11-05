using HF.Application.ClassSessions.Abstract;
using HF.Application.ClassSessions.Requests;
using HF.Application.ClassSessions.Responses;
using HF.Application.Common.Responses;
using Mediator;

namespace HF.Application.ClassSessions.Handlers;

/// <summary>
/// Mediator handlers for class session operations.
/// </summary>
public sealed class ClassSessionHandlers(IClassSessionService classSessionService)
    : IRequestHandler<CreateClassSessionRequest, ResponseBase<ClassSessionResponse>>,
      IRequestHandler<GetClassSessionByIdRequest, ResponseBase<ClassSessionResponse>>,
      IRequestHandler<ListClassSessionsRequest, ResponseBase<IReadOnlyList<ClassSessionResponse>>>,
      IRequestHandler<UpdateClassSessionRequest, ResponseBase<ClassSessionResponse>>,
      IRequestHandler<DeleteClassSessionRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<ClassSessionResponse>> Handle(
        CreateClassSessionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await classSessionService.CreateAsync(request, cancellationToken);
        return ResponseBase<ClassSessionResponse>.SuccessResponse(result, "Class session created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<ClassSessionResponse>> Handle(
        GetClassSessionByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await classSessionService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<ClassSessionResponse>.FailureResponse("Class session not found.")
            : ResponseBase<ClassSessionResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<ClassSessionResponse>>> Handle(
        ListClassSessionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await classSessionService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<ClassSessionResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<ClassSessionResponse>> Handle(
        UpdateClassSessionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await classSessionService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<ClassSessionResponse>.FailureResponse("Class session not found.")
            : ResponseBase<ClassSessionResponse>.SuccessResponse(result, "Class session updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteClassSessionRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await classSessionService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Class session deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete class session.");
    }
}

