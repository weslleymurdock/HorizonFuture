using HF.Domain.AssetsLiabilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired();

        builder.Property(a => a.AssetTag)
            .IsRequired();

        builder.Property(a => a.AcquisitionValue)
            .HasPrecision(18, 2);

        builder.HasIndex(a => a.AssetTag)
            .IsUnique();
    }
}

