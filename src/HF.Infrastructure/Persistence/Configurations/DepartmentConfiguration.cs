using HF.Domain.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(160).IsRequired();
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Name }).IsUnique();
        builder.HasOne<OrganizationalUnit>().WithMany().HasForeignKey(entity => entity.UnitId).OnDelete(DeleteBehavior.SetNull);
    }
}
