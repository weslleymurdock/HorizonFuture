namespace HF.Domain.Common.Exceptions;

/// <summary>
/// Represents a domain rule failure with a stable machine-readable code.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a domain exception.
    /// </summary>
    /// <param name="code">The stable, non-blank machine-readable error code.</param>
    /// <param name="message">The human-readable failure message.</param>
    /// <param name="innerException">The optional underlying exception.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> is blank.
    /// </exception>
    public DomainException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("The domain error code cannot be blank.", nameof(code));
        }

        Code = code;
    }

    /// <summary>
    /// Gets the stable machine-readable error code.
    /// </summary>
    public string Code { get; }
}
