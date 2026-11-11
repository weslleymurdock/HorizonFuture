using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Governance;

public enum GovernanceMandateStatus
{
    Active = 1,
    Ended = 2,
    Revoked = 3
}

public sealed class GovernanceMandate : AggregateRoot
{
    private GovernanceMandate() { }

    public GovernanceMandate(string organizationId, string bodyId, string personId, string title, DateOnly startsOn, DateOnly? endsOn = null, string? id = null) : base(id)
    {
        if (endsOn < startsOn)
        {
            throw new DomainException("mandate.invalid_period", "A mandate cannot end before it starts.");
        }

        OrganizationId = organizationId;
        BodyId = bodyId;
        PersonId = personId;
        Title = Require(title);
        StartsOn = startsOn;
        EndsOn = endsOn;
        Status = GovernanceMandateStatus.Active;
    }

    public string OrganizationId { get; private set; } = string.Empty;
    public string BodyId { get; private set; } = string.Empty;
    public string PersonId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public GovernanceMandateStatus Status { get; private set; }
    public string? EvidenceReference { get; private set; }

    public void SetEvidenceReference(string? reference)
    {
        EvidenceReference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void End(DateOnly endedOn)
    {
        if (endedOn < StartsOn)
        {
            throw new DomainException("mandate.invalid_end", "A mandate cannot end before it starts.");
        }

        EndsOn = endedOn;
        Status = GovernanceMandateStatus.Ended;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Require(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }
}
