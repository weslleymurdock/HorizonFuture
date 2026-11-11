using HF.Domain.Common.Events;

namespace HF.Domain.SocialServices.Events;

public sealed record SocialReferralCreatedEvent(
    string ReferralId,
    string BeneficiaryPersonId,
    string DestinationName,
    DateOnly ReferralDate,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record SocialReferralCompletedEvent(
    string ReferralId,
    string BeneficiaryPersonId,
    DateOnly OutcomeDate,
    bool IsSuccessful,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record SocialReferralCancelledEvent(
    string ReferralId,
    string Reason,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);
