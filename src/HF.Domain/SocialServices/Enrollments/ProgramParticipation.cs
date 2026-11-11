using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Events;

namespace HF.Domain.SocialServices.Enrollments;

public sealed class ProgramParticipation : AggregateRoot
{
    private readonly List<ParticipationHistoryEntry> _history = [];

    private ProgramParticipation() { }

    public ProgramParticipation(
        string programId,
        string beneficiaryPersonId,
        string enrollmentId,
        DateOnly startDate,
        string startedBy,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(enrollmentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(startedBy);

        ProgramId = programId.Trim();
        BeneficiaryPersonId = beneficiaryPersonId.Trim();
        EnrollmentId = enrollmentId.Trim();
        StartDate = startDate;
        Status = ParticipationStatus.Active;

        _history.Add(new ParticipationHistoryEntry(
            startDate,
            ParticipationStatus.Active,
            ParticipationStatus.Active,
            "Initial participation started",
            startedBy));

        AddDomainEvent(new ProgramParticipationStartedEvent(Id, ProgramId, BeneficiaryPersonId, StartDate));
    }

    public string ProgramId { get; private set; } = string.Empty;
    public string BeneficiaryPersonId { get; private set; } = string.Empty;
    public string EnrollmentId { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public ParticipationStatus Status { get; private set; }

    public IReadOnlyList<ParticipationHistoryEntry> History => _history.AsReadOnly();

    public void Suspend(DateOnly date, string reason, string changedBy)
    {
        if (Status != ParticipationStatus.Active)
        {
            throw new DomainException("participation.not_active", $"Cannot suspend participation in status '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("participation.suspension_reason_required", "A reason is required to suspend participation.");
        }

        EnsureChronology(date);

        var previousStatus = Status;
        Status = ParticipationStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        _history.Add(new ParticipationHistoryEntry(date, previousStatus, Status, reason, changedBy));
        AddDomainEvent(new ProgramParticipationSuspendedEvent(Id, date, reason));
    }

    public void Resume(DateOnly date, string changedBy)
    {
        if (Status != ParticipationStatus.Suspended)
        {
            throw new DomainException("participation.not_suspended", $"Cannot resume participation in status '{Status}'.");
        }

        EnsureChronology(date);

        var previousStatus = Status;
        Status = ParticipationStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        _history.Add(new ParticipationHistoryEntry(date, previousStatus, Status, "Participation resumed", changedBy));
        AddDomainEvent(new ProgramParticipationResumedEvent(Id, date));
    }

    public void Complete(DateOnly date, string? notes, string changedBy)
    {
        if (Status != ParticipationStatus.Active)
        {
            throw new DomainException("participation.cannot_complete", $"Cannot complete participation in status '{Status}'. Only active participations can be completed.");
        }

        EnsureChronology(date);

        var previousStatus = Status;
        Status = ParticipationStatus.Completed;
        EndDate = date;
        UpdatedAt = DateTime.UtcNow;

        _history.Add(new ParticipationHistoryEntry(date, previousStatus, Status, notes ?? "Participation successfully completed", changedBy));
        AddDomainEvent(new ProgramParticipationCompletedEvent(Id, date, notes));
    }

    public void Terminate(DateOnly date, string reason, string changedBy)
    {
        if (Status is ParticipationStatus.Completed or ParticipationStatus.Terminated)
        {
            throw new DomainException("participation.already_ended", "Participation has already ended.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("participation.termination_reason_required", "A reason is required to terminate participation.");
        }

        EnsureChronology(date);

        var previousStatus = Status;
        Status = ParticipationStatus.Terminated;
        EndDate = date;
        UpdatedAt = DateTime.UtcNow;

        _history.Add(new ParticipationHistoryEntry(date, previousStatus, Status, reason, changedBy));
        AddDomainEvent(new ProgramParticipationTerminatedEvent(Id, date, reason));
    }

    private void EnsureChronology(DateOnly date)
    {
        if (date < StartDate)
        {
            throw new DomainException("participation.invalid_period", "The event date cannot be earlier than the participation start date.");
        }

        var lastEntry = _history.LastOrDefault();
        if (lastEntry != null && date < lastEntry.TransitionDate)
        {
            throw new DomainException("participation.invalid_chronology", $"The transition date ({date}) cannot be earlier than the previous transition date ({lastEntry.TransitionDate}).");
        }
    }
}
