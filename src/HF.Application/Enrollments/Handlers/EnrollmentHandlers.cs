using HF.Application.Common.Responses;
using HF.Application.Enrollments.Abstract;
using HF.Application.Enrollments.Requests;
using HF.Application.Enrollments.Responses;
using Mediator;

namespace HF.Application.Enrollments.Handlers;

/// <summary>
/// Mediator handlers for enrollment operations.
/// </summary>
public sealed class EnrollmentHandlers(IEnrollmentService enrollmentService)
    : IRequestHandler<CreateEnrollmentRequest, ResponseBase<EnrollmentResponse>>,
      IRequestHandler<GetEnrollmentByIdRequest, ResponseBase<EnrollmentResponse>>,
      IRequestHandler<ListEnrollmentsRequest, ResponseBase<IReadOnlyList<EnrollmentResponse>>>,
      IRequestHandler<UpdateEnrollmentRequest, ResponseBase<EnrollmentResponse>>,
      IRequestHandler<DeleteEnrollmentRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<EnrollmentResponse>> Handle(
        CreateEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await enrollmentService.CreateAsync(request, cancellationToken);
        return ResponseBase<EnrollmentResponse>.SuccessResponse(result, "Enrollment created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<EnrollmentResponse>> Handle(
        GetEnrollmentByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await enrollmentService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<EnrollmentResponse>.FailureResponse("Enrollment not found.")
            : ResponseBase<EnrollmentResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<EnrollmentResponse>>> Handle(
        ListEnrollmentsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await enrollmentService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<EnrollmentResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<EnrollmentResponse>> Handle(
        UpdateEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await enrollmentService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<EnrollmentResponse>.FailureResponse("Enrollment not found.")
            : ResponseBase<EnrollmentResponse>.SuccessResponse(result, "Enrollment updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await enrollmentService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Enrollment deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete enrollment.");
    }
}

