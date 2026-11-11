using HF.Domain.Common.Entities;
using HF.Domain.Common.Events;

namespace HF.Domain.Common.Aggregates;

/// <summary>
/// Base class for aggregate roots that own pending domain events.
/// </summary>
/// <remarks>
/// Events are retained in registration order until explicitly cleared.
/// Dispatching, persistence, and handling belong to application and
/// infrastructure concerns.
/// </remarks>
public abstract class AggregateRoot : EntityBase
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly IReadOnlyCollection<IDomainEvent> _readOnlyDomainEvents;

    /// <summary>
    /// Initializes an aggregate root.
    /// </summary>
    protected AggregateRoot(string? id = null) : base(id)
    {
        _readOnlyDomainEvents = _domainEvents.AsReadOnly();
    }

    /// <summary>
    /// Gets the pending events in the order in which they were registered.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _readOnlyDomainEvents;

    /// <summary>
    /// Registers a pending domain event owned by this aggregate.
    /// </summary>
    /// <param name="domainEvent">The non-null event to register.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="domainEvent"/> is null.
    /// </exception>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all pending domain events without changing aggregate identity or
    /// audit information.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
