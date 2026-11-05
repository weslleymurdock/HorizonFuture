using HF.Domain.Attendances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ClassSessionId)
            .IsRequired();

        builder.Property(a => a.EnrollmentId)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired();

        builder.HasIndex(a => new { a.ClassSessionId, a.EnrollmentId })
            .IsUnique();

        builder.HasIndex(a => a.EnrollmentId);

        builder.HasOne(a => a.ClassSession)
            .WithMany(cs => cs.Attendances)
            .HasForeignKey(a => a.ClassSessionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Enrollment)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EnrollmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

