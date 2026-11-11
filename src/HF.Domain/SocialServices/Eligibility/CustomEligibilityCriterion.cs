namespace HF.Domain.SocialServices.Eligibility;

public sealed class CustomEligibilityCriterion : EligibilityCriterionBase
{
    public CustomEligibilityCriterion(
        string code,
        string name,
        string key,
        string expectedValue,
        bool isMandatory = true,
        string? description = null)
        : base(code, name, description ?? $"Requirement: {key} must equal '{expectedValue}'", isMandatory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedValue);

        Key = key.Trim();
        ExpectedValue = expectedValue.Trim();
    }

    public string Key { get; }
    public string ExpectedValue { get; }

    public override CriterionEvaluationResult Evaluate(ApplicantProfile profile, DateOnly referenceDate)
    {
        if (!profile.CustomAttributes.TryGetValue(Key, out var actualValue) || string.IsNullOrWhiteSpace(actualValue))
        {
            return new CriterionEvaluationResult(
                Code,
                Name,
                isMet: false,
                $"Required attribute '{Key}' was not provided.",
                IsMandatory);
        }

        var isMet = string.Equals(actualValue.Trim(), ExpectedValue, StringComparison.OrdinalIgnoreCase);
        var details = isMet
            ? $"Attribute '{Key}' matches expected value."
            : $"Attribute '{Key}' value '{actualValue}' did not match expected '{ExpectedValue}'.";

        return new CriterionEvaluationResult(Code, Name, isMet, details, IsMandatory);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
        yield return Name;
        yield return IsMandatory;
        yield return Key;
        yield return ExpectedValue;
    }
}
