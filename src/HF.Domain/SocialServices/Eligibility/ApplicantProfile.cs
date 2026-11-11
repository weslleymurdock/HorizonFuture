using HF.Domain.Common.ValueObjects;

namespace HF.Domain.SocialServices.Eligibility;

public sealed class ApplicantProfile : ValueObject
{
    public ApplicantProfile(
        DateOnly? dateOfBirth = null,
        string? territory = null,
        IEnumerable<string>? targetGroups = null,
        IEnumerable<string>? submittedDocuments = null,
        IReadOnlyDictionary<string, string>? customAttributes = null)
    {
        DateOfBirth = dateOfBirth;
        Territory = string.IsNullOrWhiteSpace(territory) ? null : territory.Trim();
        TargetGroups = (targetGroups ?? [])
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(g => g.Trim().ToUpperInvariant())
            .Distinct()
            .OrderBy(g => g)
            .ToList()
            .AsReadOnly();
        SubmittedDocuments = (submittedDocuments ?? [])
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d.Trim().ToUpperInvariant())
            .Distinct()
            .OrderBy(d => d)
            .ToList()
            .AsReadOnly();
        CustomAttributes = customAttributes != null
            ? new Dictionary<string, string>(customAttributes, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public DateOnly? DateOfBirth { get; }
    public string? Territory { get; }
    public IReadOnlyList<string> TargetGroups { get; }
    public IReadOnlyList<string> SubmittedDocuments { get; }
    public IReadOnlyDictionary<string, string> CustomAttributes { get; }

    public int? CalculateAgeAt(DateOnly referenceDate)
    {
        if (!DateOfBirth.HasValue)
        {
            return null;
        }

        var birth = DateOfBirth.Value;
        var age = referenceDate.Year - birth.Year;
        if (referenceDate < birth.AddYears(age))
        {
            age--;
        }

        return age;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DateOfBirth;
        yield return Territory;
        foreach (var group in TargetGroups)
        {
            yield return group;
        }
        foreach (var doc in SubmittedDocuments)
        {
            yield return doc;
        }
        foreach (var kvp in CustomAttributes.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
        {
            yield return kvp.Key;
            yield return kvp.Value;
        }
    }
}
