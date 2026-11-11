using HF.Domain.Common.Exceptions;

namespace HF.Domain.SocialServices.Eligibility;

public sealed class AgeRangeEligibilityCriterion : EligibilityCriterionBase
{
    public AgeRangeEligibilityCriterion(
        string code,
        string name,
        int minAge,
        int maxAge,
        bool isMandatory = true,
        string? description = null)
        : base(code, name, description ?? $"Age between {minAge} and {maxAge} years", isMandatory)
    {
        if (minAge < 0)
        {
            throw new DomainException("eligibility.invalid_age", "Minimum age cannot be negative.");
        }

        if (maxAge < minAge)
        {
            throw new DomainException("eligibility.invalid_age_range", "Maximum age cannot be less than minimum age.");
        }

        MinAge = minAge;
        MaxAge = maxAge;
    }

    public int MinAge { get; }
    public int MaxAge { get; }

    public override CriterionEvaluationResult Evaluate(ApplicantProfile profile, DateOnly referenceDate)
    {
        var age = profile.CalculateAgeAt(referenceDate);
        if (!age.HasValue)
        {
            return new CriterionEvaluationResult(
                Code,
                Name,
                isMet: false,
                "Date of birth was not provided for age evaluation.",
                IsMandatory);
        }

        var isMet = age.Value >= MinAge && age.Value <= MaxAge;
        var details = isMet
            ? $"Age {age.Value} is within the required range of {MinAge} to {MaxAge} years."
            : $"Age {age.Value} is outside the required range of {MinAge} to {MaxAge} years.";

        return new CriterionEvaluationResult(Code, Name, isMet, details, IsMandatory);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
        yield return Name;
        yield return IsMandatory;
        yield return MinAge;
        yield return MaxAge;
    }
}
