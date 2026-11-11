using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Events;

namespace HF.Domain.SocialServices.Referrals;

public sealed class SocialReferral : AggregateRoot
{
    private SocialReferral() { }

    public SocialReferral(
        string beneficiaryPersonId,
        ReferralDestinationType destinationType,
        string destinationName,
        string reason,
        DateOnly referralDate,
        string referredByPersonId,
        string? sourceEncounterId = null,
        string? programId = null,
        string? destinationDepartmentId = null,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationName);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentException.ThrowIfNullOrWhiteSpace(referredByPersonId);

        if (!Enum.IsDefined(destinationType))
        {
            throw new DomainException("referral.invalid_destination_type", "The specified destination type is invalid.");
        }

        BeneficiaryPersonId = beneficiaryPersonId.Trim();
        DestinationType = destinationType;
        DestinationName = destinationName.Trim();
        Reason = reason.Trim();
        ReferralDate = referralDate;
        ReferredByPersonId = referredByPersonId.Trim();
        SourceEncounterId = string.IsNullOrWhiteSpace(sourceEncounterId) ? null : sourceEncounterId.Trim();
        ProgramId = string.IsNullOrWhiteSpace(programId) ? null : programId.Trim();
        DestinationDepartmentId = string.IsNullOrWhiteSpace(destinationDepartmentId) ? null : destinationDepartmentId.Trim();
        Status = ReferralStatus.Pending;

        AddDomainEvent(new SocialReferralCreatedEvent(Id, BeneficiaryPersonId, DestinationName, ReferralDate));
    }

    public string BeneficiaryPersonId { get; private set; } = string.Empty;
    public ReferralDestinationType DestinationType { get; private set; }
    public string DestinationName { get; private set; } = string.Empty;
    public string? DestinationDepartmentId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateOnly ReferralDate { get; private set; }
    public string ReferredByPersonId { get; private set; } = string.Empty;
    public string? SourceEncounterId { get; private set; }
    public string? ProgramId { get; private set; }
    public ReferralStatus Status { get; private set; }

    public DateOnly? DispatchedDate { get; private set; }
    public string? DispatchedByPersonId { get; private set; }

    public DateOnly? AcknowledgedDate { get; private set; }
    public string? AcknowledgedBy { get; private set; }

    public DateOnly? OutcomeDate { get; private set; }
    public string? OutcomeNotes { get; private set; }
    public bool? IsSuccessful { get; private set; }
    public string? OutcomeRecordedByPersonId { get; private set; }

    public string? CancellationReason { get; private set; }
    public string? CancelledByPersonId { get; private set; }
    public DateOnly? CancellationDate { get; private set; }

    public void Dispatch(DateOnly dispatchedDate, string dispatchedByPersonId)
    {
        if (Status == ReferralStatus.Completed)
        {
            throw new DomainException("referral.cannot_dispatch_completed", "A completed referral cannot be dispatched.");
        }

        if (Status == ReferralStatus.Cancelled)
        {
            throw new DomainException("referral.cannot_dispatch_cancelled", "A cancelled referral cannot be dispatched.");
        }

        if (Status != ReferralStatus.Pending)
        {
            throw new DomainException("referral.invalid_transition", $"Cannot dispatch a referral in status '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(dispatchedByPersonId))
        {
            throw new ArgumentException("DispatchedByPersonId cannot be blank.", nameof(dispatchedByPersonId));
        }

        if (dispatchedDate < ReferralDate)
        {
            throw new DomainException("referral.invalid_dispatch_date", "Dispatch date cannot be earlier than referral date.");
        }

        DispatchedDate = dispatchedDate;
        DispatchedByPersonId = dispatchedByPersonId.Trim();
        Status = ReferralStatus.Dispatched;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AcknowledgeReceipt(DateOnly acknowledgedDate, string acknowledgedBy)
    {
        if (Status == ReferralStatus.Completed)
        {
            throw new DomainException("referral.cannot_acknowledge_completed", "A completed referral cannot be acknowledged.");
        }

        if (Status == ReferralStatus.Cancelled)
        {
            throw new DomainException("referral.cannot_acknowledge_cancelled", "A cancelled referral cannot be acknowledged.");
        }

        if (Status != ReferralStatus.Pending && Status != ReferralStatus.Dispatched)
        {
            throw new DomainException("referral.invalid_transition", $"Cannot acknowledge referral in status '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(acknowledgedBy))
        {
            throw new ArgumentException("AcknowledgedBy cannot be blank.", nameof(acknowledgedBy));
        }

        if (acknowledgedDate < ReferralDate)
        {
            throw new DomainException("referral.invalid_acknowledgement_date", "Acknowledgement date cannot be earlier than referral date.");
        }

        if (DispatchedDate.HasValue && acknowledgedDate < DispatchedDate.Value)
        {
            throw new DomainException("referral.invalid_acknowledgement_date", "Acknowledgement date cannot be earlier than dispatch date.");
        }

        AcknowledgedDate = acknowledgedDate;
        AcknowledgedBy = acknowledgedBy.Trim();
        Status = ReferralStatus.Acknowledged;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordOutcome(
        DateOnly outcomeDate,
        string outcomeNotes,
        bool isSuccessful,
        string recordedByPersonId)
    {
        if (Status == ReferralStatus.Completed)
        {
            throw new DomainException("referral.already_completed", "The referral has already been completed.");
        }

        if (Status == ReferralStatus.Cancelled)
        {
            throw new DomainException("referral.cannot_complete_cancelled", "A cancelled referral cannot receive an outcome.");
        }

        if (string.IsNullOrWhiteSpace(recordedByPersonId))
        {
            throw new ArgumentException("RecordedByPersonId cannot be blank.", nameof(recordedByPersonId));
        }

        if (string.IsNullOrWhiteSpace(outcomeNotes))
        {
            throw new DomainException("referral.outcome_notes_required", "Notes describing the confirmed outcome are required.");
        }

        if (outcomeDate < ReferralDate)
        {
            throw new DomainException("referral.invalid_outcome_date", "Outcome date cannot be earlier than referral date.");
        }

        if (DispatchedDate.HasValue && outcomeDate < DispatchedDate.Value)
        {
            throw new DomainException("referral.invalid_outcome_date", "Outcome date cannot be earlier than dispatch date.");
        }

        if (AcknowledgedDate.HasValue && outcomeDate < AcknowledgedDate.Value)
        {
            throw new DomainException("referral.invalid_outcome_date", "Outcome date cannot be earlier than acknowledgement date.");
        }

        Status = ReferralStatus.Completed;
        OutcomeDate = outcomeDate;
        OutcomeNotes = outcomeNotes.Trim();
        IsSuccessful = isSuccessful;
        OutcomeRecordedByPersonId = recordedByPersonId.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SocialReferralCompletedEvent(Id, BeneficiaryPersonId, OutcomeDate.Value, IsSuccessful.Value));
    }

    public void Cancel(DateOnly cancellationDate, string reason, string cancelledBy)
    {
        if (Status == ReferralStatus.Cancelled)
        {
            throw new DomainException("referral.already_cancelled", "The referral is already cancelled.");
        }

        if (Status == ReferralStatus.Completed)
        {
            throw new DomainException("referral.cannot_cancel_completed", "A completed referral cannot be cancelled.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("referral.cancellation_reason_required", "A reason is required to cancel a referral.");
        }

        if (string.IsNullOrWhiteSpace(cancelledBy))
        {
            throw new ArgumentException("CancelledBy cannot be blank.", nameof(cancelledBy));
        }

        if (cancellationDate < ReferralDate)
        {
            throw new DomainException("referral.invalid_cancellation_date", "Cancellation date cannot be earlier than referral date.");
        }

        Status = ReferralStatus.Cancelled;
        CancellationReason = reason.Trim();
        CancelledByPersonId = cancelledBy.Trim();
        CancellationDate = cancellationDate;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SocialReferralCancelledEvent(Id, CancellationReason));
    }
}
