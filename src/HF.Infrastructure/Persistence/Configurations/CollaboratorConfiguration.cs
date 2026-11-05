using HF.Domain.Collaborators;
using HF.Infrastructure.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class CollaboratorConfiguration : IEntityTypeConfiguration<Collaborator>
{
    public void Configure(EntityTypeBuilder<Collaborator> builder)
    {
        builder.ToTable("Collaborators");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.EmployeeNumber)
            .IsRequired();

        builder.Property(c => c.JobTitle)
            .IsRequired();

        builder.Property(c => c.Department)
            .IsRequired();

        builder.HasIndex(c => c.EmployeeNumber)
            .IsUnique();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Collaborator>(c => c.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

