namespace HF.Domain.SocialServices.Eligibility;

public interface IEligibilityCriterion
{
    string Code { get; }
    string Name { get; }
    string Description { get; }
    bool IsMandatory { get; }
    CriterionEvaluationResult Evaluate(ApplicantProfile profile, DateOnly referenceDate);
}
