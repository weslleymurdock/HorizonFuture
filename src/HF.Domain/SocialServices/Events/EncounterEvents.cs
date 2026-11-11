using HF.Domain.Common.Events;

namespace HF.Domain.SocialServices.Events;

public sealed record SocialCareEncounterScheduledEvent(
    string EncounterId,
    string BeneficiaryPersonId,
    string? ProgramId,
    DateTime ScheduledDateTime,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record SocialCareEncounterCompletedEvent(
    string EncounterId,
    string BeneficiaryPersonId,
    string AttendantPersonId,
    DateTime CompletedDateTime,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record SocialCareEncounterCancelledEvent(
    string EncounterId,
    string Reason,
    string CancelledBy,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);
