using HF.Domain.Common.Exceptions;
using HF.Domain.Common.ValueObjects;

namespace HF.Domain.SocialServices.Deliveries;

public sealed class BenefitQuantity : ValueObject
{
    public BenefitQuantity(decimal amount, string unitOfMeasure)
    {
        if (amount <= 0)
        {
            throw new DomainException("benefit.invalid_quantity", "Benefit quantity amount must be strictly greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
        {
            throw new DomainException("benefit.unit_required", "A unit of measure is required for benefit quantity.");
        }

        Amount = amount;
        UnitOfMeasure = unitOfMeasure.Trim();
    }

    public decimal Amount { get; }
    public string UnitOfMeasure { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return UnitOfMeasure;
    }

    public override string ToString() => $"{Amount} {UnitOfMeasure}";
}
