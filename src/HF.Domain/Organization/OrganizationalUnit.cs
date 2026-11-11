using HF.Domain.Common.Entities;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Organization;

public sealed class OrganizationalUnit : EntityBase
{
    private OrganizationalUnit() { }

    internal OrganizationalUnit(string organizationId, string name, string? parentUnitId, string? id = null) : base(id)
    {
        OrganizationId = organizationId;
        Rename(name);
        ParentUnitId = parentUnitId;
    }

    public string OrganizationId { get; private set; } = string.Empty;
    public string? ParentUnitId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public OrganizationalUnitStatus Status { get; private set; } = OrganizationalUnitStatus.Active;
    public DateOnly EffectiveFrom { get; private set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? EffectiveUntil { get; private set; }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(DateOnly effectiveUntil)
    {
        if (Status != OrganizationalUnitStatus.Active)
        {
            throw new DomainException("organization.unit.inactive", "The organizational unit is already inactive.");
        }

        if (effectiveUntil < EffectiveFrom)
        {
            throw new DomainException("organization.unit.invalid_period", "The end date cannot precede the start date.");
        }

        Status = OrganizationalUnitStatus.Archived;
        EffectiveUntil = effectiveUntil;
        UpdatedAt = DateTime.UtcNow;
    }
}
