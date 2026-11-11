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
        var normalizedName = NormalizeName(name);
        if (_units.Any(unit => string.Equals(NormalizeName(unit.Name), normalizedName, StringComparison.Ordinal)))
        {
            throw new DomainException("organization.unit.duplicate_name", "The organization already contains a unit with this name.");
        }

        if (parentUnitId is not null && _units.All(unit => unit.Id != parentUnitId))
        {
            throw new DomainException("organization.unit.parent_not_found", "The parent unit does not belong to this organization.");
        }

        var unit = new OrganizationalUnit(Id, normalizedName, parentUnitId, id);
        _units.Add(unit);
        UpdatedAt = DateTime.UtcNow;
        return unit;
    }

    public Department AddDepartment(string name, string? unitId = null, string? id = null)
    {
        EnsureActive();
        var normalizedName = NormalizeName(name);
        if (unitId is not null && _units.All(unit => unit.Id != unitId))
        {
            throw new DomainException("organization.department.unit_not_found", "The department unit does not belong to this organization.");
        }

        if (_departments.Any(department =>
            department.UnitId == unitId &&
            string.Equals(NormalizeName(department.Name), normalizedName, StringComparison.Ordinal)))
        {
            throw new DomainException("organization.department.duplicate_name", "The department already exists in this scope.");
        }

        var department = new Department(Id, normalizedName, unitId, id);
        _departments.Add(department);
        UpdatedAt = DateTime.UtcNow;
        return department;
    }

    public void MoveUnit(string unitId, string? parentUnitId)
    {
        EnsureActive();
        var unit = FindUnit(unitId);
        if (parentUnitId == unitId)
        {
            throw new DomainException("organization.unit.self_parent", "A unit cannot be its own parent.");
        }

        if (parentUnitId is null)
        {
            unit.ChangeParent(null);
            UpdatedAt = DateTime.UtcNow;
            return;
        }

        _ = FindUnit(parentUnitId);
        var visited = new HashSet<string>(StringComparer.Ordinal) { unitId };
        var current = parentUnitId;
        while (current is not null)
        {
            if (!visited.Add(current))
            {
                throw new DomainException("organization.unit.cycle", "Moving the unit would create a hierarchy cycle.");
            }

            current = FindUnit(current).ParentUnitId;
        }

        unit.ChangeParent(parentUnitId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ArchiveUnit(string unitId, DateOnly effectiveUntil)
    {
        EnsureActive();
        _ = FindUnit(unitId);
        if (_units.Any(child => child.ParentUnitId == unitId && child.Status == OrganizationalUnitStatus.Active))
        {
            throw new DomainException("organization.unit.active_children", "A unit with active subordinate units cannot be archived.");
        }

        FindUnit(unitId).Deactivate(effectiveUntil);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ArchiveDepartment(string departmentId)
    {
        EnsureActive();
        var department = _departments.FirstOrDefault(item => item.Id == departmentId)
            ?? throw new DomainException("organization.department.not_found", "The department does not belong to this organization.");
        department.Archive();
        UpdatedAt = DateTime.UtcNow;
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

    private OrganizationalUnit FindUnit(string unitId) =>
        _units.FirstOrDefault(unit => unit.Id == unitId)
        ?? throw new DomainException("organization.unit.not_found", "The unit does not belong to this organization.");

    private static string NormalizeName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
