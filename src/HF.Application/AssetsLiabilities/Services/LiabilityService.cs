using HF.Application.AssetsLiabilities.Abstract;
using HF.Application.AssetsLiabilities.Requests;
using HF.Application.AssetsLiabilities.Responses;
using HF.Application.Common.Abstractions.Repositories;
using HF.Domain.AssetsLiabilities;

namespace HF.Application.AssetsLiabilities.Services;

/// <summary>
/// Implements application use cases and business rules for liabilities.
/// </summary>
public sealed class LiabilityService(IRepository<Liability> liabilityRepository) : ILiabilityService
{
    /// <inheritdoc />
    public async Task<LiabilityResponse> CreateAsync(CreateLiabilityRequest request, CancellationToken cancellationToken)
    {
        var liability = new Liability
        {
            Title = request.Title.Trim(),
            Category = request.Category,
            Amount = request.Amount,
            DueDate = request.DueDate,
            PaymentDate = request.PaymentDate,
            PaidAmount = request.PaidAmount,
            Creditor = request.Creditor.Trim(),
            DocumentNumber = request.DocumentNumber?.Trim(),
            SchoolUnitId = request.SchoolUnitId?.Trim(),
            Status = request.Status,
            Notes = request.Notes?.Trim()
        };

        await liabilityRepository.AddAsync(liability, cancellationToken);
        return MapToResponse(liability);
    }

    /// <inheritdoc />
    public async Task<LiabilityResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var liability = await liabilityRepository.GetByIdAsync(id, [], cancellationToken);
        return liability is null ? null : MapToResponse(liability);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LiabilityResponse>> ListAsync(ListLiabilitiesRequest request, CancellationToken cancellationToken)
    {
        var liabilities = await liabilityRepository.ListAsync(
            l => (request.SchoolUnitId == null || l.SchoolUnitId == request.SchoolUnitId) &&
                 (request.Status == null || l.Status == request.Status) &&
                 (request.Category == null || l.Category == request.Category),
            [],
            cancellationToken);

        return liabilities.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<LiabilityResponse?> UpdateAsync(UpdateLiabilityRequest request, CancellationToken cancellationToken)
    {
        var liability = await liabilityRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (liability is null)
        {
            return null;
        }

        liability.Title = request.Title.Trim();
        liability.Category = request.Category;
        liability.Amount = request.Amount;
        liability.DueDate = request.DueDate;
        liability.PaymentDate = request.PaymentDate;
        liability.PaidAmount = request.PaidAmount;
        liability.Creditor = request.Creditor.Trim();
        liability.DocumentNumber = request.DocumentNumber?.Trim();
        liability.SchoolUnitId = request.SchoolUnitId?.Trim();
        liability.Status = request.Status;
        liability.Notes = request.Notes?.Trim();
        liability.UpdatedAt = DateTime.UtcNow;

        await liabilityRepository.UpdateAsync(liability, cancellationToken);
        return MapToResponse(liability);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var liability = await liabilityRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (liability is null)
        {
            return (false, "Liability not found.");
        }

        await liabilityRepository.RemoveAsync(liability, cancellationToken);
        return (true, null);
    }

    private static LiabilityResponse MapToResponse(Liability liability) =>
        new(
            liability.Id,
            liability.Title,
            liability.Category,
            liability.Amount,
            liability.DueDate,
            liability.PaymentDate,
            liability.PaidAmount,
            liability.Creditor,
            liability.DocumentNumber,
            liability.SchoolUnitId,
            liability.Status,
            liability.Notes,
            liability.CreatedAt,
            liability.UpdatedAt,
            liability.CreatedBy,
            liability.UpdatedBy);
}

