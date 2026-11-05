using HF.Application.Common.Responses;
using HF.Application.Teachers.Abstract;
using HF.Application.Teachers.Requests;
using HF.Application.Teachers.Responses;
using Mediator;

namespace HF.Application.Teachers.Handlers;

/// <summary>
/// Mediator handlers for teacher operations.
/// </summary>
public sealed class TeacherHandlers(ITeacherService teacherService)
    : IRequestHandler<CreateTeacherRequest, ResponseBase<TeacherResponse>>,
      IRequestHandler<GetTeacherByIdRequest, ResponseBase<TeacherResponse>>,
      IRequestHandler<ListTeachersRequest, ResponseBase<IReadOnlyList<TeacherResponse>>>,
      IRequestHandler<UpdateTeacherRequest, ResponseBase<TeacherResponse>>,
      IRequestHandler<DeleteTeacherRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<TeacherResponse>> Handle(
        CreateTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var result = await teacherService.CreateAsync(request, cancellationToken);
        return ResponseBase<TeacherResponse>.SuccessResponse(result, "Teacher created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<TeacherResponse>> Handle(
        GetTeacherByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await teacherService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<TeacherResponse>.FailureResponse("Teacher not found.")
            : ResponseBase<TeacherResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<TeacherResponse>>> Handle(
        ListTeachersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await teacherService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<TeacherResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<TeacherResponse>> Handle(
        UpdateTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var result = await teacherService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<TeacherResponse>.FailureResponse("Teacher not found.")
            : ResponseBase<TeacherResponse>.SuccessResponse(result, "Teacher updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await teacherService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Teacher deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete teacher.");
    }
}

