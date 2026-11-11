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
}
