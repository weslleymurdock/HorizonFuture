using HF.Domain.Common.Events;

namespace HF.Domain.SocialServices.Events;

public sealed record ServiceDeliveredEvent(
    string DeliveryId,
    string BeneficiaryPersonId,
    string ProgramId,
    string ServiceDefinitionId,
    DateOnly DeliveryDate,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record BenefitGrantedEvent(
    string BenefitDeliveryId,
    string BeneficiaryPersonId,
    string ProgramId,
    DateOnly GrantedDate,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record BenefitDeliveredEvent(
    string BenefitDeliveryId,
    string BeneficiaryPersonId,
    string ProgramId,
    DateOnly DeliveryDate,
    string DeliveredByPersonId,
    string ReceivedByPersonId,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record BenefitRevokedEvent(
    string BenefitDeliveryId,
    string Reason,
    string RevokedByPersonId,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);
