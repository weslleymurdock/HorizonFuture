using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Eligibility;
using HF.Domain.SocialServices.Events;
using HF.Domain.SocialServices.Programs;

namespace HF.Domain.SocialServices.Enrollments;

public sealed class ProgramEnrollment : AggregateRoot
{
    private ProgramEnrollment() { }

    public ProgramEnrollment(
        SocialProgram program,
        string beneficiaryPersonId,
        DateOnly enrollmentDate,
        string registeredBy,
        string? notes = null,
        string? id = null) : base(id)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(registeredBy);

        program.EnsureCanAcceptEnrollments();

        ProgramId = program.Id;
        BeneficiaryPersonId = beneficiaryPersonId.Trim();
        EnrollmentDate = enrollmentDate;
        RegisteredBy = registeredBy.Trim();
        Status = EnrollmentStatus.Pending;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        AddDomainEvent(new ProgramEnrollmentSubmittedEvent(Id, ProgramId, BeneficiaryPersonId));
    }

    public string ProgramId { get; private set; } = string.Empty;
    public string BeneficiaryPersonId { get; private set; } = string.Empty;
    public DateOnly EnrollmentDate { get; private set; }
    public string RegisteredBy { get; private set; } = string.Empty;
    public EnrollmentStatus Status { get; private set; }
    public EligibilityAssessmentResult? AssessmentResult { get; private set; }
    public DateOnly? DecisionDate { get; private set; }
    public string? DecidedBy { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? WithdrawalReason { get; private set; }
    public string? Notes { get; private set; }
    public string? ParticipationId { get; private set; }

    public bool HasParticipation => !string.IsNullOrEmpty(ParticipationId);

    public void StartReview(string reviewerId)
    {
        EnsureNotTerminal();
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewerId);

        if (Status != EnrollmentStatus.Pending && Status != EnrollmentStatus.Waitlisted)
        {
            throw new DomainException("enrollment.invalid_transition", $"Cannot start review from status '{Status}'.");
        }

        Status = EnrollmentStatus.UnderReview;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AttachAssessment(EligibilityAssessmentResult assessment)
    {
        EnsureNotTerminal();
        ArgumentNullException.ThrowIfNull(assessment);

        if (Status != EnrollmentStatus.Pending && Status != EnrollmentStatus.UnderReview)
        {
            throw new DomainException("enrollment.assessment_not_allowed", $"Cannot attach an assessment to an enrollment in status '{Status}'.");
        }

        AssessmentResult = assessment;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(
        SocialProgram program,
        string approvedBy,
        DateOnly decisionDate,
        EligibilityAssessmentResult? assessment = null)
    {
        EnsureNotTerminal();
        ArgumentNullException.ThrowIfNull(program);
        if (string.IsNullOrWhiteSpace(approvedBy))
        {
            throw new ArgumentException("ApprovedBy cannot be blank.", nameof(approvedBy));
        }

        if (program.Id != ProgramId)
        {
            throw new DomainException("enrollment.program_mismatch", "The supplied program does not match the enrollment's program.");
        }

        program.EnsureCanAcceptEnrollments();

        if (Status == EnrollmentStatus.Approved)
        {
            throw new DomainException("enrollment.already_approved", "The enrollment is already approved.");
        }

        if (Status != EnrollmentStatus.Pending && Status != EnrollmentStatus.UnderReview && Status != EnrollmentStatus.Waitlisted)
        {
            throw new DomainException("enrollment.invalid_transition", $"Cannot approve an enrollment in status '{Status}'.");
        }

        var effectiveAssessment = assessment ?? AssessmentResult;

        if (effectiveAssessment != null && !effectiveAssessment.IsEligible)
        {
            throw new DomainException("enrollment.ineligible", "Cannot approve an enrollment evaluated as ineligible under mandatory criteria.");
        }

        if (effectiveAssessment == null && program.EligibilityCriteria.Any(c => c.IsMandatory))
        {
            throw new DomainException("enrollment.assessment_required", "The program requires an eligibility assessment before approval because mandatory criteria are defined.");
        }

        // All validations succeeded - atomic mutation:
        if (assessment != null)
        {
            AssessmentResult = assessment;
        }

        Status = EnrollmentStatus.Approved;
        DecisionDate = decisionDate;
        DecidedBy = approvedBy.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProgramEnrollmentApprovedEvent(Id, ProgramId, BeneficiaryPersonId, DecidedBy));
    }

    public void Waitlist(string decidedBy, DateOnly decisionDate, string reason)
    {
        EnsureNotTerminal();
        ArgumentException.ThrowIfNullOrWhiteSpace(decidedBy);
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("enrollment.waitlist_reason_required", "A reason is required to waitlist an enrollment.");
        }

        if (Status == EnrollmentStatus.Waitlisted)
        {
            throw new DomainException("enrollment.already_waitlisted", "The enrollment is already waitlisted.");
        }

        if (Status != EnrollmentStatus.Pending && Status != EnrollmentStatus.UnderReview)
        {
            throw new DomainException("enrollment.invalid_transition", $"Cannot waitlist an enrollment in status '{Status}'.");
        }

        Status = EnrollmentStatus.Waitlisted;
        DecisionDate = decisionDate;
        DecidedBy = decidedBy.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProgramEnrollmentWaitlistedEvent(Id, ProgramId, BeneficiaryPersonId, reason.Trim()));
    }

    public void Reject(string rejectedBy, DateOnly decisionDate, string reason)
    {
        EnsureNotTerminal();
        ArgumentException.ThrowIfNullOrWhiteSpace(rejectedBy);
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("enrollment.rejection_reason_required", "A reason is required to reject an enrollment.");
        }

        if (Status == EnrollmentStatus.Approved)
        {
            throw new DomainException("enrollment.cannot_reject_approved", "An approved enrollment cannot be rejected directly. Cancel it instead.");
        }

        Status = EnrollmentStatus.Rejected;
        DecisionDate = decisionDate;
        DecidedBy = rejectedBy.Trim();
        RejectionReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProgramEnrollmentRejectedEvent(Id, ProgramId, BeneficiaryPersonId, RejectionReason, DecidedBy));
    }

    public void Cancel(string cancelledBy, DateOnly cancellationDate, string reason)
    {
        EnsureNotTerminal();
        ArgumentException.ThrowIfNullOrWhiteSpace(cancelledBy);
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("enrollment.cancellation_reason_required", "A reason is required to cancel an enrollment.");
        }

        Status = EnrollmentStatus.Cancelled;
        DecisionDate = cancellationDate;
        DecidedBy = cancelledBy.Trim();
        CancellationReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProgramEnrollmentCancelledEvent(Id, ProgramId, BeneficiaryPersonId, CancellationReason, DecidedBy));
    }

    public void Withdraw(string withdrawnBy, DateOnly withdrawalDate, string reason)
    {
        EnsureNotTerminal();
        ArgumentException.ThrowIfNullOrWhiteSpace(withdrawnBy);
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("enrollment.withdrawal_reason_required", "A reason is required for candidate withdrawal.");
        }

        Status = EnrollmentStatus.Withdrawn;
        DecisionDate = withdrawalDate;
        DecidedBy = withdrawnBy.Trim();
        WithdrawalReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProgramEnrollmentWithdrawnEvent(Id, ProgramId, BeneficiaryPersonId, WithdrawalReason, DecidedBy));
    }

    public ProgramParticipation CreateParticipation(DateOnly startDate, string startedBy)
    {
        if (Status != EnrollmentStatus.Approved)
        {
            throw new DomainException("enrollment.not_approved", "Only an approved enrollment can initiate active participation.");
        }

        if (HasParticipation)
        {
            throw new DomainException("enrollment.participation_already_created", "A participation has already been created for this enrollment.");
        }

        if (string.IsNullOrWhiteSpace(startedBy))
        {
            throw new ArgumentException("StartedBy cannot be blank.", nameof(startedBy));
        }

        var participation = new ProgramParticipation(this, startDate, startedBy);
        ParticipationId = participation.Id;
        UpdatedAt = DateTime.UtcNow;

        return participation;
    }

    private void EnsureNotTerminal()
    {
        if (Status is EnrollmentStatus.Rejected or EnrollmentStatus.Cancelled or EnrollmentStatus.Withdrawn)
        {
            throw new DomainException("enrollment.terminal_state", $"Enrollment is in terminal state '{Status}' and cannot be altered.");
        }
    }
}
