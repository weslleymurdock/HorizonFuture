namespace HF.Domain.Common.Events;

/// <summary>
/// Represents an immutable fact that has occurred within a domain operation.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the UTC timestamp at which the event occurred.
    /// </summary>
    DateTimeOffset OccurredOnUtc { get; }
}
