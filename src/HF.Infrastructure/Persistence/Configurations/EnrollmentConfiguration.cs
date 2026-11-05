using HF.Domain.Enrollments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.StudentId)
            .IsRequired();

        builder.Property(e => e.SubjectId)
            .IsRequired();

        builder.Property(e => e.AcademicYear)
            .IsRequired();

        builder.Property(e => e.AcademicSemester)
            .IsRequired();

        builder.Property(e => e.EnrollmentDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.FinalGrade)
            .HasPrecision(5, 2);

        builder.HasIndex(e => new { e.StudentId, e.SubjectId, e.AcademicYear, e.AcademicSemester })
            .IsUnique();

        builder.HasIndex(e => new { e.SubjectId, e.AcademicYear, e.AcademicSemester });

        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Subject)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.SubjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

