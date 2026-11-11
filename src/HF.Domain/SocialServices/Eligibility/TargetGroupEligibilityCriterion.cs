namespace HF.Domain.SocialServices.Eligibility;

public sealed class TargetGroupEligibilityCriterion : EligibilityCriterionBase
{
    public TargetGroupEligibilityCriterion(
        string code,
        string name,
        string requiredGroup,
        bool isMandatory = true,
        string? description = null)
        : base(code, name, description ?? $"Target group: {requiredGroup}", isMandatory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredGroup);
        RequiredGroup = requiredGroup.Trim().ToUpperInvariant();
    }

    public string RequiredGroup { get; }

    public override CriterionEvaluationResult Evaluate(ApplicantProfile profile, DateOnly referenceDate)
    {
        var isMet = profile.TargetGroups.Contains(RequiredGroup, StringComparer.OrdinalIgnoreCase);
        var details = isMet
            ? $"Applicant declared required target group '{RequiredGroup}'."
            : $"Applicant does not possess required target group '{RequiredGroup}'.";

        return new CriterionEvaluationResult(Code, Name, isMet, details, IsMandatory);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
        yield return Name;
        yield return IsMandatory;
        yield return RequiredGroup;
    }
}
