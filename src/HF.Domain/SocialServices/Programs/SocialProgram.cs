using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Eligibility;
using HF.Domain.SocialServices.Events;

namespace HF.Domain.SocialServices.Programs;

public sealed class SocialProgram : AggregateRoot
{
    private readonly List<EligibilityCriterionBase> _eligibilityCriteria = [];
    private readonly List<ServiceOffering> _offerings = [];

    private SocialProgram() { }

    public SocialProgram(
        string name,
        string targetAudience,
        string organizationalUnitId,
        OperatingPeriod operatingPeriod,
        string? description = null,
        string? objectives = null,
        string? location = null,
        string? departmentId = null,
        string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetAudience);
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationalUnitId);
        ArgumentNullException.ThrowIfNull(operatingPeriod);

        Name = NormalizeText(name);
        TargetAudience = NormalizeText(targetAudience);
        OrganizationalUnitId = organizationalUnitId.Trim();
        OperatingPeriod = operatingPeriod;
        Description = NormalizeOptional(description);
        Objectives = NormalizeOptional(objectives);
        Location = NormalizeOptional(location);
        DepartmentId = NormalizeOptional(departmentId);
        Status = ProgramStatus.Draft;

        AddDomainEvent(new SocialProgramCreatedEvent(Id, Name, OrganizationalUnitId));
    }

    public string Name { get; private set; } = string.Empty;
    public string TargetAudience { get; private set; } = string.Empty;
    public string OrganizationalUnitId { get; private set; } = string.Empty;
    public string? DepartmentId { get; private set; }
    public string? Location { get; private set; }
    public OperatingPeriod OperatingPeriod { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Objectives { get; private set; }
    public ProgramStatus Status { get; private set; }
    public string? SuspensionReason { get; private set; }
    public DateOnly? CompletionDate { get; private set; }

    public IReadOnlyList<EligibilityCriterionBase> EligibilityCriteria => _eligibilityCriteria.AsReadOnly();
    public IReadOnlyList<ServiceOffering> Offerings => _offerings.AsReadOnly();

    public bool CanAcceptEnrollments() => Status == ProgramStatus.Active;

    public void EnsureCanAcceptEnrollments()
    {
        if (Status != ProgramStatus.Active)
        {
            throw new DomainException("program.not_active", $"Program '{Name}' cannot accept enrollments in state '{Status}'.");
        }
    }

    public void Activate()
    {
        if (Status == ProgramStatus.Active)
        {
            throw new DomainException("program.already_active", "The program is already active.");
        }

        if (Status == ProgramStatus.Archived)
        {
            throw new DomainException("program.already_archived", "An archived program cannot be activated.");
        }

        if (Status == ProgramStatus.Completed)
        {
            throw new DomainException("program.already_completed", "A completed program cannot be activated.");
        }

        Status = ProgramStatus.Active;
        SuspensionReason = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SocialProgramActivatedEvent(Id));
    }

    public void Suspend(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("program.suspension_reason_required", "A reason is required to suspend a program.");
        }

        if (Status == ProgramStatus.Suspended)
        {
            throw new DomainException("program.already_suspended", "The program is already suspended.");
        }

        if (Status != ProgramStatus.Active)
        {
            throw new DomainException("program.cannot_suspend", $"Cannot suspend a program in state '{Status}'.");
        }

        Status = ProgramStatus.Suspended;
        SuspensionReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SocialProgramSuspendedEvent(Id, SuspensionReason));
    }

    public void Complete(DateOnly completionDate)
    {
        if (Status == ProgramStatus.Completed)
        {
            throw new DomainException("program.already_completed", "The program is already completed.");
        }

        if (Status == ProgramStatus.Archived)
        {
            throw new DomainException("program.already_archived", "An archived program cannot be completed.");
        }

        if (Status == ProgramStatus.Draft)
        {
            throw new DomainException("program.cannot_complete", "A draft program cannot be completed.");
        }

        if (completionDate < OperatingPeriod.StartDate)
        {
            throw new DomainException("program.invalid_completion_date", "Completion date cannot be earlier than the operating start date.");
        }

        Status = ProgramStatus.Completed;
        CompletionDate = completionDate;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SocialProgramCompletedEvent(Id, completionDate));
    }

    public void Archive()
    {
        if (Status == ProgramStatus.Archived)
        {
            throw new DomainException("program.already_archived", "The program is already archived.");
        }

        if (Status == ProgramStatus.Active)
        {
            throw new DomainException("program.cannot_archive_active", "An active program must be suspended or completed before archiving.");
        }

        Status = ProgramStatus.Archived;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SocialProgramArchivedEvent(Id));
    }

    public void UpdateProfile(
        string name,
        string targetAudience,
        OperatingPeriod operatingPeriod,
        string? description,
        string? objectives,
        string? location,
        string? departmentId)
    {
        EnsureModifiable();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetAudience);
        ArgumentNullException.ThrowIfNull(operatingPeriod);

        Name = NormalizeText(name);
        TargetAudience = NormalizeText(targetAudience);
        OperatingPeriod = operatingPeriod;
        Description = NormalizeOptional(description);
        Objectives = NormalizeOptional(objectives);
        Location = NormalizeOptional(location);
        DepartmentId = NormalizeOptional(departmentId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddEligibilityCriterion(EligibilityCriterionBase criterion)
    {
        EnsureModifiable();
        ArgumentNullException.ThrowIfNull(criterion);

        if (_eligibilityCriteria.Any(c => string.Equals(c.Code, criterion.Code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("program.duplicate_criterion", $"A criterion with code '{criterion.Code}' already exists in this program.");
        }

        _eligibilityCriteria.Add(criterion);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveEligibilityCriterion(string criterionCode)
    {
        EnsureModifiable();
        ArgumentException.ThrowIfNullOrWhiteSpace(criterionCode);

        var existing = _eligibilityCriteria.FirstOrDefault(c => string.Equals(c.Code, criterionCode.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            throw new DomainException("program.criterion_not_found", $"Criterion with code '{criterionCode}' was not found.");
        }

        _eligibilityCriteria.Remove(existing);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddOffering(ServiceOffering offering)
    {
        EnsureModifiable();
        ArgumentNullException.ThrowIfNull(offering);

        if (_offerings.Any(o => string.Equals(o.Code, offering.Code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("program.duplicate_offering", $"An offering with code '{offering.Code}' already exists in this program.");
        }

        _offerings.Add(offering);
        UpdatedAt = DateTime.UtcNow;
    }

    public EligibilityAssessmentResult EvaluateEligibility(
        ApplicantProfile profile,
        DateOnly referenceDate,
        string assessedBy,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentException.ThrowIfNullOrWhiteSpace(assessedBy);

        var evaluations = new List<CriterionEvaluationResult>();
        var isEligible = true;

        foreach (var criterion in _eligibilityCriteria)
        {
            var result = criterion.Evaluate(profile, referenceDate);
            evaluations.Add(result);

            if (criterion.IsMandatory && !result.IsMet)
            {
                isEligible = false;
            }
        }

        return new EligibilityAssessmentResult(isEligible, evaluations, referenceDate, assessedBy, notes);
    }

    private void EnsureModifiable()
    {
        if (Status is ProgramStatus.Archived or ProgramStatus.Completed)
        {
            throw new DomainException("program.not_modifiable", $"Program cannot be modified in state '{Status}'.");
        }
    }

    private static string NormalizeText(string value) => string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
