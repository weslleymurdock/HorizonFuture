using HF.Application.Auth.Abstract;
using HF.Application.Collaborators.Abstract;
using HF.Application.Collaborators.Requests;
using HF.Application.Collaborators.Responses;
using HF.Application.Common.Abstractions.Repositories;
using HF.Domain.Collaborators;

namespace HF.Application.Collaborators.Services;

/// <summary>
/// Implements application use cases and business rules for collaborators.
/// </summary>
public sealed class CollaboratorService(
    IRepository<Collaborator> collaboratorRepository,
    IIdentityService identityService)
    : ICollaboratorService
{
    /// <inheritdoc />
    public async Task<CollaboratorResponse> CreateAsync(CreateCollaboratorRequest request, CancellationToken cancellationToken)
    {
        var collaborator = new Collaborator
        {
            UserId = request.UserId,
            PersonId = request.PersonId?.Trim(),
            EmployeeNumber = request.EmployeeNumber.Trim(),
            JobTitle = request.JobTitle.Trim(),
            Department = request.Department.Trim(),
            SchoolUnitId = request.SchoolUnitId?.Trim(),
            ContractType = request.ContractType,
            AdmissionDate = request.AdmissionDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = request.Status
        };

        await collaboratorRepository.AddAsync(collaborator, cancellationToken);
        return MapToResponse(collaborator);
    }

    /// <inheritdoc />
    public async Task<CollaboratorResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var collaborator = await collaboratorRepository.GetByIdAsync(id, [], cancellationToken);
        return collaborator is null ? null : MapToResponse(collaborator);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CollaboratorResponse>> ListAsync(ListCollaboratorsRequest request, CancellationToken cancellationToken)
    {
        var collaborators = await collaboratorRepository.ListAsync(
            c => (request.SchoolUnitId == null || c.SchoolUnitId == request.SchoolUnitId) &&
                 (request.Status == null || c.Status == request.Status) &&
                 (request.Department == null || c.Department == request.Department),
            [],
            cancellationToken);

        return collaborators.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<CollaboratorResponse?> UpdateAsync(UpdateCollaboratorRequest request, CancellationToken cancellationToken)
    {
        var collaborator = await collaboratorRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (collaborator is null)
        {
            return null;
        }

        collaborator.EmployeeNumber = request.EmployeeNumber.Trim();
        collaborator.PersonId = request.PersonId?.Trim();
        collaborator.JobTitle = request.JobTitle.Trim();
        collaborator.Department = request.Department.Trim();
        collaborator.SchoolUnitId = request.SchoolUnitId?.Trim();
        collaborator.ContractType = request.ContractType;
        collaborator.Status = request.Status;
        collaborator.UpdatedAt = DateTime.UtcNow;

        await collaboratorRepository.UpdateAsync(collaborator, cancellationToken);
        return MapToResponse(collaborator);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var collaborator = await collaboratorRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (collaborator is null)
        {
            return (false, "Collaborator not found.");
        }

        await collaboratorRepository.RemoveAsync(collaborator, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> EmployeeNumberExistsAsync(string employeeNumber, string? excludeId, CancellationToken cancellationToken)
    {
        var trimmed = employeeNumber.Trim();
        return await collaboratorRepository.ExistsAsync(
            c => c.EmployeeNumber == trimmed && (excludeId == null || c.Id != excludeId),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> UserAvailableAsync(string userId, string? excludeId, CancellationToken cancellationToken)
    {
        var user = await identityService.GetInfoAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        bool alreadyAssigned = await collaboratorRepository.ExistsAsync(
            c => c.UserId == userId && (excludeId == null || c.Id != excludeId),
            cancellationToken);

        return !alreadyAssigned;
    }

    private static CollaboratorResponse MapToResponse(Collaborator collaborator) =>
        new(
            collaborator.Id,
            collaborator.UserId,
            collaborator.EmployeeNumber,
            collaborator.JobTitle,
            collaborator.Department,
            collaborator.SchoolUnitId,
            collaborator.ContractType,
            collaborator.AdmissionDate,
            collaborator.Status,
            collaborator.CreatedAt,
            collaborator.UpdatedAt,
            collaborator.CreatedBy,
            collaborator.UpdatedBy);
}
