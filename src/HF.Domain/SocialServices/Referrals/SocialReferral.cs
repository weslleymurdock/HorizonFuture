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

    public void Dispatch(DateOnly? dispatchedDate = null, string? dispatchedByPersonId = null)
    {
        if (Status != ReferralStatus.Pending)
        {
            throw new DomainException("referral.invalid_transition", $"Cannot dispatch a referral in status '{Status}'.");
        }

        var effectiveDispatchedDate = dispatchedDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        if (effectiveDispatchedDate < ReferralDate)
        {
            throw new DomainException("referral.invalid_dispatch_date", "Dispatch date cannot be earlier than referral date.");
        }

        DispatchedDate = effectiveDispatchedDate;
        DispatchedByPersonId = string.IsNullOrWhiteSpace(dispatchedByPersonId) ? null : dispatchedByPersonId.Trim();
        Status = ReferralStatus.Dispatched;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AcknowledgeReceipt(DateOnly? acknowledgedDate = null, string? acknowledgedBy = null)
    {
        if (Status != ReferralStatus.Pending && Status != ReferralStatus.Dispatched)
        {
            throw new DomainException("referral.invalid_transition", $"Cannot acknowledge referral in status '{Status}'.");
        }

        var effectiveDate = acknowledgedDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        if (DispatchedDate.HasValue && effectiveDate < DispatchedDate.Value)
        {
            throw new DomainException("referral.invalid_acknowledgement_date", "Acknowledgement date cannot be earlier than dispatch date.");
        }

        AcknowledgedDate = effectiveDate;
        AcknowledgedBy = string.IsNullOrWhiteSpace(acknowledgedBy) ? null : acknowledgedBy.Trim();
        Status = ReferralStatus.Acknowledged;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordOutcome(
        DateOnly outcomeDate,
        string outcomeNotes,
        bool isSuccessful,
        string? recordedByPersonId = null)
    {
        if (Status == ReferralStatus.Completed)
        {
            throw new DomainException("referral.already_completed", "The referral has already been completed.");
        }

        if (Status == ReferralStatus.Cancelled)
        {
            throw new DomainException("referral.cannot_complete_cancelled", "A cancelled referral cannot receive an outcome.");
        }

        if (outcomeDate < ReferralDate)
        {
            throw new DomainException("referral.invalid_outcome_date", "Outcome date cannot be earlier than referral date.");
        }

        if (string.IsNullOrWhiteSpace(outcomeNotes))
        {
            throw new DomainException("referral.outcome_notes_required", "Notes describing the confirmed outcome are required.");
        }

        Status = ReferralStatus.Completed;
        OutcomeDate = outcomeDate;
        OutcomeNotes = outcomeNotes.Trim();
        IsSuccessful = isSuccessful;
        OutcomeRecordedByPersonId = string.IsNullOrWhiteSpace(recordedByPersonId) ? null : recordedByPersonId.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SocialReferralCompletedEvent(Id, BeneficiaryPersonId, OutcomeDate.Value, IsSuccessful.Value));
    }

    public void Cancel(string reason, string? cancelledBy = null, DateOnly? cancellationDate = null)
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

        Status = ReferralStatus.Cancelled;
        CancellationReason = reason.Trim();
        CancelledByPersonId = string.IsNullOrWhiteSpace(cancelledBy) ? null : cancelledBy.Trim();
        CancellationDate = cancellationDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SocialReferralCancelledEvent(Id, CancellationReason));
    }
}
