using HF.Application.Auth.Abstract;
using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Managers.Abstract;
using HF.Application.Managers.Requests;
using HF.Application.Managers.Responses;
using HF.Domain.Managers;

namespace HF.Application.Managers.Services;

/// <summary>
/// Implements application use cases and business rules for managers.
/// </summary>
public sealed class ManagerService(
    IRepository<Manager> managerRepository,
    IIdentityService identityService)
    : IManagerService
{
    /// <inheritdoc />
    public async Task<ManagerResponse> CreateAsync(CreateManagerRequest request, CancellationToken cancellationToken)
    {
        var manager = new Manager
        {
            UserId = request.UserId,
            PersonId = request.PersonId?.Trim(),
            EmployeeNumber = request.EmployeeNumber.Trim(),
            Department = request.Department.Trim(),
            RoleTitle = request.RoleTitle.Trim(),
            SchoolUnitId = request.SchoolUnitId?.Trim(),
            StartDate = request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = request.Status
        };

        await managerRepository.AddAsync(manager, cancellationToken);
        return MapToResponse(manager);
    }

    /// <inheritdoc />
    public async Task<ManagerResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetByIdAsync(id, [], cancellationToken);
        return manager is null ? null : MapToResponse(manager);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ManagerResponse>> ListAsync(ListManagersRequest request, CancellationToken cancellationToken)
    {
        var managers = await managerRepository.ListAsync(
            m => (request.SchoolUnitId == null || m.SchoolUnitId == request.SchoolUnitId) &&
                 (request.Status == null || m.Status == request.Status) &&
                 (request.Department == null || m.Department == request.Department),
            [],
            cancellationToken);

        return managers.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<ManagerResponse?> UpdateAsync(UpdateManagerRequest request, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (manager is null)
        {
            return null;
        }

        manager.EmployeeNumber = request.EmployeeNumber.Trim();
        manager.PersonId = request.PersonId?.Trim();
        manager.Department = request.Department.Trim();
        manager.RoleTitle = request.RoleTitle.Trim();
        manager.SchoolUnitId = request.SchoolUnitId?.Trim();
        manager.Status = request.Status;
        manager.UpdatedAt = DateTime.UtcNow;

        await managerRepository.UpdateAsync(manager, cancellationToken);
        return MapToResponse(manager);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (manager is null)
        {
            return (false, "Manager not found.");
        }

        await managerRepository.RemoveAsync(manager, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> EmployeeNumberExistsAsync(string employeeNumber, string? excludeId, CancellationToken cancellationToken)
    {
        var trimmed = employeeNumber.Trim();
        return await managerRepository.ExistsAsync(
            m => m.EmployeeNumber == trimmed && (excludeId == null || m.Id != excludeId),
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

        bool alreadyAssigned = await managerRepository.ExistsAsync(
            m => m.UserId == userId && (excludeId == null || m.Id != excludeId),
            cancellationToken);

        return !alreadyAssigned;
    }

    private static ManagerResponse MapToResponse(Manager manager) =>
        new(
            manager.Id,
            manager.UserId,
            manager.EmployeeNumber,
            manager.Department,
            manager.RoleTitle,
            manager.SchoolUnitId,
            manager.StartDate,
            manager.Status,
            manager.CreatedAt,
            manager.UpdatedAt,
            manager.CreatedBy,
            manager.UpdatedBy);
}
