using HF.Domain.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public sealed class OrganizationalUnitConfiguration : IEntityTypeConfiguration<OrganizationalUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationalUnit> builder)
    {
        builder.ToTable("OrganizationalUnits");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(1000);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Name }).IsUnique();
        builder.HasOne<OrganizationalUnit>().WithMany().HasForeignKey(entity => entity.ParentUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
