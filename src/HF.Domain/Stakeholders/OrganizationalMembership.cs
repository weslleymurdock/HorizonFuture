using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Entities;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Stakeholders;

public sealed class OrganizationalMembership : AggregateRoot
{
    private readonly List<MembershipRole> _roles = [];

    private OrganizationalMembership() { }

    public OrganizationalMembership(
        MembershipScope scope,
        string identityId,
        IdentityKind identityKind,
        DateOnly startsOn,
        string? id = null) : base(id)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(identityId);
        if (!scope.IsStructurallyValid)
        {
            throw new DomainException("membership.scope_mismatch", "The membership scope does not contain a valid department relationship.");
        }

        OrganizationId = scope.OrganizationId;
        IdentityId = identityId;
        IdentityKind = identityKind;
        StartsOn = startsOn;
        UnitId = scope.UnitId;
        DepartmentId = scope.DepartmentId;
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

    public void ChangeScope(MembershipScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (Status != MembershipStatus.Active)
        {
            throw new DomainException("membership.inactive", "The scope of an inactive membership cannot be changed.");
        }

        if (!scope.IsStructurallyValid ||
            scope.OrganizationId != OrganizationId)
        {
            throw new DomainException("membership.scope_mismatch", "The membership scope does not belong to the membership organization.");
        }

        UnitId = scope.UnitId;
        DepartmentId = scope.DepartmentId;
        UpdatedAt = DateTime.UtcNow;
    }

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

        if (_roles.Any(role => role.StartsOn > endedOn))
        {
            throw new DomainException("membership.invalid_period", "A membership cannot end before an active role starts.");
        }

        Status = MembershipStatus.Ended;
        EndsOn = endedOn;
        foreach (var role in _roles.Where(role => role.EndsOn is null))
        {
            role.End(endedOn);
        }

        UpdatedAt = DateTime.UtcNow;
    }

}

public sealed record MembershipScope
{
    public MembershipScope(
        string organizationId,
        string? unitId = null,
        string? departmentId = null,
        string? departmentUnitId = null,
        bool departmentWithoutUnitConfirmed = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);

        OrganizationId = organizationId;
        UnitId = unitId;
        DepartmentId = departmentId;
        DepartmentUnitId = departmentUnitId;
        DepartmentWithoutUnitConfirmed = departmentWithoutUnitConfirmed;

        if (!IsStructurallyValid)
        {
            throw new DomainException("membership.scope_mismatch", "The membership scope does not contain a valid department relationship.");
        }
    }

    public string OrganizationId { get; }
    public string? UnitId { get; }
    public string? DepartmentId { get; }
    public string? DepartmentUnitId { get; }
    public bool DepartmentWithoutUnitConfirmed { get; }
    public bool IsStructurallyValid =>
        DepartmentId is null
            ? UnitId is null
                ? DepartmentUnitId is null && !DepartmentWithoutUnitConfirmed
                : DepartmentUnitId is null && !DepartmentWithoutUnitConfirmed
            : UnitId is null
                ? DepartmentUnitId is null && DepartmentWithoutUnitConfirmed
                : DepartmentUnitId == UnitId && !DepartmentWithoutUnitConfirmed;
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
