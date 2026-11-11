using HF.Domain.Common.ValueObjects;

namespace HF.Domain.SocialServices.Eligibility;

public sealed class EligibilityAssessmentResult : ValueObject
{
    public EligibilityAssessmentResult(
        bool isEligible,
        IEnumerable<CriterionEvaluationResult> evaluatedCriteria,
        DateOnly assessedOn,
        string assessedBy,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(evaluatedCriteria);
        ArgumentException.ThrowIfNullOrWhiteSpace(assessedBy);

        IsEligible = isEligible;
        EvaluatedCriteria = evaluatedCriteria.ToList().AsReadOnly();
        AssessedOn = assessedOn;
        AssessedBy = assessedBy.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public bool IsEligible { get; }
    public IReadOnlyList<CriterionEvaluationResult> EvaluatedCriteria { get; }
    public DateOnly AssessedOn { get; }
    public string AssessedBy { get; }
    public string? Notes { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return IsEligible;
        yield return AssessedOn;
        yield return AssessedBy;
        yield return Notes;
        foreach (var criterion in EvaluatedCriteria)
        {
            yield return criterion;
        }
    }
}
