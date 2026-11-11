using HF.Domain.Common.Exceptions;
using HF.Domain.Common.ValueObjects;

namespace HF.Domain.SocialServices.Programs;

public sealed class OperatingPeriod : ValueObject
{
    public OperatingPeriod(DateOnly startDate, DateOnly? endDate = null)
    {
        if (endDate.HasValue && endDate.Value < startDate)
        {
            throw new DomainException("operating_period.invalid_range", "The operating end date cannot be earlier than the start date.");
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    public DateOnly StartDate { get; }
    public DateOnly? EndDate { get; }

    public bool IsActiveOn(DateOnly date)
    {
        if (date < StartDate)
        {
            return false;
        }

        return !EndDate.HasValue || date <= EndDate.Value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }
}
