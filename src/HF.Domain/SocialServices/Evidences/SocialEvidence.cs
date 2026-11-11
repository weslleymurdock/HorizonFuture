using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.SocialServices.Evidences;

public sealed class SocialEvidence : AggregateRoot
{
    private SocialEvidence() { }

    public SocialEvidence(
        EvidenceType evidenceType,
        string storageReference,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string uploadedByPersonId,
        string? description = null,
        EvidenceSensitivityLevel sensitivityLevel = EvidenceSensitivityLevel.Standard,
        string? associatedEntityType = null,
        string? associatedEntityId = null,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(uploadedByPersonId);

        if (!Enum.IsDefined(evidenceType))
        {
            throw new DomainException("evidence.invalid_type", "The specified evidence type is invalid.");
        }

        if (!Enum.IsDefined(sensitivityLevel))
        {
            throw new DomainException("evidence.invalid_sensitivity_level", "The specified sensitivity level is invalid.");
        }

        if (fileSizeBytes <= 0)
        {
            throw new DomainException("evidence.invalid_size", "File size in bytes must be positive.");
        }

        EvidenceType = evidenceType;
        StorageReference = storageReference.Trim();
        FileName = fileName.Trim();
        ContentType = contentType.Trim();
        FileSizeBytes = fileSizeBytes;
        UploadedByPersonId = uploadedByPersonId.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SensitivityLevel = sensitivityLevel;
        AssociatedEntityType = string.IsNullOrWhiteSpace(associatedEntityType) ? null : associatedEntityType.Trim();
        AssociatedEntityId = string.IsNullOrWhiteSpace(associatedEntityId) ? null : associatedEntityId.Trim();
        UploadedAtUtc = DateTime.UtcNow;
    }

    public EvidenceType EvidenceType { get; private set; }
    public string StorageReference { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public DateTime UploadedAtUtc { get; private set; }
    public string UploadedByPersonId { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public EvidenceSensitivityLevel SensitivityLevel { get; private set; }
    public string? AssociatedEntityType { get; private set; }
    public string? AssociatedEntityId { get; private set; }

    public void LinkToEntity(string entityType, string entityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);

        AssociatedEntityType = entityType.Trim();
        AssociatedEntityId = entityId.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSensitivity(EvidenceSensitivityLevel newLevel)
    {
        if (!Enum.IsDefined(newLevel))
        {
            throw new DomainException("evidence.invalid_sensitivity_level", "The specified sensitivity level is invalid.");
        }

        SensitivityLevel = newLevel;
        UpdatedAt = DateTime.UtcNow;
    }
}
