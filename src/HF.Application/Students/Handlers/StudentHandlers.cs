using HF.Application.Common.Responses;
using HF.Application.Students.Abstract;
using HF.Application.Students.Requests;
using HF.Application.Students.Responses;
using Mediator;

namespace HF.Application.Students.Handlers;

/// <summary>
/// Mediator handlers for student operations.
/// </summary>
public sealed class StudentHandlers(IStudentService studentService)
    : IRequestHandler<CreateStudentRequest, ResponseBase<StudentResponse>>,
      IRequestHandler<GetStudentByIdRequest, ResponseBase<StudentResponse>>,
      IRequestHandler<ListStudentsRequest, ResponseBase<IReadOnlyList<StudentResponse>>>,
      IRequestHandler<UpdateStudentRequest, ResponseBase<StudentResponse>>,
      IRequestHandler<DeleteStudentRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<StudentResponse>> Handle(
        CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await studentService.CreateAsync(request, cancellationToken);
        return ResponseBase<StudentResponse>.SuccessResponse(result, "Student created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<StudentResponse>> Handle(
        GetStudentByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await studentService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<StudentResponse>.FailureResponse("Student not found.")
            : ResponseBase<StudentResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<StudentResponse>>> Handle(
        ListStudentsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await studentService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<StudentResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<StudentResponse>> Handle(
        UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await studentService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<StudentResponse>.FailureResponse("Student not found.")
            : ResponseBase<StudentResponse>.SuccessResponse(result, "Student updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteStudentRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await studentService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Student deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete student.");
    }
}

