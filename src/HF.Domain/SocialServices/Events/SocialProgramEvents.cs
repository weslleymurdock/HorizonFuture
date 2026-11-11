using HF.Domain.Common.Events;

namespace HF.Domain.SocialServices.Events;

public sealed record SocialProgramCreatedEvent(
    string ProgramId,
    string Name,
    string OrganizationalUnitId,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record SocialProgramActivatedEvent(
    string ProgramId,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record SocialProgramSuspendedEvent(
    string ProgramId,
    string Reason,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record SocialProgramCompletedEvent(
    string ProgramId,
    DateOnly CompletionDate,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record SocialProgramArchivedEvent(
    string ProgramId,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);
