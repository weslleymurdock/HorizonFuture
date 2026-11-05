using HF.Domain.Volunteers;
using HF.Infrastructure.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class VolunteerConfiguration : IEntityTypeConfiguration<Volunteer>
{
    public void Configure(EntityTypeBuilder<Volunteer> builder)
    {
        builder.ToTable("Volunteers");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.UserId)
            .IsRequired();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Volunteer>(v => v.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

