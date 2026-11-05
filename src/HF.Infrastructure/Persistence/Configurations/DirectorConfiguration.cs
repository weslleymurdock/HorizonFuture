using HF.Domain.Directors;
using HF.Infrastructure.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class DirectorConfiguration : IEntityTypeConfiguration<Director>
{
    public void Configure(EntityTypeBuilder<Director> builder)
    {
        builder.ToTable("Directors");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.UserId)
            .IsRequired();

        builder.Property(d => d.Directorate)
            .IsRequired();

        builder.Property(d => d.PositionTitle)
            .IsRequired();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Director>(d => d.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

