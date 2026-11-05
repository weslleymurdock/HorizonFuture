using HF.Domain.TeacherAssignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class TeacherAssignmentConfiguration : IEntityTypeConfiguration<TeacherAssignment>
{
    public void Configure(EntityTypeBuilder<TeacherAssignment> builder)
    {
        builder.ToTable("TeacherAssignments");

        builder.HasKey(ta => ta.Id);

        builder.Property(ta => ta.TeacherId)
            .IsRequired();

        builder.Property(ta => ta.SubjectId)
            .IsRequired();

        builder.Property(ta => ta.AcademicYear)
            .IsRequired();

        builder.Property(ta => ta.AcademicSemester)
            .IsRequired();

        builder.Property(ta => ta.Role)
            .IsRequired();

        builder.HasIndex(ta => new { ta.TeacherId, ta.SubjectId, ta.AcademicYear, ta.AcademicSemester })
            .IsUnique();

        builder.HasIndex(ta => new { ta.SubjectId, ta.AcademicYear, ta.AcademicSemester });

        builder.HasOne(ta => ta.Teacher)
            .WithMany(t => t.Assignments)
            .HasForeignKey(ta => ta.TeacherId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.Subject)
            .WithMany(s => s.TeacherAssignments)
            .HasForeignKey(ta => ta.SubjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

