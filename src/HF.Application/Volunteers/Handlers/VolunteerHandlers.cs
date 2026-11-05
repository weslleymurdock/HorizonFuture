using HF.Application.Common.Responses;
using HF.Application.Volunteers.Abstract;
using HF.Application.Volunteers.Requests;
using HF.Application.Volunteers.Responses;
using Mediator;

namespace HF.Application.Volunteers.Handlers;

/// <summary>
/// Mediator handlers for volunteer operations.
/// </summary>
public sealed class VolunteerHandlers(IVolunteerService volunteerService)
    : IRequestHandler<CreateVolunteerRequest, ResponseBase<VolunteerResponse>>,
      IRequestHandler<GetVolunteerByIdRequest, ResponseBase<VolunteerResponse>>,
      IRequestHandler<ListVolunteersRequest, ResponseBase<IReadOnlyList<VolunteerResponse>>>,
      IRequestHandler<UpdateVolunteerRequest, ResponseBase<VolunteerResponse>>,
      IRequestHandler<DeleteVolunteerRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<VolunteerResponse>> Handle(
        CreateVolunteerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await volunteerService.CreateAsync(request, cancellationToken);
        return ResponseBase<VolunteerResponse>.SuccessResponse(result, "Volunteer created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<VolunteerResponse>> Handle(
        GetVolunteerByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await volunteerService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<VolunteerResponse>.FailureResponse("Volunteer not found.")
            : ResponseBase<VolunteerResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<VolunteerResponse>>> Handle(
        ListVolunteersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await volunteerService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<VolunteerResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<VolunteerResponse>> Handle(
        UpdateVolunteerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await volunteerService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<VolunteerResponse>.FailureResponse("Volunteer not found.")
            : ResponseBase<VolunteerResponse>.SuccessResponse(result, "Volunteer updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteVolunteerRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await volunteerService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Volunteer deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete volunteer.");
    }
}

