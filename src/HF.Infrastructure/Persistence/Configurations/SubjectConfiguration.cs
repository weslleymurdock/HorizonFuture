using HF.Domain.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired();

        builder.Property(s => s.Code)
            .IsRequired();

        builder.HasIndex(s => s.Code)
            .IsUnique();

        builder.HasOne(s => s.Course)
            .WithMany(c => c.Subjects)
            .HasForeignKey(s => s.CourseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

