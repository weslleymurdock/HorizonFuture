using HF.Domain.Students;
using HF.Infrastructure.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.PersonId)
            .HasMaxLength(36);

        builder.HasIndex(s => s.PersonId);

        builder.Property(s => s.RegistrationNumber)
            .IsRequired();

        builder.HasIndex(s => s.RegistrationNumber)
            .IsUnique();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Student>(s => s.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
