using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Events;

namespace HF.Domain.SocialServices.Enrollments;

public sealed class ProgramParticipation : AggregateRoot
{
    private readonly List<ParticipationHistoryEntry> _history = [];

    private ProgramParticipation() { }

    internal ProgramParticipation(
        ProgramEnrollment enrollment,
        DateOnly startDate,
        string startedBy,
        string? id = null) : base(id)
    {
        ArgumentNullException.ThrowIfNull(enrollment);
        if (string.IsNullOrWhiteSpace(startedBy))
        {
            throw new ArgumentException("StartedBy cannot be blank.", nameof(startedBy));
        }

        if (enrollment.Status != EnrollmentStatus.Approved)
        {
            throw new DomainException("enrollment.not_approved", "Only an approved enrollment can initiate active participation.");
        }

        var initialEntry = new ParticipationHistoryEntry(
            startDate,
            ParticipationStatus.Active,
            ParticipationStatus.Active,
            "Initial participation started",
            startedBy.Trim());

        ProgramId = enrollment.ProgramId;
        BeneficiaryPersonId = enrollment.BeneficiaryPersonId;
        EnrollmentId = enrollment.Id;
        StartDate = startDate;
        Status = ParticipationStatus.Active;

        _history.Add(initialEntry);

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

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new ArgumentException("ChangedBy cannot be blank.", nameof(changedBy));
        }

        EnsureChronology(date);

        var entry = new ParticipationHistoryEntry(date, Status, ParticipationStatus.Suspended, reason.Trim(), changedBy.Trim());

        Status = ParticipationStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
        _history.Add(entry);

        AddDomainEvent(new ProgramParticipationSuspendedEvent(Id, date, reason.Trim()));
    }

    public void Resume(DateOnly date, string changedBy)
    {
        if (Status != ParticipationStatus.Suspended)
        {
            throw new DomainException("participation.not_suspended", $"Cannot resume participation in status '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new ArgumentException("ChangedBy cannot be blank.", nameof(changedBy));
        }

        EnsureChronology(date);

        var entry = new ParticipationHistoryEntry(date, Status, ParticipationStatus.Active, "Participation resumed", changedBy.Trim());

        Status = ParticipationStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        _history.Add(entry);

        AddDomainEvent(new ProgramParticipationResumedEvent(Id, date));
    }

    public void Complete(DateOnly date, string? notes, string changedBy)
    {
        if (Status != ParticipationStatus.Active)
        {
            throw new DomainException("participation.cannot_complete", $"Cannot complete participation in status '{Status}'. Only active participations can be completed.");
        }

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new ArgumentException("ChangedBy cannot be blank.", nameof(changedBy));
        }

        EnsureChronology(date);

        var entry = new ParticipationHistoryEntry(date, Status, ParticipationStatus.Completed, notes ?? "Participation successfully completed", changedBy.Trim());

        Status = ParticipationStatus.Completed;
        EndDate = date;
        UpdatedAt = DateTime.UtcNow;
        _history.Add(entry);

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

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new ArgumentException("ChangedBy cannot be blank.", nameof(changedBy));
        }

        EnsureChronology(date);

        var entry = new ParticipationHistoryEntry(date, Status, ParticipationStatus.Terminated, reason.Trim(), changedBy.Trim());

        Status = ParticipationStatus.Terminated;
        EndDate = date;
        UpdatedAt = DateTime.UtcNow;
        _history.Add(entry);

        AddDomainEvent(new ProgramParticipationTerminatedEvent(Id, date, reason.Trim()));
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
