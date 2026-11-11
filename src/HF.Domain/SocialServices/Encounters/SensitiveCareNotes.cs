using HF.Domain.Common.Exceptions;
using HF.Domain.Common.ValueObjects;
using HF.Domain.SocialServices.Evidences;

namespace HF.Domain.SocialServices.Encounters;

public sealed class SensitiveCareNotes : ValueObject
{
    public SensitiveCareNotes(
        string content,
        string legitimatePurpose,
        string authorPersonId,
        EvidenceSensitivityLevel sensitivityLevel = EvidenceSensitivityLevel.Confidential)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException("care_notes.empty_content", "Care notes content cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(legitimatePurpose))
        {
            throw new DomainException("care_notes.purpose_required", "A legitimate operational purpose is required to record sensitive care notes.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(authorPersonId);

        Content = content.Trim();
        LegitimatePurpose = legitimatePurpose.Trim();
        AuthorPersonId = authorPersonId.Trim();
        SensitivityLevel = sensitivityLevel;
        RecordedAtUtc = DateTime.UtcNow;
    }

    public string Content { get; }
    public string LegitimatePurpose { get; }
    public string AuthorPersonId { get; }
    public EvidenceSensitivityLevel SensitivityLevel { get; }
    public DateTime RecordedAtUtc { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Content;
        yield return LegitimatePurpose;
        yield return AuthorPersonId;
        yield return SensitivityLevel;
    }
}
