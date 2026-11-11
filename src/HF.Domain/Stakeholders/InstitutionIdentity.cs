using HF.Domain.Common.Aggregates;

namespace HF.Domain.Stakeholders;

public sealed class InstitutionIdentity : AggregateRoot
{
    private InstitutionIdentity() { }

    public InstitutionIdentity(string legalName, string? registrationIdentifier = null, string? id = null) : base(id)
    {
        LegalName = Require(legalName);
        RegistrationIdentifier = NormalizeIdentifier(registrationIdentifier);
        Status = IdentityStatus.Active;
    }

    public string LegalName { get; private set; } = string.Empty;
    public string? TradeName { get; private set; }
    public string? RegistrationIdentifier { get; private set; }
    public IdentityStatus Status { get; private set; }

    public void UpdateProfile(string legalName, string? tradeName, string? registrationIdentifier)
    {
        if (Status != IdentityStatus.Active)
        {
            throw new InvalidOperationException("The institution identity is not active.");
        }

        LegalName = Require(legalName);
        TradeName = string.IsNullOrWhiteSpace(tradeName) ? null : tradeName.Trim();
        RegistrationIdentifier = NormalizeIdentifier(registrationIdentifier);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = IdentityStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Require(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }

    private static string? NormalizeIdentifier(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
}
