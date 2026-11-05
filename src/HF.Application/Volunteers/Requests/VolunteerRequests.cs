using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Common.Responses;
using HF.Application.Volunteers.Responses;
using HF.Domain.Volunteers;
using Mediator;

namespace HF.Application.Volunteers.Requests;

/// <summary>Command to create a new volunteer.</summary>
public sealed record CreateVolunteerRequest(
    string UserId,
    string? SkillsAndInterests = null,
    int? AvailableHoursPerWeek = null,
    string? AvailabilityNotes = null,
    string? SchoolUnitId = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null,
    VolunteerStatus Status = VolunteerStatus.Active)
    : IRequest<ResponseBase<VolunteerResponse>>, ITransactionalRequest;

/// <summary>Query to get a volunteer by identifier.</summary>
public sealed record GetVolunteerByIdRequest(string Id)
    : IRequest<ResponseBase<VolunteerResponse>>;

/// <summary>Query to list volunteers matching optional filters.</summary>
public sealed record ListVolunteersRequest(
    string? SchoolUnitId = null,
    VolunteerStatus? Status = null)
    : IRequest<ResponseBase<IReadOnlyList<VolunteerResponse>>>;

/// <summary>Command to update an existing volunteer.</summary>
public sealed record UpdateVolunteerRequest(
    string Id,
    string? SkillsAndInterests = null,
    int? AvailableHoursPerWeek = null,
    string? AvailabilityNotes = null,
    string? SchoolUnitId = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null,
    VolunteerStatus Status = VolunteerStatus.Active)
    : IRequest<ResponseBase<VolunteerResponse>>, ITransactionalRequest;

/// <summary>Command to delete a volunteer by identifier.</summary>
public sealed record DeleteVolunteerRequest(string Id)
    : IRequest<ResponseBase<bool>>, ITransactionalRequest;

