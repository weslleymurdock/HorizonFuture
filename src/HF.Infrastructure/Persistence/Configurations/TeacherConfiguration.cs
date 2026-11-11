using HF.Domain.Teachers;
using HF.Infrastructure.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.PersonId)
            .HasMaxLength(36);

        builder.HasIndex(t => t.PersonId);

        builder.Property(t => t.EmployeeNumber)
            .IsRequired();

        builder.Property(t => t.Specialty)
            .IsRequired();

        builder.HasIndex(t => t.EmployeeNumber)
            .IsUnique();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Teacher>(t => t.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
