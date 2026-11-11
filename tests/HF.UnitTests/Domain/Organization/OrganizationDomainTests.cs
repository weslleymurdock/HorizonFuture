using HF.Domain.Common.Exceptions;
using HF.Domain.Organization;
using OrganizationAggregate = HF.Domain.Organization.Organization;
using Xunit;

namespace HF.UnitTests.Domain.Organization;

public sealed class OrganizationDomainTests
{
    [Fact]
    public void AddUnitAndDepartment_RequiresOrganizationScope()
    {
        var organization = new OrganizationAggregate("Instituto Horizonte", OrganizationType.CivilSocietyOrganization);

        var unit = organization.AddUnit("Centro Norte");
        var department = organization.AddDepartment("Atendimento", unit.Id);

        Assert.Equal(organization.Id, unit.OrganizationId);
        Assert.Equal(unit.Id, department.UnitId);
    }

    [Fact]
    public void ArchiveUnit_RejectsAnEndDateBeforeItsStart()
    {
        var organization = new OrganizationAggregate("Instituto Horizonte", OrganizationType.CivilSocietyOrganization);
        var unit = organization.AddUnit("Centro Norte");

        var exception = Assert.Throws<DomainException>((Action)(() =>
            organization.ArchiveUnit(unit.Id, unit.EffectiveFrom.AddDays(-1))));

        Assert.Equal("organization.unit.invalid_period", exception.Code);
    }

    [Fact]
    public void MoveUnit_RejectsSelfParentAndIndirectCycles()
    {
        var organization = new OrganizationAggregate("Instituto Horizonte", OrganizationType.CivilSocietyOrganization);
        var root = organization.AddUnit("Root");
        var child = organization.AddUnit("Child", root.Id);
        var grandchild = organization.AddUnit("Grandchild", child.Id);

        var self = Assert.Throws<DomainException>(() => organization.MoveUnit(child.Id, child.Id));
        var cycle = Assert.Throws<DomainException>(() => organization.MoveUnit(root.Id, grandchild.Id));

        Assert.Equal("organization.unit.self_parent", self.Code);
        Assert.Equal("organization.unit.cycle", cycle.Code);
    }

    [Fact]
    public void AddUnit_RejectsEquivalentNamesWithinOrganization()
    {
        var organization = new OrganizationAggregate("Instituto Horizonte", OrganizationType.CivilSocietyOrganization);
        organization.AddUnit("Centro Norte");

        var exception = Assert.Throws<DomainException>(() => organization.AddUnit(" Centro   Norte "));

        Assert.Equal("organization.unit.duplicate_name", exception.Code);
    }

    [Fact]
    public void RenameUnit_RejectsEquivalentNamesWithinOrganization()
    {
        var organization = new OrganizationAggregate("Instituto Horizonte", OrganizationType.CivilSocietyOrganization);
        var first = organization.AddUnit("Centro Norte");
        var second = organization.AddUnit("Centro Sul");

        var exception = Assert.Throws<DomainException>(() => organization.RenameUnit(second.Id, " Centro   Norte "));

        Assert.Equal("organization.unit.duplicate_name", exception.Code);
        Assert.Equal("Centro Sul", second.Name);
        organization.RenameUnit(first.Id, "  Centro   Central ");
        Assert.Equal("Centro Central", first.Name);
    }

    [Fact]
    public void RenameDepartment_UsesUnitScopeAndOrganizationCannotArchiveWithActiveDepartments()
    {
        var organization = new OrganizationAggregate("Instituto Horizonte", OrganizationType.CivilSocietyOrganization);
        var north = organization.AddUnit("Norte");
        var south = organization.AddUnit("Sul");
        var first = organization.AddDepartment("Atendimento", north.Id);
        var second = organization.AddDepartment("Recepção", north.Id);
        var third = organization.AddDepartment("Atendimento", south.Id);

        var duplicate = Assert.Throws<DomainException>(() => organization.RenameDepartment(second.Id, " Atendimento "));
        Assert.Equal("organization.department.duplicate_name", duplicate.Code);
        organization.ArchiveDepartment(first.Id);
        organization.ArchiveDepartment(second.Id);
        organization.ArchiveDepartment(third.Id);
        organization.ArchiveUnit(north.Id, north.EffectiveFrom);
        organization.ArchiveUnit(south.Id, south.EffectiveFrom);
        organization.Archive();
        Assert.Equal(OrganizationStatus.Archived, organization.Status);
    }

    [Fact]
    public void ArchiveUnit_RejectsActiveChildrenThroughAggregateBoundary()
    {
        var organization = new OrganizationAggregate("Instituto Horizonte", OrganizationType.CivilSocietyOrganization);
        var parent = organization.AddUnit("Parent");
        organization.AddUnit("Child", parent.Id);

        var exception = Assert.Throws<DomainException>(() => organization.ArchiveUnit(parent.Id, parent.EffectiveFrom));

        Assert.Equal("organization.unit.active_children", exception.Code);
    }
}
