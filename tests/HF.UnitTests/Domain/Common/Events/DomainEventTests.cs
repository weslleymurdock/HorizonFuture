using HF.Domain.Common.Events;
using Xunit;

namespace HF.Application.Tests.Domain.Common.Events;

public class DomainEventTests
{
    [Fact]
    public void ConstructorWithoutTimestamp_GeneratesUtcTimestamp()
    {
        var before = DateTimeOffset.UtcNow;
        var domainEvent = new TestDomainEvent();
        var after = DateTimeOffset.UtcNow;

        Assert.Equal(TimeSpan.Zero, domainEvent.OccurredOnUtc.Offset);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void ConstructorWithUtcTimestamp_PreservesTimestamp()
    {
        var timestamp = new DateTimeOffset(2026, 11, 10, 12, 30, 0, TimeSpan.Zero);

        var domainEvent = new TestDomainEvent(timestamp);

        Assert.Equal(timestamp, domainEvent.OccurredOnUtc);
    }

    [Fact]
    public void ConstructorWithNonUtcTimestamp_Throws()
    {
        var timestamp = new DateTimeOffset(2026, 11, 10, 12, 30, 0, TimeSpan.FromHours(-3));

        var exception = Assert.Throws<ArgumentException>(() => new TestDomainEvent(timestamp));

        Assert.Equal("occurredOnUtc", exception.ParamName);
    }

    [Fact]
    public void OccurredOnUtc_HasNoPublicSetter()
    {
        var property = typeof(IDomainEvent).GetProperty(nameof(IDomainEvent.OccurredOnUtc));

        Assert.NotNull(property);
        Assert.Null(property!.SetMethod);
    }

    private sealed record TestDomainEvent(DateTimeOffset? Timestamp = null) : DomainEvent(Timestamp);
}
