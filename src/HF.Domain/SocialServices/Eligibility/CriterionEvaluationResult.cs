using HF.Domain.Common.ValueObjects;

namespace HF.Domain.SocialServices.Eligibility;

public sealed class CriterionEvaluationResult : ValueObject
{
    public CriterionEvaluationResult(
        string criterionCode,
        string criterionName,
        bool isMet,
        string details,
        bool isMandatory = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(criterionCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(criterionName);

        CriterionCode = criterionCode.Trim();
        CriterionName = criterionName.Trim();
        IsMet = isMet;
        Details = details?.Trim() ?? string.Empty;
        IsMandatory = isMandatory;
    }

    public string CriterionCode { get; }
    public string CriterionName { get; }
    public bool IsMet { get; }
    public string Details { get; }
    public bool IsMandatory { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CriterionCode;
        yield return CriterionName;
        yield return IsMet;
        yield return Details;
        yield return IsMandatory;
    }
}
