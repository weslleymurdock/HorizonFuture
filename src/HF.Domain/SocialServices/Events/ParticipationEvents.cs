using HF.Domain.Common.Events;

namespace HF.Domain.SocialServices.Events;

public sealed record ProgramParticipationStartedEvent(
    string ParticipationId,
    string ProgramId,
    string BeneficiaryPersonId,
    DateOnly StartDate,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ProgramParticipationSuspendedEvent(
    string ParticipationId,
    DateOnly SuspendedDate,
    string Reason,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ProgramParticipationResumedEvent(
    string ParticipationId,
    DateOnly ResumedDate,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ProgramParticipationCompletedEvent(
    string ParticipationId,
    DateOnly CompletedDate,
    string? Notes,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ProgramParticipationTerminatedEvent(
    string ParticipationId,
    DateOnly TerminatedDate,
    string Reason,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);
