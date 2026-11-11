using HF.Domain.Governance;
using HF.Domain.Stakeholders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HF.Infrastructure.Persistence.Configurations;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.FullName).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.SocialName).HasMaxLength(200);
        builder.HasIndex(entity => new { entity.FullName, entity.DateOfBirth });
    }
}

public sealed class InstitutionIdentityConfiguration : IEntityTypeConfiguration<InstitutionIdentity>
{
    public void Configure(EntityTypeBuilder<InstitutionIdentity> builder)
    {
        builder.ToTable("InstitutionIdentities");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.TradeName).HasMaxLength(200);
        builder.Property(entity => entity.RegistrationIdentifier).HasMaxLength(32);
        builder.HasIndex(entity => entity.RegistrationIdentifier).IsUnique().HasFilter("\"RegistrationIdentifier\" IS NOT NULL");
    }
}

public sealed class OrganizationalMembershipConfiguration : IEntityTypeConfiguration<OrganizationalMembership>
{
    public void Configure(EntityTypeBuilder<OrganizationalMembership> builder)
    {
        builder.ToTable("OrganizationalMemberships");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.IdentityId).HasMaxLength(36).IsRequired();
        builder.HasIndex(entity => new { entity.OrganizationId, entity.IdentityId, entity.IdentityKind }).IsUnique();
        builder.HasMany(entity => entity.Roles).WithOne().HasForeignKey(role => role.MembershipId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class MembershipRoleConfiguration : IEntityTypeConfiguration<MembershipRole>
{
    public void Configure(EntityTypeBuilder<MembershipRole> builder)
    {
        builder.ToTable("MembershipRoles");
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => new { entity.MembershipId, entity.Role, entity.StartsOn });
    }
}

public sealed class UserAccountLinkConfiguration : IEntityTypeConfiguration<UserAccountLink>
{
    public void Configure(EntityTypeBuilder<UserAccountLink> builder)
    {
        builder.ToTable("UserAccountLinks");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.IdentityId).HasMaxLength(36).IsRequired();
        builder.Property(entity => entity.AccountId).HasMaxLength(36).IsRequired();
        builder.HasIndex(entity => new { entity.IdentityId, entity.IdentityKind, entity.AccountId }).IsUnique();
        builder.HasIndex(entity => entity.AccountId).IsUnique().HasFilter("\"IsActive\" = true");
    }
}

public sealed class PersonRelationshipConfiguration : IEntityTypeConfiguration<PersonRelationship>
{
    public void Configure(EntityTypeBuilder<PersonRelationship> builder)
    {
        builder.ToTable("PersonRelationships");
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => new { entity.PersonId, entity.RelatedPersonId, entity.Type, entity.StartsOn }).IsUnique();
    }
}

public sealed class IdentityMergeConfiguration : IEntityTypeConfiguration<IdentityMerge>
{
    public void Configure(EntityTypeBuilder<IdentityMerge> builder)
    {
        builder.ToTable("IdentityMerges");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.SourceIdentityId).HasMaxLength(36).IsRequired();
        builder.Property(entity => entity.TargetIdentityId).HasMaxLength(36).IsRequired();
        builder.Property(entity => entity.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(entity => entity.ApprovedBy).HasMaxLength(100);
        builder.Property(entity => entity.CompletedBy).HasMaxLength(100);
        builder.Property(entity => entity.RejectedBy).HasMaxLength(100);
        builder.HasIndex(entity => new { entity.SourceIdentityId, entity.TargetIdentityId }).IsUnique();
    }
}
