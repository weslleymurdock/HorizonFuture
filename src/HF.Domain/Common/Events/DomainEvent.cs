namespace HF.Domain.Common.Events;

/// <summary>
/// Base record for immutable domain events with a UTC occurrence timestamp.
/// </summary>
/// <remarks>
/// Domain events describe facts that have already occurred. They are not
/// integration messages, HTTP notifications, or dispatch instructions.
/// </remarks>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a domain event with the current UTC time or an explicit UTC
    /// timestamp.
    /// </summary>
    /// <param name="occurredOnUtc">An explicit UTC timestamp, when provided.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the explicit timestamp does not use a zero UTC offset.
    /// </exception>
    protected DomainEvent(DateTimeOffset? occurredOnUtc = null)
    {
        if (occurredOnUtc.HasValue && occurredOnUtc.Value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("The occurrence timestamp must be UTC.", nameof(occurredOnUtc));
        }

        OccurredOnUtc = occurredOnUtc ?? DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the immutable UTC occurrence timestamp.
    /// </summary>
    public DateTimeOffset OccurredOnUtc { get; }
}
