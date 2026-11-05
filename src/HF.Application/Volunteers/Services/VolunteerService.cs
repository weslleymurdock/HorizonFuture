using HF.Application.Auth.Abstract;
using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Volunteers.Abstract;
using HF.Application.Volunteers.Requests;
using HF.Application.Volunteers.Responses;
using HF.Domain.Volunteers;

namespace HF.Application.Volunteers.Services;

/// <summary>
/// Implements application use cases and business rules for volunteers.
/// </summary>
public sealed class VolunteerService(
    IRepository<Volunteer> volunteerRepository,
    IIdentityService identityService)
    : IVolunteerService
{
    /// <inheritdoc />
    public async Task<VolunteerResponse> CreateAsync(CreateVolunteerRequest request, CancellationToken cancellationToken)
    {
        var volunteer = new Volunteer
        {
            UserId = request.UserId,
            SkillsAndInterests = request.SkillsAndInterests?.Trim(),
            AvailableHoursPerWeek = request.AvailableHoursPerWeek,
            AvailabilityNotes = request.AvailabilityNotes?.Trim(),
            SchoolUnitId = request.SchoolUnitId?.Trim(),
            StartDate = request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = request.EndDate,
            EmergencyContactName = request.EmergencyContactName?.Trim(),
            EmergencyContactPhone = request.EmergencyContactPhone?.Trim(),
            Status = request.Status
        };

        await volunteerRepository.AddAsync(volunteer, cancellationToken);
        return MapToResponse(volunteer);
    }

    /// <inheritdoc />
    public async Task<VolunteerResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var volunteer = await volunteerRepository.GetByIdAsync(id, [], cancellationToken);
        return volunteer is null ? null : MapToResponse(volunteer);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VolunteerResponse>> ListAsync(ListVolunteersRequest request, CancellationToken cancellationToken)
    {
        var volunteers = await volunteerRepository.ListAsync(
            v => (request.SchoolUnitId == null || v.SchoolUnitId == request.SchoolUnitId) &&
                 (request.Status == null || v.Status == request.Status),
            [],
            cancellationToken);

        return volunteers.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<VolunteerResponse?> UpdateAsync(UpdateVolunteerRequest request, CancellationToken cancellationToken)
    {
        var volunteer = await volunteerRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (volunteer is null)
        {
            return null;
        }

        volunteer.SkillsAndInterests = request.SkillsAndInterests?.Trim();
        volunteer.AvailableHoursPerWeek = request.AvailableHoursPerWeek;
        volunteer.AvailabilityNotes = request.AvailabilityNotes?.Trim();
        volunteer.SchoolUnitId = request.SchoolUnitId?.Trim();
        if (request.StartDate.HasValue)
        {
            volunteer.StartDate = request.StartDate.Value;
        }
        volunteer.EndDate = request.EndDate;
        volunteer.EmergencyContactName = request.EmergencyContactName?.Trim();
        volunteer.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();
        volunteer.Status = request.Status;
        volunteer.UpdatedAt = DateTime.UtcNow;

        await volunteerRepository.UpdateAsync(volunteer, cancellationToken);
        return MapToResponse(volunteer);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var volunteer = await volunteerRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (volunteer is null)
        {
            return (false, "Volunteer not found.");
        }

        await volunteerRepository.RemoveAsync(volunteer, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> UserAvailableAsync(string userId, string? excludeId, CancellationToken cancellationToken)
    {
        var user = await identityService.GetInfoAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        bool alreadyAssigned = await volunteerRepository.ExistsAsync(
            v => v.UserId == userId && (excludeId == null || v.Id != excludeId),
            cancellationToken);

        return !alreadyAssigned;
    }

    private static VolunteerResponse MapToResponse(Volunteer volunteer) =>
        new(
            volunteer.Id,
            volunteer.UserId,
            volunteer.SkillsAndInterests,
            volunteer.AvailableHoursPerWeek,
            volunteer.AvailabilityNotes,
            volunteer.SchoolUnitId,
            volunteer.StartDate,
            volunteer.EndDate,
            volunteer.EmergencyContactName,
            volunteer.EmergencyContactPhone,
            volunteer.Status,
            volunteer.CreatedAt,
            volunteer.UpdatedAt,
            volunteer.CreatedBy,
            volunteer.UpdatedBy);
}

