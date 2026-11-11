using HF.Domain.Common.Exceptions;
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
        if (string.IsNullOrWhiteSpace(assessedBy))
        {
            throw new ArgumentException("AssessedBy cannot be blank.", nameof(assessedBy));
        }

        var criteriaList = evaluatedCriteria.ToList().AsReadOnly();
        var mandatoryCriteriaMet = criteriaList.All(c => !c.IsMandatory || c.IsMet);

        if (isEligible != mandatoryCriteriaMet)
        {
            throw new DomainException(
                "eligibility.contradictory_result",
                $"Contradictory eligibility evaluation: IsEligible is {isEligible}, but mandatory criteria satisfaction is {mandatoryMet(mandatoryCriteriaMet)}.");
        }

        IsEligible = isEligible;
        EvaluatedCriteria = criteriaList;
        AssessedOn = assessedOn;
        AssessedBy = assessedBy.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public static EligibilityAssessmentResult Create(
        IEnumerable<CriterionEvaluationResult> evaluatedCriteria,
        DateOnly assessedOn,
        string assessedBy,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(evaluatedCriteria);
        var criteriaList = evaluatedCriteria.ToList();
        var isEligible = criteriaList.All(c => !c.IsMandatory || c.IsMet);

        return new EligibilityAssessmentResult(isEligible, criteriaList, assessedOn, assessedBy, notes);
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

    private static string mandatoryMet(bool value) => value ? "satisfied" : "not satisfied";
}
