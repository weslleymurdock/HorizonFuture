using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Enrollments;
using HF.Domain.SocialServices.Events;
using HF.Domain.SocialServices.Programs;
using Xunit;

namespace HF.UnitTests.Domain.SocialServices;

public sealed class ProgramParticipationDomainTests
{
    private static (SocialProgram Program, ProgramEnrollment Enrollment) CreateApprovedEnrollment(
        string programId = "prog-1",
        string personId = "person-1")
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Teste", "Público", "unit-1", period, id: programId);
        program.Activate();

        var enrollment = new ProgramEnrollment(program, personId, new DateOnly(2026, 2, 1), "staff-1");
        enrollment.Approve(program, "coordenador-1", new DateOnly(2026, 2, 2));
        return (program, enrollment);
    }

    [Fact]
    public void Start_InitializesActiveParticipationWithAuditEntry()
    {
        var (_, enrollment) = CreateApprovedEnrollment();

        var participation = enrollment.CreateParticipation(
            startDate: new DateOnly(2026, 2, 1),
            startedBy: "staff-1");

        Assert.Equal(ParticipationStatus.Active, participation.Status);
        Assert.Equal(enrollment.ProgramId, participation.ProgramId);
        Assert.Equal(enrollment.BeneficiaryPersonId, participation.BeneficiaryPersonId);
        Assert.Equal(enrollment.Id, participation.EnrollmentId);
        Assert.Equal(new DateOnly(2026, 2, 1), participation.StartDate);
        Assert.Null(participation.EndDate);

        Assert.Single(participation.History);
        var initial = participation.History[0];
        Assert.Equal(ParticipationStatus.Active, initial.NewStatus);
        Assert.Equal(new DateOnly(2026, 2, 1), initial.TransitionDate);
        Assert.Equal("staff-1", initial.ChangedBy);

        Assert.Single(participation.DomainEvents);
        Assert.IsType<ProgramParticipationStartedEvent>(participation.DomainEvents.First());
    }

    [Fact]
    public void DuplicateParticipation_RejectedAndStatePreserved()
    {
        var (_, enrollment) = CreateApprovedEnrollment();

        // 1. First creation is allowed
        var participation1 = enrollment.CreateParticipation(new DateOnly(2026, 2, 10), "staff-1");
        Assert.NotNull(participation1);
        Assert.Equal(enrollment.Id, participation1.EnrollmentId);
        Assert.True(enrollment.HasParticipation);
        Assert.Equal(participation1.Id, enrollment.ParticipationId);

        // 2. Second creation is rejected
        var exDuplicate = Assert.Throws<DomainException>(() =>
            enrollment.CreateParticipation(new DateOnly(2026, 2, 11), "staff-2"));
        Assert.Equal("enrollment.participation_already_created", exDuplicate.Code);

        // 3. Rejection does not improperly alter the enrollment state
        Assert.Equal(EnrollmentStatus.Approved, enrollment.Status);
        Assert.Equal(participation1.Id, enrollment.ParticipationId);

        // 4. Cannot create participation from non-approved enrollment
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa 2", "Público", "unit-1", period);
        program.Activate();
        var pendingEnrollment = new ProgramEnrollment(program, "person-2", new DateOnly(2026, 2, 1), "staff-1");
        var exNotApproved = Assert.Throws<DomainException>(() =>
            pendingEnrollment.CreateParticipation(new DateOnly(2026, 2, 1), "staff-1"));
        Assert.Equal("enrollment.not_approved", exNotApproved.Code);
    }

    [Fact]
    public void SuspendAndResume_PreservesCompleteHistoricalTrail()
    {
        var (_, enrollment) = CreateApprovedEnrollment();
        var participation = enrollment.CreateParticipation(new DateOnly(2026, 2, 1), "staff-1");

        // Suspend
        participation.Suspend(new DateOnly(2026, 4, 1), "Afastamento médico temporário", "assistente-social-1");
        Assert.Equal(ParticipationStatus.Suspended, participation.Status);
        Assert.Equal(2, participation.History.Count);

        var suspendEntry = participation.History[1];
        Assert.Equal(ParticipationStatus.Active, suspendEntry.PreviousStatus);
        Assert.Equal(ParticipationStatus.Suspended, suspendEntry.NewStatus);
        Assert.Equal("Afastamento médico temporário", suspendEntry.Reason);

        // Resume
        participation.Resume(new DateOnly(2026, 5, 2), "assistente-social-1");
        Assert.Equal(ParticipationStatus.Active, participation.Status);
        Assert.Equal(3, participation.History.Count);

        var resumeEntry = participation.History[2];
        Assert.Equal(ParticipationStatus.Suspended, resumeEntry.PreviousStatus);
        Assert.Equal(ParticipationStatus.Active, resumeEntry.NewStatus);

        // Complete
        participation.Complete(new DateOnly(2026, 12, 15), "Concluiu todas as oficinas com êxito", "coordenador-1");
        Assert.Equal(ParticipationStatus.Completed, participation.Status);
        Assert.Equal(new DateOnly(2026, 12, 15), participation.EndDate);
        Assert.Equal(4, participation.History.Count);
    }

    [Fact]
    public void TransitionAtomicity_InvalidArguments_PreservesAggregateStateWithoutPartialMutations()
    {
        var (_, enrollment) = CreateApprovedEnrollment();
        var participation = enrollment.CreateParticipation(new DateOnly(2026, 2, 1), "staff-1");

        // 1. Suspend with blank changedBy
        Assert.Throws<ArgumentException>(() =>
            participation.Suspend(new DateOnly(2026, 4, 1), "Motivo válido", "   "));
        Assert.Equal(ParticipationStatus.Active, participation.Status);
        Assert.Single(participation.History);
        Assert.Single(participation.DomainEvents);
        Assert.Null(participation.EndDate);

        // 2. Suspend with blank reason
        var exReason = Assert.Throws<DomainException>(() =>
            participation.Suspend(new DateOnly(2026, 4, 1), "   ", "staff-1"));
        Assert.Equal("participation.suspension_reason_required", exReason.Code);
        Assert.Equal(ParticipationStatus.Active, participation.Status);
        Assert.Single(participation.History);

        // Valid suspend
        participation.Suspend(new DateOnly(2026, 4, 1), "Afastamento temporário", "staff-1");
        Assert.Equal(ParticipationStatus.Suspended, participation.Status);
        Assert.Equal(2, participation.History.Count);
        Assert.Equal(2, participation.DomainEvents.Count);

        // 3. Resume with blank changedBy
        Assert.Throws<ArgumentException>(() =>
            participation.Resume(new DateOnly(2026, 5, 1), "   "));
        Assert.Equal(ParticipationStatus.Suspended, participation.Status);
        Assert.Equal(2, participation.History.Count);
        Assert.Equal(2, participation.DomainEvents.Count);

        // Valid resume
        participation.Resume(new DateOnly(2026, 5, 1), "staff-1");
        Assert.Equal(ParticipationStatus.Active, participation.Status);
        Assert.Equal(3, participation.History.Count);

        // 4. Complete with blank changedBy
        Assert.Throws<ArgumentException>(() =>
            participation.Complete(new DateOnly(2026, 12, 1), "Notas", "   "));
        Assert.Equal(ParticipationStatus.Active, participation.Status);
        Assert.Null(participation.EndDate);
        Assert.Equal(3, participation.History.Count);

        // 5. Terminate with blank changedBy or reason
        Assert.Throws<ArgumentException>(() =>
            participation.Terminate(new DateOnly(2026, 12, 1), "Motivo", "   "));
        Assert.Equal(ParticipationStatus.Active, participation.Status);
        Assert.Null(participation.EndDate);
        Assert.Equal(3, participation.History.Count);
    }

    [Fact]
    public void ChronologyValidation_RejectsDatesEarlierThanStartOrPreviousTransition()
    {
        var (_, enrollment) = CreateApprovedEnrollment();
        var participation = enrollment.CreateParticipation(new DateOnly(2026, 3, 1), "staff-1");

        // Before start date
        var exBeforeStart = Assert.Throws<DomainException>(() =>
            participation.Suspend(new DateOnly(2026, 2, 28), "Motivo", "staff-1"));
        Assert.Equal("participation.invalid_period", exBeforeStart.Code);
        Assert.Equal(ParticipationStatus.Active, participation.Status);

        participation.Suspend(new DateOnly(2026, 4, 10), "Viagem familiar", "staff-1");

        // Before last transition
        var exBeforePrevious = Assert.Throws<DomainException>(() =>
            participation.Resume(new DateOnly(2026, 4, 5), "staff-1"));
        Assert.Equal("participation.invalid_chronology", exBeforePrevious.Code);
        Assert.Equal(ParticipationStatus.Suspended, participation.Status);
    }

    [Fact]
    public void Terminate_SetsEndDateAndRecordsReason_AndRejectsFurtherAlteration()
    {
        var (_, enrollment) = CreateApprovedEnrollment();
        var participation = enrollment.CreateParticipation(new DateOnly(2026, 2, 1), "staff-1");

        var exNoReason = Assert.Throws<DomainException>(() =>
            participation.Terminate(new DateOnly(2026, 5, 1), "   ", "staff-1"));
        Assert.Equal("participation.termination_reason_required", exNoReason.Code);
        Assert.Equal(ParticipationStatus.Active, participation.Status);

        participation.Terminate(new DateOnly(2026, 5, 1), "Mudança de cidade da família", "assistente-social-1");
        Assert.Equal(ParticipationStatus.Terminated, participation.Status);
        Assert.Equal(new DateOnly(2026, 5, 1), participation.EndDate);

        // Cannot suspend or complete already terminated participation
        var exSuspend = Assert.Throws<DomainException>(() =>
            participation.Suspend(new DateOnly(2026, 6, 1), "Motivo", "staff-1"));
        Assert.Equal("participation.not_active", exSuspend.Code);

        var exTerminateAgain = Assert.Throws<DomainException>(() =>
            participation.Terminate(new DateOnly(2026, 6, 1), "Outro motivo", "staff-1"));
        Assert.Equal("participation.already_ended", exTerminateAgain.Code);
    }
}
