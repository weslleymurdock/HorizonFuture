using HF.Application.Attendances.Abstract;
using HF.Application.Attendances.Requests;
using HF.Application.Attendances.Responses;
using HF.Application.Common.Responses;
using Mediator;

namespace HF.Application.Attendances.Handlers;

/// <summary>
/// Mediator handlers for attendance operations.
/// </summary>
public sealed class AttendanceHandlers(IAttendanceService attendanceService)
    : IRequestHandler<CreateAttendanceRequest, ResponseBase<AttendanceResponse>>,
      IRequestHandler<GetAttendanceByIdRequest, ResponseBase<AttendanceResponse>>,
      IRequestHandler<ListAttendancesRequest, ResponseBase<IReadOnlyList<AttendanceResponse>>>,
      IRequestHandler<UpdateAttendanceRequest, ResponseBase<AttendanceResponse>>,
      IRequestHandler<DeleteAttendanceRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<AttendanceResponse>> Handle(
        CreateAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await attendanceService.CreateAsync(request, cancellationToken);
        return ResponseBase<AttendanceResponse>.SuccessResponse(result, "Attendance recorded successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<AttendanceResponse>> Handle(
        GetAttendanceByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await attendanceService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<AttendanceResponse>.FailureResponse("Attendance record not found.")
            : ResponseBase<AttendanceResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<AttendanceResponse>>> Handle(
        ListAttendancesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await attendanceService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<AttendanceResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<AttendanceResponse>> Handle(
        UpdateAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await attendanceService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<AttendanceResponse>.FailureResponse("Attendance record not found.")
            : ResponseBase<AttendanceResponse>.SuccessResponse(result, "Attendance updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await attendanceService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Attendance deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete attendance record.");
    }
}

