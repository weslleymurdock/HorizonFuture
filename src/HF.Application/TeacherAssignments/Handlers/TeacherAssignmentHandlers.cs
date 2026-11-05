using HF.Application.Common.Responses;
using HF.Application.TeacherAssignments.Abstract;
using HF.Application.TeacherAssignments.Requests;
using HF.Application.TeacherAssignments.Responses;
using Mediator;

namespace HF.Application.TeacherAssignments.Handlers;

/// <summary>
/// Mediator handlers for teacher assignment operations.
/// </summary>
public sealed class TeacherAssignmentHandlers(ITeacherAssignmentService assignmentService)
    : IRequestHandler<CreateTeacherAssignmentRequest, ResponseBase<TeacherAssignmentResponse>>,
      IRequestHandler<GetTeacherAssignmentByIdRequest, ResponseBase<TeacherAssignmentResponse>>,
      IRequestHandler<ListTeacherAssignmentsRequest, ResponseBase<IReadOnlyList<TeacherAssignmentResponse>>>,
      IRequestHandler<UpdateTeacherAssignmentRequest, ResponseBase<TeacherAssignmentResponse>>,
      IRequestHandler<DeleteTeacherAssignmentRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<TeacherAssignmentResponse>> Handle(
        CreateTeacherAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await assignmentService.CreateAsync(request, cancellationToken);
        return ResponseBase<TeacherAssignmentResponse>.SuccessResponse(result, "Teacher assignment created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<TeacherAssignmentResponse>> Handle(
        GetTeacherAssignmentByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await assignmentService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<TeacherAssignmentResponse>.FailureResponse("Teacher assignment not found.")
            : ResponseBase<TeacherAssignmentResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<TeacherAssignmentResponse>>> Handle(
        ListTeacherAssignmentsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await assignmentService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<TeacherAssignmentResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<TeacherAssignmentResponse>> Handle(
        UpdateTeacherAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await assignmentService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<TeacherAssignmentResponse>.FailureResponse("Teacher assignment not found.")
            : ResponseBase<TeacherAssignmentResponse>.SuccessResponse(result, "Teacher assignment updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteTeacherAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await assignmentService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Teacher assignment deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete teacher assignment.");
    }
}

