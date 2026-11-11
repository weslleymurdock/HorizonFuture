using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Enrollments;
using HF.Domain.SocialServices.Events;
using Xunit;

namespace HF.UnitTests.Domain.SocialServices;

public sealed class ProgramParticipationDomainTests
{
    [Fact]
    public void Start_InitializesActiveParticipationWithAuditEntry()
    {
        var participation = new ProgramParticipation(
            programId: "prog-1",
            beneficiaryPersonId: "person-1",
            enrollmentId: "enroll-1",
            startDate: new DateOnly(2026, 2, 1),
            startedBy: "staff-1");

        Assert.Equal(ParticipationStatus.Active, participation.Status);
        Assert.Equal("prog-1", participation.ProgramId);
        Assert.Equal("person-1", participation.BeneficiaryPersonId);
        Assert.Equal("enroll-1", participation.EnrollmentId);
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
    public void SuspendAndResume_PreservesCompleteHistoricalTrail()
    {
        var participation = new ProgramParticipation(
            "prog-1", "person-1", "enroll-1", new DateOnly(2026, 2, 1), "staff-1");

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
    public void ChronologyValidation_RejectsDatesEarlierThanStartOrPreviousTransition()
    {
        var participation = new ProgramParticipation(
            "prog-1", "person-1", "enroll-1", new DateOnly(2026, 3, 1), "staff-1");

        // Before start date
        var exBeforeStart = Assert.Throws<DomainException>(() =>
            participation.Suspend(new DateOnly(2026, 2, 28), "Motivo", "staff-1"));
        Assert.Equal("participation.invalid_period", exBeforeStart.Code);

        participation.Suspend(new DateOnly(2026, 4, 10), "Viagem familiar", "staff-1");

        // Before last transition
        var exBeforePrevious = Assert.Throws<DomainException>(() =>
            participation.Resume(new DateOnly(2026, 4, 5), "staff-1"));
        Assert.Equal("participation.invalid_chronology", exBeforePrevious.Code);
    }

    [Fact]
    public void Terminate_SetsEndDateAndRecordsReason_AndRejectsFurtherAlteration()
    {
        var participation = new ProgramParticipation(
            "prog-1", "person-1", "enroll-1", new DateOnly(2026, 2, 1), "staff-1");

        var exNoReason = Assert.Throws<DomainException>(() =>
            participation.Terminate(new DateOnly(2026, 5, 1), "   ", "staff-1"));
        Assert.Equal("participation.termination_reason_required", exNoReason.Code);

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
