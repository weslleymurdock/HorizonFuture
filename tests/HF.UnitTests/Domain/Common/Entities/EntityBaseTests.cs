using HF.Domain.Common.Entities;
using Xunit;

namespace HF.Application.Tests.Domain.Common.Entities;

public class EntityBaseTests
{
    [Fact]
    public void ConstructorWithoutId_GeneratesGuidV7IdentifierAndAuditDefaults()
    {
        var before = DateTime.UtcNow;

        var entity = new EntityBase();

        var after = DateTime.UtcNow;
        Assert.True(Guid.TryParse(entity.Id, out var identifier));
        Assert.Equal(7, identifier.Version);
        Assert.InRange(entity.CreatedAt, before, after);
        Assert.Null(entity.UpdatedAt);
        Assert.Equal(string.Empty, entity.CreatedBy);
        Assert.Null(entity.UpdatedBy);
    }

    [Fact]
    public void ConstructorWithId_PreservesIdentifier()
    {
        var entity = new EntityBase("entity-1");

        Assert.Equal("entity-1", entity.Id);
    }
}
