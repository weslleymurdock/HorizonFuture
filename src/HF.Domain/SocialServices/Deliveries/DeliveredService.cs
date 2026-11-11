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
        string scheduledByPersonId,
        DateOnly scheduledDate,
        string? description = null,
        string? linkedEncounterId = null,
        ServiceDeliveryStatus initialStatus = ServiceDeliveryStatus.Scheduled,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceDefinitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scheduledByPersonId);

        if (!Enum.IsDefined(initialStatus))
        {
            throw new DomainException("service_delivery.invalid_status", "The specified initial service delivery status is invalid.");
        }

        if (initialStatus is not (ServiceDeliveryStatus.Requested or ServiceDeliveryStatus.Scheduled))
        {
            throw new DomainException(
                "service_delivery.invalid_initial_status",
                $"A service delivery cannot be created directly in status '{initialStatus}'. Use Scheduled, Requested, or RecordCompleted().");
        }

        BeneficiaryPersonId = beneficiaryPersonId.Trim();
        ProgramId = programId.Trim();
        ServiceDefinitionId = serviceDefinitionId.Trim();
        ScheduledByPersonId = scheduledByPersonId.Trim();
        DeliveryDate = scheduledDate;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        LinkedEncounterId = string.IsNullOrWhiteSpace(linkedEncounterId) ? null : linkedEncounterId.Trim();
        Status = initialStatus;
    }

    public static DeliveredService RecordCompleted(
        string beneficiaryPersonId,
        string programId,
        string serviceDefinitionId,
        string deliveredByPersonId,
        DateOnly deliveryDate,
        string? description = null,
        string? linkedEncounterId = null,
        string? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceDefinitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(deliveredByPersonId);

        var service = new DeliveredService
        {
            Id = id ?? Guid.CreateVersion7().ToString(),
            BeneficiaryPersonId = beneficiaryPersonId.Trim(),
            ProgramId = programId.Trim(),
            ServiceDefinitionId = serviceDefinitionId.Trim(),
            ScheduledByPersonId = deliveredByPersonId.Trim(),
            DeliveredByPersonId = deliveredByPersonId.Trim(),
            DeliveryDate = deliveryDate,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            LinkedEncounterId = string.IsNullOrWhiteSpace(linkedEncounterId) ? null : linkedEncounterId.Trim(),
            Status = ServiceDeliveryStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        service.AddDomainEvent(new ServiceDeliveredEvent(
            service.Id,
            service.BeneficiaryPersonId,
            service.ProgramId,
            service.ServiceDefinitionId,
            service.DeliveryDate));

        return service;
    }

    public string BeneficiaryPersonId { get; private set; } = string.Empty;
    public string ProgramId { get; private set; } = string.Empty;
    public string ServiceDefinitionId { get; private set; } = string.Empty;
    public string ScheduledByPersonId { get; private set; } = string.Empty;
    public string? DeliveredByPersonId { get; private set; }
    public DateOnly DeliveryDate { get; private set; }
    public string? Description { get; private set; }
    public string? LinkedEncounterId { get; private set; }
    public ServiceDeliveryStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? CancelledByPersonId { get; private set; }
    public DateOnly? CancelledDate { get; private set; }

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

        if (Status != ServiceDeliveryStatus.Scheduled && Status != ServiceDeliveryStatus.Requested)
        {
            throw new DomainException("service_delivery.invalid_transition", $"Cannot complete a service delivery in status '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(deliveredByPersonId))
        {
            throw new ArgumentException("DeliveredByPersonId cannot be blank.", nameof(deliveredByPersonId));
        }

        if (completionDate < DeliveryDate)
        {
            throw new DomainException("service_delivery.invalid_completion_date", "Completion date cannot be earlier than scheduled date.");
        }

        DeliveryDate = completionDate;
        DeliveredByPersonId = deliveredByPersonId.Trim();
        Status = ServiceDeliveryStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceDeliveredEvent(Id, BeneficiaryPersonId, ProgramId, ServiceDefinitionId, DeliveryDate));
    }

    public void Cancel(string reason, string cancelledBy, DateOnly? cancellationDate = null)
    {
        if (Status == ServiceDeliveryStatus.Cancelled)
        {
            throw new DomainException("service_delivery.already_cancelled", "The service delivery is already cancelled.");
        }

        if (Status == ServiceDeliveryStatus.Completed)
        {
            throw new DomainException("service_delivery.cannot_cancel_completed", "A completed service delivery cannot be cancelled.");
        }

        if (Status != ServiceDeliveryStatus.Scheduled && Status != ServiceDeliveryStatus.Requested)
        {
            throw new DomainException("service_delivery.invalid_transition", $"Cannot cancel a service delivery in status '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("service_delivery.cancellation_reason_required", "A reason is required to cancel a service delivery.");
        }

        if (string.IsNullOrWhiteSpace(cancelledBy))
        {
            throw new ArgumentException("CancelledBy cannot be blank.", nameof(cancelledBy));
        }

        Status = ServiceDeliveryStatus.Cancelled;
        CancellationReason = reason.Trim();
        CancelledByPersonId = cancelledBy.Trim();
        CancelledDate = cancellationDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(DateOnly cancellationDate, string reason, string cancelledBy) =>
        Cancel(reason, cancelledBy, cancellationDate);

    public void AttachEvidence(string evidenceId)
    {
        if (Status == ServiceDeliveryStatus.Cancelled)
        {
            throw new DomainException("service_delivery.cannot_modify_cancelled", "Cannot attach evidence to a cancelled service delivery.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceId);

        if (!_evidenceIds.Contains(evidenceId.Trim()))
        {
            _evidenceIds.Add(evidenceId.Trim());
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
