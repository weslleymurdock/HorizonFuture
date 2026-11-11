using HF.Domain.Common.ValueObjects;

namespace HF.Domain.SocialServices.Enrollments;

public sealed class ParticipationHistoryEntry : ValueObject
{
    public ParticipationHistoryEntry(
        DateOnly transitionDate,
        ParticipationStatus previousStatus,
        ParticipationStatus newStatus,
        string? reason,
        string changedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(changedBy);

        TransitionDate = transitionDate;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ChangedBy = changedBy.Trim();
        RecordedAtUtc = DateTime.UtcNow;
    }

    public DateOnly TransitionDate { get; }
    public ParticipationStatus PreviousStatus { get; }
    public ParticipationStatus NewStatus { get; }
    public string? Reason { get; }
    public string ChangedBy { get; }
    public DateTime RecordedAtUtc { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TransitionDate;
        yield return PreviousStatus;
        yield return NewStatus;
        yield return Reason;
        yield return ChangedBy;
    }
}
