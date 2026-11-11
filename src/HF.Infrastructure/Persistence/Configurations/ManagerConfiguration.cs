using HF.Domain.Managers;
using HF.Infrastructure.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.ToTable("Managers");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.Property(m => m.PersonId)
            .HasMaxLength(36);

        builder.HasIndex(m => m.PersonId);

        builder.Property(m => m.EmployeeNumber)
            .IsRequired();

        builder.Property(m => m.Department)
            .IsRequired();

        builder.Property(m => m.RoleTitle)
            .IsRequired();

        builder.HasIndex(m => m.EmployeeNumber)
            .IsUnique();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Manager>(m => m.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
