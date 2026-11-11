using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Organization;

public sealed class Organization : AggregateRoot
{
    private readonly List<OrganizationalUnit> _units = [];
    private readonly List<Department> _departments = [];

    private Organization() { }

    public Organization(string legalName, OrganizationType type, string? id = null) : base(id)
    {
        LegalName = Require(legalName, nameof(legalName));
        Type = type;
        Status = OrganizationStatus.Active;
    }

    public string LegalName { get; private set; } = string.Empty;
    public string? TradeName { get; private set; }
    public string? LegalIdentifier { get; private set; }
    public OrganizationType Type { get; private set; }
    public OrganizationStatus Status { get; private set; }
    public OrganizationAddress? Address { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public DateOnly? FoundedOn { get; private set; }
    public IReadOnlyCollection<OrganizationalUnit> Units => _units.AsReadOnly();
    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();

    public void UpdateInstitutionalDetails(
        string legalName,
        string? tradeName,
        string? legalIdentifier,
        OrganizationAddress? address,
        string? contactEmail,
        string? contactPhone,
        DateOnly? foundedOn)
    {
        EnsureActive();
        LegalName = Require(legalName, nameof(legalName));
        TradeName = NormalizeOptional(tradeName);
        LegalIdentifier = NormalizeLegalIdentifier(legalIdentifier);
        Address = address;
        ContactEmail = NormalizeOptional(contactEmail);
        ContactPhone = NormalizeOptional(contactPhone);
        FoundedOn = foundedOn;
        UpdatedAt = DateTime.UtcNow;
    }

    public OrganizationalUnit AddUnit(string name, string? parentUnitId = null, string? id = null)
    {
        EnsureActive();
        if (parentUnitId is not null && _units.All(unit => unit.Id != parentUnitId))
        {
            throw new DomainException("organization.unit.parent_not_found", "The parent unit does not belong to this organization.");
        }

        var unit = new OrganizationalUnit(Id, name, parentUnitId, id);
        _units.Add(unit);
        UpdatedAt = DateTime.UtcNow;
        return unit;
    }

    public Department AddDepartment(string name, string? unitId = null, string? id = null)
    {
        EnsureActive();
        if (unitId is not null && _units.All(unit => unit.Id != unitId))
        {
            throw new DomainException("organization.department.unit_not_found", "The department unit does not belong to this organization.");
        }

        var department = new Department(Id, name, unitId, id);
        _departments.Add(department);
        UpdatedAt = DateTime.UtcNow;
        return department;
    }

    public void Archive()
    {
        if (_units.Any(unit => unit.Status == OrganizationalUnitStatus.Active))
        {
            throw new DomainException("organization.active_units", "An organization with active units cannot be archived.");
        }

        Status = OrganizationStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (Status != OrganizationStatus.Active)
        {
            throw new DomainException("organization.inactive", "The organization is not active.");
        }
    }

    private static string Require(string value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeLegalIdentifier(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? null
            : new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return normalized;
    }
}
