namespace HF.Domain.Common.ValueObjects;

/// <summary>
/// Provides value-based equality for immutable domain value objects.
/// </summary>
/// <remarks>
/// Derived types should be immutable and provide their significant values in
/// deterministic order through <see cref="GetEqualityComponents"/>.
/// </remarks>
public abstract class ValueObject
{
    /// <summary>
    /// Determines whether this value object equals another object of the exact
    /// same runtime type and with equal components.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Returns a hash code derived from the equality components in order.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(GetType());

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Compares two value objects by value.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        ReferenceEquals(left, right) || (left is not null && left.Equals(right));

    /// <summary>
    /// Compares two value objects by value for inequality.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);

    /// <summary>
    /// Returns the ordered components that define equality for the derived
    /// value object.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();
}
