using HF.Domain.AssetsLiabilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class LiabilityConfiguration : IEntityTypeConfiguration<Liability>
{
    public void Configure(EntityTypeBuilder<Liability> builder)
    {
        builder.ToTable("Liabilities");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .IsRequired();

        builder.Property(l => l.Creditor)
            .IsRequired();

        builder.Property(l => l.Amount)
            .HasPrecision(18, 2);

        builder.Property(l => l.PaidAmount)
            .HasPrecision(18, 2);
    }
}

