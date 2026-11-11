using HF.Domain.Governance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public sealed class GovernanceBodyConfiguration : IEntityTypeConfiguration<GovernanceBody>
{
    public void Configure(EntityTypeBuilder<GovernanceBody> builder)
    {
        builder.ToTable("GovernanceBodies");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Type).HasMaxLength(80).IsRequired();
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Name }).IsUnique();
    }
}

public sealed class GovernanceMandateConfiguration : IEntityTypeConfiguration<GovernanceMandate>
{
    public void Configure(EntityTypeBuilder<GovernanceMandate> builder)
    {
        builder.ToTable("GovernanceMandates");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Title).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.EvidenceReference).HasMaxLength(500);
        builder.HasIndex(entity => new { entity.BodyId, entity.PersonId, entity.StartsOn });
    }
}

public sealed class AuthorityAssignmentConfiguration : IEntityTypeConfiguration<AuthorityAssignment>
{
    public void Configure(EntityTypeBuilder<AuthorityAssignment> builder)
    {
        builder.ToTable("AuthorityAssignments");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Responsibility).HasMaxLength(200).IsRequired();
        builder.HasIndex(entity => new { entity.OrganizationId, entity.PersonId, entity.Responsibility, entity.StartsOn });
    }
}
