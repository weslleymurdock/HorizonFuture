using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Governance;

public enum AuthorityAssignmentStatus
{
    Active = 1,
    Revoked = 2,
    Expired = 3
}

public sealed class AuthorityAssignment : AggregateRoot
{
    private AuthorityAssignment() { }

    public AuthorityAssignment(string organizationId, string personId, string responsibility, DateOnly startsOn, DateOnly? endsOn = null, string? unitId = null, string? id = null) : base(id)
    {
        if (endsOn < startsOn)
        {
            throw new DomainException("authority.invalid_period", "An authority assignment cannot end before it starts.");
        }

        OrganizationId = organizationId;
        PersonId = personId;
        Responsibility = Require(responsibility);
        StartsOn = startsOn;
        EndsOn = endsOn;
        UnitId = unitId;
        Status = AuthorityAssignmentStatus.Active;
    }

    public string OrganizationId { get; private set; } = string.Empty;
    public string PersonId { get; private set; } = string.Empty;
    public string? UnitId { get; private set; }
    public string Responsibility { get; private set; } = string.Empty;
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public AuthorityAssignmentStatus Status { get; private set; }

    public void Revoke(DateOnly revokedOn)
    {
        if (revokedOn < StartsOn)
        {
            throw new DomainException("authority.invalid_revoke", "An authority cannot be revoked before it starts.");
        }

        EndsOn = revokedOn;
        Status = AuthorityAssignmentStatus.Revoked;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Require(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }
}
