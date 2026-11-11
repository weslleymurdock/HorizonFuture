using HF.Domain.Common.Entities;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.SocialServices.Programs;

public sealed class ServiceOffering : EntityBase
{
    private ServiceOffering() { }

    public ServiceOffering(
        string code,
        string name,
        string? description = null,
        string? category = null,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        IsActive = true;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Category { get; private set; }
    public bool IsActive { get; private set; }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new DomainException("service_offering.already_inactive", "The service offering is already inactive.");
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (IsActive)
        {
            throw new DomainException("service_offering.already_active", "The service offering is already active.");
        }

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
