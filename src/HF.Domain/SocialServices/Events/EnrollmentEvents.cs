using HF.Domain.Common.Events;

namespace HF.Domain.SocialServices.Events;

public sealed record ProgramEnrollmentSubmittedEvent(
    string EnrollmentId,
    string ProgramId,
    string BeneficiaryPersonId,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ProgramEnrollmentApprovedEvent(
    string EnrollmentId,
    string ProgramId,
    string BeneficiaryPersonId,
    string ApprovedBy,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ProgramEnrollmentWaitlistedEvent(
    string EnrollmentId,
    string ProgramId,
    string BeneficiaryPersonId,
    string Reason,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ProgramEnrollmentRejectedEvent(
    string EnrollmentId,
    string ProgramId,
    string BeneficiaryPersonId,
    string Reason,
    string RejectedBy,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ProgramEnrollmentCancelledEvent(
    string EnrollmentId,
    string ProgramId,
    string BeneficiaryPersonId,
    string Reason,
    string CancelledBy,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);

public sealed record ProgramEnrollmentWithdrawnEvent(
    string EnrollmentId,
    string ProgramId,
    string BeneficiaryPersonId,
    string Reason,
    string WithdrawnBy,
    DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);
