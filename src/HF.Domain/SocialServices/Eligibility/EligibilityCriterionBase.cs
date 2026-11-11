using HF.Domain.Common.ValueObjects;

namespace HF.Domain.SocialServices.Eligibility;

public abstract class EligibilityCriterionBase : ValueObject, IEligibilityCriterion
{
    protected EligibilityCriterionBase(string code, string name, string description, bool isMandatory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        IsMandatory = isMandatory;
    }

    public string Code { get; }
    public string Name { get; }
    public string Description { get; }
    public bool IsMandatory { get; }

    public abstract CriterionEvaluationResult Evaluate(ApplicantProfile profile, DateOnly referenceDate);
}
