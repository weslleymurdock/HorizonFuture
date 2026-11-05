using HF.Domain.ClassSessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("ClassSessions");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.TeacherAssignmentId)
            .IsRequired();

        builder.Property(cs => cs.Date)
            .IsRequired();

        builder.HasIndex(cs => new { cs.TeacherAssignmentId, cs.Date });

        builder.HasOne(cs => cs.TeacherAssignment)
            .WithMany(ta => ta.ClassSessions)
            .HasForeignKey(cs => cs.TeacherAssignmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

