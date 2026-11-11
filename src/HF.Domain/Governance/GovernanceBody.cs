using HF.Domain.Common.Entities;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Governance;

public sealed class GovernanceBody : EntityBase
{
    private GovernanceBody() { }

    public GovernanceBody(string organizationId, string name, string type, string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);
        OrganizationId = organizationId;
        Rename(name);
        Type = Require(type);
        IsActive = true;
    }

    public string OrganizationId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public void Rename(string name)
    {
        Name = Require(name);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Require(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }
}
