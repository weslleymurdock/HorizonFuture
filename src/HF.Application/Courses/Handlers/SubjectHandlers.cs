using HF.Application.Common.Responses;
using HF.Application.Courses.Abstract;
using HF.Application.Courses.Requests;
using HF.Application.Courses.Responses;
using Mediator;

namespace HF.Application.Courses.Handlers;

/// <summary>
/// Mediator handlers for subject operations.
/// </summary>
public sealed class SubjectHandlers(ISubjectService subjectService)
    : IRequestHandler<CreateSubjectRequest, ResponseBase<SubjectResponse>>,
      IRequestHandler<GetSubjectByIdRequest, ResponseBase<SubjectResponse>>,
      IRequestHandler<ListSubjectsRequest, ResponseBase<IReadOnlyList<SubjectResponse>>>,
      IRequestHandler<UpdateSubjectRequest, ResponseBase<SubjectResponse>>,
      IRequestHandler<DeleteSubjectRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<SubjectResponse>> Handle(
        CreateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await subjectService.CreateAsync(request, cancellationToken);
        return ResponseBase<SubjectResponse>.SuccessResponse(result, "Subject created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<SubjectResponse>> Handle(
        GetSubjectByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await subjectService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<SubjectResponse>.FailureResponse("Subject not found.")
            : ResponseBase<SubjectResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<SubjectResponse>>> Handle(
        ListSubjectsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await subjectService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<SubjectResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<SubjectResponse>> Handle(
        UpdateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await subjectService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<SubjectResponse>.FailureResponse("Subject not found.")
            : ResponseBase<SubjectResponse>.SuccessResponse(result, "Subject updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await subjectService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Subject deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete subject.");
    }
}

