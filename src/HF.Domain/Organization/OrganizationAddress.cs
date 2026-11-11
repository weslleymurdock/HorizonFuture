using HF.Domain.Common.ValueObjects;

namespace HF.Domain.Organization;

public sealed class OrganizationAddress : ValueObject
{
    private OrganizationAddress() { }

    public OrganizationAddress(
        string street,
        string number,
        string district,
        string city,
        string state,
        string postalCode,
        string? complement = null)
    {
        Street = Require(street, nameof(street), 200);
        Number = Require(number, nameof(number), 30);
        District = Require(district, nameof(district), 120);
        City = Require(city, nameof(city), 120);
        State = Require(state, nameof(state), 80);
        PostalCode = NormalizePostalCode(postalCode);
        Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();
    }

    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string District { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string? Complement { get; private set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return Number;
        yield return District;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Complement;
    }

    private static string Require(string value, string name, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{name} cannot exceed {maxLength} characters.", name);
        }

        return normalized;
    }

    private static string NormalizePostalCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        var normalized = new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (normalized.Length is < 5 or > 12)
        {
            throw new ArgumentException("Postal code has an invalid length.", nameof(value));
        }

        return normalized;
    }
}
