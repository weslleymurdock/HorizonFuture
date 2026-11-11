using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Events;

namespace HF.Domain.SocialServices.Activities;

public sealed class SocialActivity : AggregateRoot
{
    private SocialActivity() { }

    public SocialActivity(
        string programId,
        string title,
        string? description = null,
        string? objectives = null,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        ProgramId = programId.Trim();
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Objectives = string.IsNullOrWhiteSpace(objectives) ? null : objectives.Trim();
        Status = ActivityStatus.Draft;

        AddDomainEvent(new SocialActivityCreatedEvent(Id, ProgramId, Title));
    }

    public string ProgramId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Objectives { get; private set; }
    public ActivityStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? CancelledByPersonId { get; private set; }

    public void Activate()
    {
        if (Status == ActivityStatus.Active)
        {
            throw new DomainException("activity.already_active", "The activity is already active.");
        }

        if (Status is ActivityStatus.Completed or ActivityStatus.Cancelled)
        {
            throw new DomainException("activity.cannot_activate", $"Cannot activate an activity in status '{Status}'.");
        }

        Status = ActivityStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status == ActivityStatus.Completed)
        {
            throw new DomainException("activity.already_completed", "The activity is already completed.");
        }

        if (Status != ActivityStatus.Active)
        {
            throw new DomainException("activity.cannot_complete", $"Cannot complete an activity in status '{Status}'.");
        }

        Status = ActivityStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason, string cancelledBy)
    {
        if (Status == ActivityStatus.Cancelled)
        {
            throw new DomainException("activity.already_cancelled", "The activity is already cancelled.");
        }

        if (Status == ActivityStatus.Completed)
        {
            throw new DomainException("activity.cannot_cancel_completed", "A completed activity cannot be cancelled.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("activity.cancellation_reason_required", "A reason is required to cancel an activity.");
        }

        if (string.IsNullOrWhiteSpace(cancelledBy))
        {
            throw new ArgumentException("CancelledBy cannot be blank.", nameof(cancelledBy));
        }

        Status = ActivityStatus.Cancelled;
        CancellationReason = reason.Trim();
        CancelledByPersonId = cancelledBy.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public ActivitySession ScheduleSession(
        DateTime scheduledStart,
        DateTime scheduledEnd,
        string location,
        string? title = null)
    {
        if (Status != ActivityStatus.Active)
        {
            throw new DomainException("activity.not_active", "Sessions can only be scheduled for active activities.");
        }

        return new ActivitySession(Id, ProgramId, scheduledStart, scheduledEnd, location, title ?? Title);
    }
}
