using HF.Application.Auth.Abstract;
using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Directors.Abstract;
using HF.Application.Directors.Requests;
using HF.Application.Directors.Responses;
using HF.Domain.Directors;

namespace HF.Application.Directors.Services;

/// <summary>
/// Implements application use cases and business rules for directors.
/// </summary>
public sealed class DirectorService(
    IRepository<Director> directorRepository,
    IIdentityService identityService)
    : IDirectorService
{
    /// <inheritdoc />
    public async Task<DirectorResponse> CreateAsync(CreateDirectorRequest request, CancellationToken cancellationToken)
    {
        var director = new Director
        {
            UserId = request.UserId,
            Directorate = request.Directorate.Trim(),
            PositionTitle = request.PositionTitle.Trim(),
            SchoolUnitId = request.SchoolUnitId?.Trim(),
            MandateStart = request.MandateStart ?? DateOnly.FromDateTime(DateTime.UtcNow),
            MandateEnd = request.MandateEnd,
            Status = request.Status
        };

        await directorRepository.AddAsync(director, cancellationToken);
        return MapToResponse(director);
    }

    /// <inheritdoc />
    public async Task<DirectorResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var director = await directorRepository.GetByIdAsync(id, [], cancellationToken);
        return director is null ? null : MapToResponse(director);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DirectorResponse>> ListAsync(ListDirectorsRequest request, CancellationToken cancellationToken)
    {
        var directors = await directorRepository.ListAsync(
            d => (request.SchoolUnitId == null || d.SchoolUnitId == request.SchoolUnitId) &&
                 (request.Status == null || d.Status == request.Status) &&
                 (request.Directorate == null || d.Directorate == request.Directorate),
            [],
            cancellationToken);

        return directors.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<DirectorResponse?> UpdateAsync(UpdateDirectorRequest request, CancellationToken cancellationToken)
    {
        var director = await directorRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (director is null)
        {
            return null;
        }

        director.Directorate = request.Directorate.Trim();
        director.PositionTitle = request.PositionTitle.Trim();
        director.SchoolUnitId = request.SchoolUnitId?.Trim();
        if (request.MandateStart.HasValue)
        {
            director.MandateStart = request.MandateStart.Value;
        }
        director.MandateEnd = request.MandateEnd;
        director.Status = request.Status;
        director.UpdatedAt = DateTime.UtcNow;

        await directorRepository.UpdateAsync(director, cancellationToken);
        return MapToResponse(director);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var director = await directorRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (director is null)
        {
            return (false, "Director not found.");
        }

        await directorRepository.RemoveAsync(director, cancellationToken);
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

        bool alreadyAssigned = await directorRepository.ExistsAsync(
            d => d.UserId == userId && (excludeId == null || d.Id != excludeId),
            cancellationToken);

        return !alreadyAssigned;
    }

    private static DirectorResponse MapToResponse(Director director) =>
        new(
            director.Id,
            director.UserId,
            director.Directorate,
            director.PositionTitle,
            director.SchoolUnitId,
            director.MandateStart,
            director.MandateEnd,
            director.Status,
            director.CreatedAt,
            director.UpdatedAt,
            director.CreatedBy,
            director.UpdatedBy);
}

