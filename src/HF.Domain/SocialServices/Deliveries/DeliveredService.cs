using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Events;

namespace HF.Domain.SocialServices.Deliveries;

public sealed class DeliveredService : AggregateRoot
{
    private readonly List<string> _evidenceIds = [];

    private DeliveredService() { }

    public DeliveredService(
        string beneficiaryPersonId,
        string programId,
        string serviceDefinitionId,
        string deliveredByPersonId,
        DateOnly deliveryDate,
        string? description = null,
        string? linkedEncounterId = null,
        ServiceDeliveryStatus status = ServiceDeliveryStatus.Completed,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceDefinitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(deliveredByPersonId);

        BeneficiaryPersonId = beneficiaryPersonId.Trim();
        ProgramId = programId.Trim();
        ServiceDefinitionId = serviceDefinitionId.Trim();
        DeliveredByPersonId = deliveredByPersonId.Trim();
        DeliveryDate = deliveryDate;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        LinkedEncounterId = string.IsNullOrWhiteSpace(linkedEncounterId) ? null : linkedEncounterId.Trim();
        Status = status;

        if (status == ServiceDeliveryStatus.Completed)
        {
            AddDomainEvent(new ServiceDeliveredEvent(Id, BeneficiaryPersonId, ProgramId, ServiceDefinitionId, DeliveryDate));
        }
    }

    public string BeneficiaryPersonId { get; private set; } = string.Empty;
    public string ProgramId { get; private set; } = string.Empty;
    public string ServiceDefinitionId { get; private set; } = string.Empty;
    public string DeliveredByPersonId { get; private set; } = string.Empty;
    public DateOnly DeliveryDate { get; private set; }
    public string? Description { get; private set; }
    public string? LinkedEncounterId { get; private set; }
    public ServiceDeliveryStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }

    public IReadOnlyList<string> EvidenceIds => _evidenceIds.AsReadOnly();

    public void MarkAsCompleted(DateOnly completionDate, string deliveredByPersonId)
    {
        if (Status == ServiceDeliveryStatus.Completed)
        {
            throw new DomainException("service_delivery.already_completed", "The service delivery is already completed.");
        }

        if (Status == ServiceDeliveryStatus.Cancelled)
        {
            throw new DomainException("service_delivery.cannot_complete_cancelled", "A cancelled service delivery cannot be completed.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(deliveredByPersonId);

        DeliveryDate = completionDate;
        DeliveredByPersonId = deliveredByPersonId.Trim();
        Status = ServiceDeliveryStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceDeliveredEvent(Id, BeneficiaryPersonId, ProgramId, ServiceDefinitionId, DeliveryDate));
    }

    public void Cancel(string reason, string cancelledBy)
    {
        if (Status == ServiceDeliveryStatus.Cancelled)
        {
            throw new DomainException("service_delivery.already_cancelled", "The service delivery is already cancelled.");
        }

        if (Status == ServiceDeliveryStatus.Completed)
        {
            throw new DomainException("service_delivery.cannot_cancel_completed", "A completed service delivery cannot be cancelled.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("service_delivery.cancellation_reason_required", "A reason is required to cancel a service delivery.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cancelledBy);

        Status = ServiceDeliveryStatus.Cancelled;
        CancellationReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AttachEvidence(string evidenceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceId);

        if (!_evidenceIds.Contains(evidenceId.Trim()))
        {
            _evidenceIds.Add(evidenceId.Trim());
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
