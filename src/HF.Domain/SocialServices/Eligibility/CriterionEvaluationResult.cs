using HF.Domain.Common.ValueObjects;

namespace HF.Domain.SocialServices.Eligibility;

public sealed class CriterionEvaluationResult : ValueObject
{
    public CriterionEvaluationResult(string criterionCode, string criterionName, bool isMet, string details)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(criterionCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(criterionName);

        CriterionCode = criterionCode.Trim();
        CriterionName = criterionName.Trim();
        IsMet = isMet;
        Details = details?.Trim() ?? string.Empty;
    }

    public string CriterionCode { get; }
    public string CriterionName { get; }
    public bool IsMet { get; }
    public string Details { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CriterionCode;
        yield return CriterionName;
        yield return IsMet;
        yield return Details;
    }
}
