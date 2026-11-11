using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Events;

namespace HF.Domain.SocialServices.Activities;

public sealed class ActivitySession : AggregateRoot
{
    private readonly List<SessionFacilitator> _facilitators = [];
    private readonly List<SessionAttendanceRecord> _attendance = [];

    private ActivitySession() { }

    public ActivitySession(
        string activityId,
        string programId,
        DateTime scheduledStart,
        DateTime scheduledEnd,
        string location,
        string title,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityId);
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (scheduledEnd < scheduledStart)
        {
            throw new DomainException("session.invalid_schedule", "Scheduled end time cannot be earlier than scheduled start time.");
        }

        ActivityId = activityId.Trim();
        ProgramId = programId.Trim();
        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
        Location = location.Trim();
        Title = title.Trim();
        Status = ActivitySessionStatus.Scheduled;

        AddDomainEvent(new ActivitySessionScheduledEvent(Id, ActivityId, ProgramId, ScheduledStart));
    }

    public string ActivityId { get; private set; } = string.Empty;
    public string ProgramId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public DateTime ScheduledStart { get; private set; }
    public DateTime ScheduledEnd { get; private set; }
    public DateTime? ActualStart { get; private set; }
    public DateTime? ActualEnd { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public ActivitySessionStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? CancelledByPersonId { get; private set; }
    public string? RescheduledByPersonId { get; private set; }

    public IReadOnlyList<SessionFacilitator> Facilitators => _facilitators.AsReadOnly();
    public IReadOnlyList<SessionAttendanceRecord> Attendance => _attendance.AsReadOnly();

    public void AddFacilitator(string personId, FacilitatorRole role)
    {
        EnsureNotFinished();
        ArgumentException.ThrowIfNullOrWhiteSpace(personId);

        var trimmedPersonId = personId.Trim();
        if (_facilitators.Any(f => f.PersonId == trimmedPersonId && f.Role == role))
        {
            throw new DomainException("session.duplicate_facilitator", "The facilitator with this role is already assigned to this session.");
        }

        _facilitators.Add(new SessionFacilitator(trimmedPersonId, role));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start(DateTime actualStart)
    {
        if (Status != ActivitySessionStatus.Scheduled && Status != ActivitySessionStatus.Rescheduled)
        {
            throw new DomainException("session.cannot_start", $"Cannot start a session in status '{Status}'.");
        }

        Status = ActivitySessionStatus.InProgress;
        ActualStart = actualStart;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(DateTime actualEnd)
    {
        if (Status == ActivitySessionStatus.Completed)
        {
            throw new DomainException("session.already_completed", "The session is already completed.");
        }

        if (Status == ActivitySessionStatus.Cancelled)
        {
            throw new DomainException("session.cannot_complete_cancelled", "A cancelled session cannot be completed.");
        }

        var startReference = ActualStart ?? ScheduledStart;
        if (actualEnd < startReference)
        {
            throw new DomainException("session.invalid_end_time", "Session end time cannot be earlier than start time.");
        }

        ActualEnd = actualEnd;
        Status = ActivitySessionStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ActivitySessionCompletedEvent(Id, ActivityId, ActualEnd.Value));
    }

    public void Cancel(string reason, string cancelledBy)
    {
        if (Status == ActivitySessionStatus.Cancelled)
        {
            throw new DomainException("session.already_cancelled", "The session is already cancelled.");
        }

        if (Status == ActivitySessionStatus.Completed)
        {
            throw new DomainException("session.cannot_cancel_completed", "A completed session cannot be cancelled.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("session.cancellation_reason_required", "A reason is required to cancel a session.");
        }

        if (string.IsNullOrWhiteSpace(cancelledBy))
        {
            throw new ArgumentException("CancelledBy cannot be blank.", nameof(cancelledBy));
        }

        Status = ActivitySessionStatus.Cancelled;
        CancellationReason = reason.Trim();
        CancelledByPersonId = cancelledBy.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ActivitySessionCancelledEvent(Id, ActivityId, CancellationReason));
    }

    public void Reschedule(DateTime newStart, DateTime newEnd, string reason, string? rescheduledBy = null)
    {
        EnsureNotFinished();

        if (newEnd < newStart)
        {
            throw new DomainException("session.invalid_schedule", "Scheduled end time cannot be earlier than scheduled start time.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("session.reschedule_reason_required", "A reason is required to reschedule a session.");
        }

        ScheduledStart = newStart;
        ScheduledEnd = newEnd;
        Status = ActivitySessionStatus.Rescheduled;
        RescheduledByPersonId = string.IsNullOrWhiteSpace(rescheduledBy) ? null : rescheduledBy.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordAttendance(
        string beneficiaryPersonId,
        AttendanceStatus status,
        string recordedByPersonId,
        string? justification = null)
    {
        if (Status == ActivitySessionStatus.Completed)
        {
            throw new DomainException("session.completed", "Attendance cannot be recorded for a completed session.");
        }

        if (Status == ActivitySessionStatus.Cancelled)
        {
            throw new DomainException("session.cancelled", "Cannot record attendance for a cancelled session.");
        }

        if (Status != ActivitySessionStatus.Scheduled &&
            Status != ActivitySessionStatus.Rescheduled &&
            Status != ActivitySessionStatus.InProgress)
        {
            throw new DomainException("session.invalid_state_for_attendance", $"Cannot record attendance in session status '{Status}'.");
        }

        if (!Enum.IsDefined(status) || status == AttendanceStatus.NotRecorded)
        {
            throw new DomainException("attendance.invalid_status", "The attendance status is invalid.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(recordedByPersonId);

        var trimmedPersonId = beneficiaryPersonId.Trim();
        if (_attendance.Any(a => a.BeneficiaryPersonId == trimmedPersonId))
        {
            throw new DomainException("attendance.duplicate_record", $"Attendance has already been recorded for participant '{trimmedPersonId}' in this session.");
        }

        // Validate record construction (including justification requirement) BEFORE adding
        var record = new SessionAttendanceRecord(trimmedPersonId, status, recordedByPersonId.Trim(), justification);
        _attendance.Add(record);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new AttendanceRecordedEvent(Id, trimmedPersonId, status, recordedByPersonId.Trim()));
    }

    private void EnsureNotFinished()
    {
        if (Status is ActivitySessionStatus.Completed or ActivitySessionStatus.Cancelled)
        {
            throw new DomainException("session.not_modifiable", $"Session in status '{Status}' cannot be modified.");
        }
    }
}
