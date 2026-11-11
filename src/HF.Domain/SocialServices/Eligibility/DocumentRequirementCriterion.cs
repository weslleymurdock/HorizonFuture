namespace HF.Domain.SocialServices.Eligibility;

public sealed class DocumentRequirementCriterion : EligibilityCriterionBase
{
    public DocumentRequirementCriterion(
        string code,
        string name,
        string requiredDocumentType,
        bool isMandatory = true,
        string? description = null)
        : base(code, name, description ?? $"Mandatory document: {requiredDocumentType}", isMandatory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredDocumentType);
        RequiredDocumentType = requiredDocumentType.Trim().ToUpperInvariant();
    }

    public string RequiredDocumentType { get; }

    public override CriterionEvaluationResult Evaluate(ApplicantProfile profile, DateOnly referenceDate)
    {
        var isMet = profile.SubmittedDocuments.Contains(RequiredDocumentType, StringComparer.OrdinalIgnoreCase);
        var details = isMet
            ? $"Required document '{RequiredDocumentType}' was submitted."
            : $"Required document '{RequiredDocumentType}' was not submitted.";

        return new CriterionEvaluationResult(Code, Name, isMet, details, IsMandatory);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
        yield return Name;
        yield return IsMandatory;
        yield return RequiredDocumentType;
    }
}
