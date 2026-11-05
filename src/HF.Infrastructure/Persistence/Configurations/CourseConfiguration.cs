using HF.Domain.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired();

        builder.Property(c => c.Code)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasMany(c => c.Subjects)
            .WithOne(s => s.Course)
            .HasForeignKey(s => s.CourseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

