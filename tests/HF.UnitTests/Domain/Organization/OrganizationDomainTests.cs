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
    public void DeactivateUnit_RejectsAnEndDateBeforeItsStart()
    {
        var organization = new OrganizationAggregate("Instituto Horizonte", OrganizationType.CivilSocietyOrganization);
        var unit = organization.AddUnit("Centro Norte");

        var exception = Assert.Throws<DomainException>((Action)(() =>
            unit.Deactivate(unit.EffectiveFrom.AddDays(-1))));

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
}
