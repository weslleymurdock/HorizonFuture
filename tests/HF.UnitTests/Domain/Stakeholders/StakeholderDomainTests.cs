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
        Assert.Equal(new DateOnly(2026, 3, 31), membership.Roles.First(role => role.Role == StakeholderRoleType.Volunteer).EndsOn);
        Assert.Equal(new DateOnly(2026, 6, 30), membership.Roles.First(role => role.Role == StakeholderRoleType.Educator).EndsOn);
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
    public void Membership_RejectsRoleOutsideMembershipAndEndingBeforeRoleStarts()
    {
        var membership = new OrganizationalMembership("org-1", "person-1", IdentityKind.Person, new DateOnly(2026, 1, 1));
        membership.AddRole(StakeholderRoleType.Volunteer, new DateOnly(2026, 3, 1));

        var exception = Assert.Throws<DomainException>(() => membership.End(new DateOnly(2026, 2, 1)));

        Assert.Equal("membership.invalid_period", exception.Code);
        Assert.Equal(MembershipStatus.Active, membership.Status);
    }

    [Fact]
    public void Membership_RejectsInvalidScopeAndScopeChangesAfterEnding()
    {
        var membership = new OrganizationalMembership("org-1", "person-1", IdentityKind.Person, new DateOnly(2026, 1, 1));
        var mismatch = Assert.Throws<DomainException>(() =>
            membership.ChangeScope(new MembershipScope("org-2", "unit-1", "department-1", "unit-2")));
        Assert.Equal("membership.scope_mismatch", mismatch.Code);

        membership.ChangeScope(new MembershipScope("org-1", "unit-1", "department-1", "unit-1"));
        membership.End(new DateOnly(2026, 2, 1));
        var inactive = Assert.Throws<DomainException>(() =>
            membership.ChangeScope(new MembershipScope("org-1", "unit-2")));
        Assert.Equal("membership.inactive", inactive.Code);
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
    public void IdentityMatching_RequiresSupportingSignalForExactDocumentMatch()
    {
        var left = new Person("Ana Silva");
        var right = new Person("Beatriz Souza");
        var documentOnly = IdentityMatching.Compare(left, right, "123456", "123456");
        var exact = IdentityMatching.Compare(left, new Person("Ana Silva"), "123456", "123456");

        Assert.Equal(DuplicateMatchKind.None, documentOnly.Kind);
        Assert.Equal(DuplicateMatchKind.Exact, exact.Kind);
    }

    [Fact]
    public void InstitutionIdentity_NormalizesAndProtectsArchivedProfile()
    {
        var institution = new InstitutionIdentity("  Instituto   Horizonte ", "12.345/0001-99");
        institution.UpdateProfile(institution.LegalName, "  Horizonte ", "12.345/0001-99");
        institution.Archive();

        Assert.Equal("Instituto Horizonte", institution.LegalName);
        Assert.Equal("12345000199", institution.RegistrationIdentifier);
        Assert.Throws<DomainException>(() => institution.UpdateProfile("Outro", null, null));
        institution.Reactivate();
        Assert.Equal(IdentityStatus.Active, institution.Status);
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
