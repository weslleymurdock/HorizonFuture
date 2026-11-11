using HF.Domain.Common.Entities;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Organization;

public sealed class Department : EntityBase
{
    private Department() { }

    internal Department(string organizationId, string name, string? unitId, string? id = null) : base(id)
    {
        OrganizationId = organizationId;
        UnitId = unitId;
        Rename(name);
    }

    public string OrganizationId { get; private set; } = string.Empty;
    public string? UnitId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    internal void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    internal void Archive()
    {
        if (!IsActive)
        {
            throw new DomainException("organization.department.inactive", "The department is already inactive.");
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
