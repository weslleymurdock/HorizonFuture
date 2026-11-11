using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Entities;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Stakeholders;

public sealed class OrganizationalMembership : AggregateRoot
{
    private readonly List<MembershipRole> _roles = [];

    private OrganizationalMembership() { }

    public OrganizationalMembership(
        string organizationId,
        string identityId,
        IdentityKind identityKind,
        DateOnly startsOn,
        string? unitId = null,
        string? departmentId = null,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(identityId);
        OrganizationId = organizationId;
        IdentityId = identityId;
        IdentityKind = identityKind;
        StartsOn = startsOn;
        UnitId = unitId;
        DepartmentId = departmentId;
        Status = MembershipStatus.Active;
        if (EndsOn < StartsOn)
        {
            throw new DomainException("membership.invalid_period", "A membership cannot end before it starts.");
        }
    }

    public string OrganizationId { get; private set; } = string.Empty;
    public string IdentityId { get; private set; } = string.Empty;
    public IdentityKind IdentityKind { get; private set; }
    public string? UnitId { get; private set; }
    public string? DepartmentId { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public MembershipStatus Status { get; private set; }
    public IReadOnlyCollection<MembershipRole> Roles => _roles.AsReadOnly();

    public void AddRole(StakeholderRoleType role, DateOnly startsOn, DateOnly? endsOn = null)
    {
        if (Status != MembershipStatus.Active)
        {
            throw new DomainException("membership.inactive", "Roles cannot be added to an inactive membership.");
        }

        if (endsOn < startsOn)
        {
            throw new DomainException("membership.invalid_role_period", "A role cannot end before it starts.");
        }

        if (startsOn < StartsOn || endsOn > EndsOn)
        {
            throw new DomainException("membership.role_outside_membership", "A role must be contained by the membership period.");
        }

        if (_roles.Any(existing => existing.Role == role && existing.Overlaps(startsOn, endsOn)))
        {
            throw new DomainException("membership.duplicate_role", "The membership already contains an overlapping role of this type.");
        }

        _roles.Add(new MembershipRole(Id, role, startsOn, endsOn));
        UpdatedAt = DateTime.UtcNow;
    }

    public void End(DateOnly endedOn)
    {
        if (Status != MembershipStatus.Active)
        {
            throw new DomainException("membership.already_ended", "The membership is already inactive.");
        }

        if (endedOn < StartsOn)
        {
            throw new DomainException("membership.invalid_period", "A membership cannot end before it starts.");
        }

        Status = MembershipStatus.Ended;
        EndsOn = endedOn;
        foreach (var role in _roles)
        {
            role.End(endedOn);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeScope(string? unitId, string? departmentId)
    {
        if (Status != MembershipStatus.Active)
        {
            throw new DomainException("membership.inactive", "The scope of an inactive membership cannot be changed.");
        }

        UnitId = unitId;
        DepartmentId = departmentId;
        UpdatedAt = DateTime.UtcNow;
    }
}

public sealed class MembershipRole : EntityBase
{
    private MembershipRole() { }

    internal MembershipRole(string membershipId, StakeholderRoleType role, DateOnly startsOn, DateOnly? endsOn, string? id = null) : base(id)
    {
        MembershipId = membershipId;
        Role = role;
        StartsOn = startsOn;
        EndsOn = endsOn;
    }

    public string MembershipId { get; private set; } = string.Empty;
    public StakeholderRoleType Role { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }

    internal bool Overlaps(DateOnly startsOn, DateOnly? endsOn) =>
        StartsOn <= (endsOn ?? DateOnly.MaxValue) && (EndsOn ?? DateOnly.MaxValue) >= startsOn;

    internal void End(DateOnly endedOn) => EndsOn = endedOn;
}
