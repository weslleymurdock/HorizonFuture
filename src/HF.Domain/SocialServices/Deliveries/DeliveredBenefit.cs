using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Events;

namespace HF.Domain.SocialServices.Deliveries;

public sealed class DeliveredBenefit : AggregateRoot
{
    private DeliveredBenefit() { }

    public DeliveredBenefit(
        string beneficiaryPersonId,
        string programId,
        BenefitType benefitType,
        string description,
        BenefitQuantity quantity,
        DateOnly grantedDate,
        string grantedByPersonId,
        string? projectId = null,
        string? fundingSourceId = null,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(quantity);
        ArgumentException.ThrowIfNullOrWhiteSpace(grantedByPersonId);

        BeneficiaryPersonId = beneficiaryPersonId.Trim();
        ProgramId = programId.Trim();
        BenefitType = benefitType;
        Description = description.Trim();
        Quantity = quantity;
        GrantedDate = grantedDate;
        GrantedByPersonId = grantedByPersonId.Trim();
        Status = BenefitDeliveryStatus.Granted;
        ProjectId = string.IsNullOrWhiteSpace(projectId) ? null : projectId.Trim();
        FundingSourceId = string.IsNullOrWhiteSpace(fundingSourceId) ? null : fundingSourceId.Trim();

        AddDomainEvent(new BenefitGrantedEvent(Id, BeneficiaryPersonId, ProgramId, GrantedDate));
    }

    public string BeneficiaryPersonId { get; private set; } = string.Empty;
    public string ProgramId { get; private set; } = string.Empty;
    public BenefitType BenefitType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public BenefitQuantity Quantity { get; private set; } = null!;
    public BenefitDeliveryStatus Status { get; private set; }
    public DateOnly GrantedDate { get; private set; }
    public string GrantedByPersonId { get; private set; } = string.Empty;
    public DateOnly? DeliveredDate { get; private set; }
    public string? DeliveredByPersonId { get; private set; }
    public string? ReceivedByPersonId { get; private set; }
    public string? DeliveryReceiptEvidenceId { get; private set; }
    public string? RevocationReason { get; private set; }
    public string? ReturnReason { get; private set; }
    public string? ProjectId { get; private set; }
    public string? FundingSourceId { get; private set; }

    public void Dispatch(DateOnly dispatchDate, string dispatchedByPersonId)
    {
        if (Status != BenefitDeliveryStatus.Granted)
        {
            throw new DomainException("benefit.invalid_state_for_dispatch", $"Cannot dispatch benefit in state '{Status}'.");
        }

        if (dispatchDate < GrantedDate)
        {
            throw new DomainException("benefit.invalid_dispatch_date", "Dispatch date cannot be earlier than grant date.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(dispatchedByPersonId);

        Status = BenefitDeliveryStatus.Dispatched;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered(
        DateOnly deliveryDate,
        string deliveredByPersonId,
        string receivedByPersonId,
        string? deliveryReceiptEvidenceId = null)
    {
        if (Status == BenefitDeliveryStatus.Delivered)
        {
            throw new DomainException("benefit.already_delivered", "The benefit has already been delivered.");
        }

        if (Status != BenefitDeliveryStatus.Granted && Status != BenefitDeliveryStatus.Dispatched)
        {
            throw new DomainException("benefit.invalid_state_for_delivery", $"Cannot deliver benefit in state '{Status}'.");
        }

        if (deliveryDate < GrantedDate)
        {
            throw new DomainException("benefit.invalid_delivery_date", "Delivery date cannot be earlier than grant date.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(deliveredByPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(receivedByPersonId);

        Status = BenefitDeliveryStatus.Delivered;
        DeliveredDate = deliveryDate;
        DeliveredByPersonId = deliveredByPersonId.Trim();
        ReceivedByPersonId = receivedByPersonId.Trim();
        DeliveryReceiptEvidenceId = string.IsNullOrWhiteSpace(deliveryReceiptEvidenceId) ? null : deliveryReceiptEvidenceId.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BenefitDeliveredEvent(Id, BeneficiaryPersonId, ProgramId, DeliveredDate.Value, DeliveredByPersonId, ReceivedByPersonId));
    }

    public void Revoke(string reason, string revokedByPersonId)
    {
        if (Status == BenefitDeliveryStatus.Delivered)
        {
            throw new DomainException("benefit.cannot_revoke_delivered", "Cannot revoke a benefit that has already been delivered.");
        }

        if (Status is BenefitDeliveryStatus.Revoked or BenefitDeliveryStatus.Returned)
        {
            throw new DomainException("benefit.already_inactive", $"The benefit is already '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("benefit.revocation_reason_required", "A reason is required to revoke a benefit.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(revokedByPersonId);

        Status = BenefitDeliveryStatus.Revoked;
        RevocationReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BenefitRevokedEvent(Id, RevocationReason, revokedByPersonId.Trim()));
    }

    public void Return(DateOnly returnDate, string reason, string returnedByPersonId)
    {
        if (Status != BenefitDeliveryStatus.Delivered)
        {
            throw new DomainException("benefit.cannot_return_undelivered", "Only delivered benefits can be marked as returned.");
        }

        if (DeliveredDate.HasValue && returnDate < DeliveredDate.Value)
        {
            throw new DomainException("benefit.invalid_return_date", "Return date cannot be earlier than delivery date.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("benefit.return_reason_required", "A reason is required to record a benefit return.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(returnedByPersonId);

        Status = BenefitDeliveryStatus.Returned;
        ReturnReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAccountingHooks(string? projectId, string? fundingSourceId)
    {
        ProjectId = string.IsNullOrWhiteSpace(projectId) ? null : projectId.Trim();
        FundingSourceId = string.IsNullOrWhiteSpace(fundingSourceId) ? null : fundingSourceId.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
