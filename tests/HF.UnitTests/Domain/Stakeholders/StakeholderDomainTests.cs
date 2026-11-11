using HF.Domain.Common.Exceptions;
using HF.Domain.Stakeholders;
using Xunit;

namespace HF.UnitTests.Domain.Stakeholders;

public sealed class StakeholderDomainTests
{
    [Fact]
    public void Membership_AllowsMultipleNonOverlappingRolesAndEndsTheirHistory()
    {
        var membership = new OrganizationalMembership(
            "org-1",
            "person-1",
            IdentityKind.Person,
            new DateOnly(2026, 1, 1));

        membership.AddRole(StakeholderRoleType.Volunteer, new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31));
        membership.AddRole(StakeholderRoleType.Educator, new DateOnly(2026, 4, 1));
        membership.End(new DateOnly(2026, 6, 30));

        Assert.Equal(MembershipStatus.Ended, membership.Status);
        Assert.All(membership.Roles, role => Assert.Equal(new DateOnly(2026, 6, 30), role.EndsOn));
    }

    [Fact]
    public void Membership_RejectsOverlappingDuplicateRole()
    {
        var membership = new OrganizationalMembership("org-1", "person-1", IdentityKind.Person, new DateOnly(2026, 1, 1));
        membership.AddRole(StakeholderRoleType.Volunteer, new DateOnly(2026, 1, 1));

        var exception = Assert.Throws<DomainException>((Action)(() =>
            membership.AddRole(StakeholderRoleType.Volunteer, new DateOnly(2026, 2, 1))));

        Assert.Equal("membership.duplicate_role", exception.Code);
    }

    [Fact]
    public void PersonRelationship_RejectsSelfReference()
    {
        var exception = Assert.Throws<DomainException>((Action)(() =>
            new PersonRelationship("person-1", "person-1", PersonRelationshipType.LegalGuardian, new DateOnly(2026, 1, 1))));

        Assert.Equal("person_relationship.self_reference", exception.Code);
    }

    [Fact]
    public void AccountLink_CanBeUnlinkedOnlyOnce()
    {
        var linkedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var link = new UserAccountLink("person-1", IdentityKind.Person, "account-1", linkedAt);
        link.Unlink(linkedAt.AddDays(1));

        var exception = Assert.Throws<DomainException>((Action)(() => link.Unlink(linkedAt.AddDays(2))));

        Assert.Equal("account_link.already_unlinked", exception.Code);
    }

    [Fact]
    public void IdentityMatching_NormalizesAccentsAndSpaces()
    {
        var left = new Person("  José   da Silva ");
        var right = new Person("JOSE DA SILVA");
        left.UpdateProfile(left.FullName, null, new DateOnly(1990, 1, 1));
        right.UpdateProfile(right.FullName, null, new DateOnly(1990, 1, 1));

        var result = IdentityMatching.Compare(left, right);

        Assert.Equal(DuplicateMatchKind.Possible, result.Kind);
        Assert.Contains("normalized_name", result.Signals);
        Assert.Contains("date_of_birth", result.Signals);
    }

    [Fact]
    public void ArchivedPerson_CannotBeUpdatedUntilReactivated()
    {
        var person = new Person("Ana Silva");
        person.Archive();

        Assert.Throws<DomainException>(() => person.UpdateProfile("Ana Santos", null, null));
        person.Reactivate();

        Assert.Equal(IdentityStatus.Active, person.Status);
    }
}
