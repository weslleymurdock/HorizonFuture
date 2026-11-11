using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Stakeholders;

public sealed class Person : AggregateRoot
{
    private Person() { }

    public Person(string fullName, string? id = null) : base(id)
    {
        FullName = NormalizeName(fullName);
        Status = IdentityStatus.Active;
    }

    public string FullName { get; private set; } = string.Empty;
    public string? SocialName { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public IdentityStatus Status { get; private set; }

    public void UpdateProfile(string fullName, string? socialName, DateOnly? dateOfBirth)
    {
        EnsureActive();
        FullName = NormalizeName(fullName);
        SocialName = NormalizeOptional(socialName);
        DateOfBirth = dateOfBirth;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = IdentityStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (Status != IdentityStatus.Active)
        {
            throw new DomainException("identity.inactive", "The identity is not active.");
        }
    }

    private static string NormalizeName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
