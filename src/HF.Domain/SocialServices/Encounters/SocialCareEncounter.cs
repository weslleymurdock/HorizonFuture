using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Events;

namespace HF.Domain.SocialServices.Encounters;

public sealed class SocialCareEncounter : AggregateRoot
{
    private readonly List<string> _evidenceIds = [];

    private SocialCareEncounter() { }

    public SocialCareEncounter(
        string beneficiaryPersonId,
        string organizationalUnitId,
        string attendantPersonId,
        EncounterType encounterType,
        DateTime scheduledDateTime,
        string? programId = null,
        string? initialNotes = null,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationalUnitId);
        ArgumentException.ThrowIfNullOrWhiteSpace(attendantPersonId);

        BeneficiaryPersonId = beneficiaryPersonId.Trim();
        OrganizationalUnitId = organizationalUnitId.Trim();
        AttendantPersonId = attendantPersonId.Trim();
        EncounterType = encounterType;
        ScheduledDateTime = scheduledDateTime;
        ProgramId = string.IsNullOrWhiteSpace(programId) ? null : programId.Trim();
        OperationalSummary = string.IsNullOrWhiteSpace(initialNotes) ? null : initialNotes.Trim();
        Status = EncounterStatus.Scheduled;

        AddDomainEvent(new SocialCareEncounterScheduledEvent(Id, BeneficiaryPersonId, ProgramId, ScheduledDateTime));
    }

    public string BeneficiaryPersonId { get; private set; } = string.Empty;
    public string? ProgramId { get; private set; }
    public string OrganizationalUnitId { get; private set; } = string.Empty;
    public string AttendantPersonId { get; private set; } = string.Empty;
    public EncounterType EncounterType { get; private set; }
    public EncounterStatus Status { get; private set; }
    public DateTime ScheduledDateTime { get; private set; }
    public DateTime? EncounterDateTime { get; private set; }
    public string? OperationalSummary { get; private set; }
    public SensitiveCareNotes? SensitiveNotes { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? CancelledByPersonId { get; private set; }
    public string? NoShowRecordedByPersonId { get; private set; }

    public IReadOnlyList<string> EvidenceIds => _evidenceIds.AsReadOnly();

    public void Complete(
        DateTime encounterDateTime,
        string attendantPersonId,
        string operationalSummary,
        SensitiveCareNotes? sensitiveNotes = null)
    {
        if (Status == EncounterStatus.Completed)
        {
            throw new DomainException("encounter.already_completed", "The encounter has already been completed.");
        }

        if (Status is EncounterStatus.Cancelled or EncounterStatus.NoShow)
        {
            throw new DomainException("encounter.cannot_complete", $"Cannot complete an encounter in status '{Status}'.");
        }

        if (encounterDateTime > DateTime.UtcNow.AddMinutes(5))
        {
            throw new DomainException("encounter.future_date", "Encounter completion timestamp cannot be in the future.");
        }

        if (string.IsNullOrWhiteSpace(operationalSummary))
        {
            throw new DomainException("encounter.summary_required", "An operational summary is required when completing an encounter.");
        }

        if (string.IsNullOrWhiteSpace(attendantPersonId))
        {
            throw new ArgumentException("AttendantPersonId cannot be blank.", nameof(attendantPersonId));
        }

        EncounterDateTime = encounterDateTime;
        AttendantPersonId = attendantPersonId.Trim();
        OperationalSummary = operationalSummary.Trim();
        SensitiveNotes = sensitiveNotes;
        Status = EncounterStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SocialCareEncounterCompletedEvent(Id, BeneficiaryPersonId, AttendantPersonId, EncounterDateTime.Value));
    }

    public void Cancel(string reason, string cancelledBy)
    {
        if (Status == EncounterStatus.Cancelled)
        {
            throw new DomainException("encounter.already_cancelled", "The encounter is already cancelled.");
        }

        if (Status == EncounterStatus.Completed)
        {
            throw new DomainException("encounter.cannot_cancel_completed", "A completed encounter cannot be cancelled.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("encounter.cancellation_reason_required", "A reason is required to cancel an encounter.");
        }

        if (string.IsNullOrWhiteSpace(cancelledBy))
        {
            throw new ArgumentException("CancelledBy cannot be blank.", nameof(cancelledBy));
        }

        Status = EncounterStatus.Cancelled;
        CancellationReason = reason.Trim();
        CancelledByPersonId = cancelledBy.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SocialCareEncounterCancelledEvent(Id, CancellationReason, CancelledByPersonId));
    }

    public void RecordNoShow(string recordedBy)
    {
        if (Status is EncounterStatus.Completed or EncounterStatus.Cancelled)
        {
            throw new DomainException("encounter.cannot_record_noshow", $"Cannot mark an encounter as no-show when in status '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(recordedBy))
        {
            throw new ArgumentException("RecordedBy cannot be blank.", nameof(recordedBy));
        }

        Status = EncounterStatus.NoShow;
        NoShowRecordedByPersonId = recordedBy.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AttachEvidence(string evidenceId)
    {
        if (Status == EncounterStatus.Cancelled)
        {
            throw new DomainException("encounter.cannot_modify_cancelled", "Cannot attach evidence to a cancelled encounter.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceId);

        if (!_evidenceIds.Contains(evidenceId.Trim()))
        {
            _evidenceIds.Add(evidenceId.Trim());
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
