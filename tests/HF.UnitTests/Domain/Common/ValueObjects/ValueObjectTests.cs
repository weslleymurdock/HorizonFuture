using HF.Domain.Common.ValueObjects;
using Xunit;

namespace HF.Application.Tests.Domain.Common.ValueObjects;

public class ValueObjectTests
{
    [Fact]
    public void Equals_WhenSameTypeAndComponentsMatch_ReturnsTrue()
    {
        var left = new TestValueObject("same", null);
        var right = new TestValueObject("same", null);

        Assert.True(left.Equals(right));
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
        Assert.True(left == right);
        Assert.False(left != right);
    }

    [Fact]
    public void Equals_WhenSameTypeAndComponentsDiffer_ReturnsFalse()
    {
        var left = new TestValueObject("left", 1);
        var right = new TestValueObject("right", 1);

        Assert.False(left.Equals(right));
        Assert.True(left != right);
    }

    [Fact]
    public void Equals_WhenRuntimeTypesDiffer_ReturnsFalse()
    {
        var left = new TestValueObject("same", 1);
        var right = new OtherValueObject("same", 1);

        Assert.False(left.Equals(right));
    }

    [Fact]
    public void EqualityOperators_HandleNullValues()
    {
        TestValueObject? nullValue = null;
        var value = new TestValueObject("value", null);

        Assert.True(nullValue == null);
        Assert.False(nullValue != null);
        Assert.False(nullValue == value);
        Assert.True(nullValue != value);
    }

    private sealed class TestValueObject(string text, int? number) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents() => [text, number];
    }

    private sealed class OtherValueObject(string text, int? number) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents() => [text, number];
    }
}
