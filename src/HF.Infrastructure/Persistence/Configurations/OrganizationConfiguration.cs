using HF.Domain.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.TradeName).HasMaxLength(200);
        builder.Property(entity => entity.LegalIdentifier).HasMaxLength(32);
        builder.HasIndex(entity => entity.LegalIdentifier).IsUnique().HasFilter("\"LegalIdentifier\" IS NOT NULL");
        builder.OwnsOne(entity => entity.Address, address =>
        {
            address.Property(value => value.Street).HasMaxLength(200);
            address.Property(value => value.Number).HasMaxLength(30);
            address.Property(value => value.District).HasMaxLength(120);
            address.Property(value => value.City).HasMaxLength(120);
            address.Property(value => value.State).HasMaxLength(80);
            address.Property(value => value.PostalCode).HasMaxLength(12);
            address.Property(value => value.Complement).HasMaxLength(200);
        });
        builder.HasMany(entity => entity.Units).WithOne().HasForeignKey(unit => unit.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(entity => entity.Departments).WithOne().HasForeignKey(department => department.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
