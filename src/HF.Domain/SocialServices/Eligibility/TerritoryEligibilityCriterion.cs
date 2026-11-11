namespace HF.Domain.SocialServices.Eligibility;

public sealed class TerritoryEligibilityCriterion : EligibilityCriterionBase
{
    public TerritoryEligibilityCriterion(
        string code,
        string name,
        IEnumerable<string> allowedTerritories,
        bool isMandatory = true,
        string? description = null)
        : base(code, name, description ?? "Residence in designated territory", isMandatory)
    {
        ArgumentNullException.ThrowIfNull(allowedTerritories);

        AllowedTerritories = allowedTerritories
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<string> AllowedTerritories { get; }

    public override CriterionEvaluationResult Evaluate(ApplicantProfile profile, DateOnly referenceDate)
    {
        if (string.IsNullOrWhiteSpace(profile.Territory))
        {
            return new CriterionEvaluationResult(
                Code,
                Name,
                isMet: false,
                "Applicant territory was not provided.",
                IsMandatory);
        }

        var isMet = AllowedTerritories.Contains(profile.Territory.Trim(), StringComparer.OrdinalIgnoreCase);
        var details = isMet
            ? $"Applicant territory '{profile.Territory}' is covered."
            : $"Applicant territory '{profile.Territory}' is not within eligible territories ({string.Join(", ", AllowedTerritories)}).";

        return new CriterionEvaluationResult(Code, Name, isMet, details, IsMandatory);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
        yield return Name;
        yield return IsMandatory;
        foreach (var territory in AllowedTerritories)
        {
            yield return territory;
        }
    }
}
