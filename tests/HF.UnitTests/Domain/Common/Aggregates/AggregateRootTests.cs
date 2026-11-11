using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Events;
using Xunit;

namespace HF.Application.Tests.Domain.Common.Aggregates;

public class AggregateRootTests
{
    [Fact]
    public void NewAggregate_StartsWithoutEvents()
    {
        var aggregate = new TestAggregate();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_RegistersEventsInOrder()
    {
        var aggregate = new TestAggregate();
        var first = new TestDomainEvent("first");
        var second = new TestDomainEvent("second");

        aggregate.Register(first);
        aggregate.Register(second);

        Assert.Equal([first, second], aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_ExposesReadOnlyCollection()
    {
        var aggregate = new TestAggregate();
        aggregate.Register(new TestDomainEvent("event"));

        Assert.IsAssignableFrom<IReadOnlyCollection<IDomainEvent>>(aggregate.DomainEvents);
        Assert.IsNotAssignableFrom<List<IDomainEvent>>(aggregate.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_WhenEventIsNull_Throws()
    {
        var aggregate = new TestAggregate();

        Assert.Throws<ArgumentNullException>(() => aggregate.Register(null!));
    }

    [Fact]
    public void ClearDomainEvents_RemovesEventsWithoutChangingIdentityOrAudit()
    {
        var aggregate = new TestAggregate("aggregate-1")
        {
            CreatedBy = "creator",
            UpdatedBy = "updater",
            UpdatedAt = DateTime.UtcNow
        };
        var createdAt = aggregate.CreatedAt;
        var updatedAt = aggregate.UpdatedAt;
        aggregate.Register(new TestDomainEvent("event"));

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
        Assert.Equal("aggregate-1", aggregate.Id);
        Assert.Equal(createdAt, aggregate.CreatedAt);
        Assert.Equal("creator", aggregate.CreatedBy);
        Assert.Equal(updatedAt, aggregate.UpdatedAt);
        Assert.Equal("updater", aggregate.UpdatedBy);
    }

    private sealed class TestAggregate(string? id = null) : AggregateRoot(id)
    {
        public void Register(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);
    }

    private sealed record TestDomainEvent(string Name) : DomainEvent;
}
