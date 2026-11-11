using HF.Domain.Common.Events;
using HF.Domain.SocialServices.Activities;

namespace HF.Domain.SocialServices.Events;

public sealed record SocialActivityCreatedEvent(
    string ActivityId,
    string ProgramId,
    string Title,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ActivitySessionScheduledEvent(
    string SessionId,
    string ActivityId,
    string ProgramId,
    DateTime ScheduledStart,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ActivitySessionStartedEvent(
    string SessionId,
    string ActivityId,
    DateTime ActualStart,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ActivitySessionCompletedEvent(
    string SessionId,
    string ActivityId,
    DateTime ActualEnd,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ActivitySessionCancelledEvent(
    string SessionId,
    string ActivityId,
    string Reason,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record AttendanceRecordedEvent(
    string SessionId,
    string BeneficiaryPersonId,
    AttendanceStatus Status,
    string RecordedByPersonId,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);
