using HF.Domain.Common.Exceptions;
using Xunit;

namespace HF.Application.Tests.Domain.Common.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithValidCode_PreservesCodeAndMessage()
    {
        var exception = new DomainException("domain.invalid", "The domain rule failed.");

        Assert.Equal("domain.invalid", exception.Code);
        Assert.Equal("The domain rule failed.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_WithBlankCode_Throws(string code)
    {
        Assert.Throws<ArgumentException>(() => new DomainException(code, "Message"));
    }

    [Fact]
    public void Constructor_WithInnerException_PreservesInnerException()
    {
        var innerException = new InvalidOperationException("Underlying failure");

        var exception = new DomainException("domain.failure", "Domain failure", innerException);

        Assert.Same(innerException, exception.InnerException);
    }
}
