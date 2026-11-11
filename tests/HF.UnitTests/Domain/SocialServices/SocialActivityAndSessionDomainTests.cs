using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Activities;
using HF.Domain.SocialServices.Events;
using Xunit;

namespace HF.UnitTests.Domain.SocialServices;

public sealed class SocialActivityAndSessionDomainTests
{
    [Fact]
    public void Activity_ScheduleSession_SetsTimesAndEmitsEvents()
    {
        var activity = new SocialActivity(
            programId: "prog-artes",
            title: "Oficina de Percussão e Ritmos",
            description: "Atividade prática semanal de musicalização e percussão corporal",
            objectives: "Coordenação motora, trabalho em equipe e expressão cultural");

        // Cannot schedule if not active
        var exNotActive = Assert.Throws<DomainException>(() =>
            activity.ScheduleSession(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2), "Sala 3"));
        Assert.Equal("activity.not_active", exNotActive.Code);

        activity.Activate();
        Assert.Equal(ActivityStatus.Active, activity.Status);

        var start = DateTime.UtcNow.AddDays(2);
        var end = start.AddHours(2);
        var session = activity.ScheduleSession(start, end, "Auditório Central", "Ensaio Geral 01");

        Assert.Equal(activity.Id, session.ActivityId);
        Assert.Equal(activity.ProgramId, session.ProgramId);
        Assert.Equal("Ensaio Geral 01", session.Title);
        Assert.Equal(ActivitySessionStatus.Scheduled, session.Status);
        Assert.Contains(session.DomainEvents, e => e is ActivitySessionScheduledEvent);
    }

    [Fact]
    public void Session_Facilitators_AssignAndRejectDuplicates()
    {
        var session = new ActivitySession(
            "act-1", "prog-1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2), "Sala 1", "Sessão 01");

        session.AddFacilitator("educador-1", FacilitatorRole.LeadFacilitator);
        session.AddFacilitator("educador-2", FacilitatorRole.Assistant);

        Assert.Equal(2, session.Facilitators.Count);

        var exDuplicate = Assert.Throws<DomainException>(() =>
            session.AddFacilitator("educador-1", FacilitatorRole.LeadFacilitator));
        Assert.Equal("session.duplicate_facilitator", exDuplicate.Code);
    }

    [Fact]
    public void Session_Attendance_RecordsInAllowedStates_AndRejectsInvalidStatus()
    {
        var start = new DateTime(2026, 4, 10, 14, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 4, 10, 16, 0, 0, DateTimeKind.Utc);
        var session = new ActivitySession(
            "act-1", "prog-1", start, end, "Sala 1", "Sessão 01");

        // Record in Scheduled state is allowed (pre-registration / check-in)
        session.RecordAttendance("person-scheduled", AttendanceStatus.Present, "educador-1");
        Assert.Single(session.Attendance);

        // Start session (InProgress)
        session.Start(start);
        Assert.Equal(ActivitySessionStatus.InProgress, session.Status);

        // Present
        session.RecordAttendance("person-1", AttendanceStatus.Present, "educador-1");

        // Absent
        session.RecordAttendance("person-2", AttendanceStatus.Absent, "educador-1");

        // Excused
        session.RecordAttendance("person-excused", AttendanceStatus.Excused, "educador-1");

        // Justified absence without justification throws
        var exJustification = Assert.Throws<DomainException>(() =>
            session.RecordAttendance("person-3", AttendanceStatus.JustifiedAbsence, "educador-1", "   "));
        Assert.Equal("attendance.justification_required", exJustification.Code);

        // Justified absence with valid justification
        session.RecordAttendance("person-3", AttendanceStatus.JustifiedAbsence, "educador-1", "Atestado médico de consulta pediátrica");

        // Invalid enum value rejected
        var exInvalidEnum = Assert.Throws<DomainException>(() =>
            session.RecordAttendance("person-4", (AttendanceStatus)999, "educador-1"));
        Assert.Equal("attendance.invalid_status", exInvalidEnum.Code);

        // NotRecorded rejected as explicit record
        var exNotRecorded = Assert.Throws<DomainException>(() =>
            session.RecordAttendance("person-5", AttendanceStatus.NotRecorded, "educador-1"));
        Assert.Equal("attendance.invalid_status", exNotRecorded.Code);

        // Duplicate attendance for same person is rejected
        var exDuplicate = Assert.Throws<DomainException>(() =>
            session.RecordAttendance("person-1", AttendanceStatus.Present, "educador-1"));
        Assert.Equal("attendance.duplicate_record", exDuplicate.Code);

        // Complete session
        session.Complete(end);
        Assert.Equal(ActivitySessionStatus.Completed, session.Status);
        Assert.Contains(session.DomainEvents, e => e is ActivitySessionCompletedEvent);
    }

    [Fact]
    public void Session_Completed_RejectsAttendanceModificationAndPreservesRecords()
    {
        var start = new DateTime(2026, 4, 10, 14, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 4, 10, 16, 0, 0, DateTimeKind.Utc);
        var session = new ActivitySession(
            "act-1", "prog-1", start, end, "Sala 1", "Sessão Concluída");

        session.Start(start);
        session.RecordAttendance("person-1", AttendanceStatus.Present, "educador-1");
        session.Complete(end);

        Assert.Equal(ActivitySessionStatus.Completed, session.Status);
        Assert.Single(session.Attendance);

        // Attempting to record attendance on completed session must be rejected
        var exCompleted = Assert.Throws<DomainException>(() =>
            session.RecordAttendance("person-2", AttendanceStatus.Present, "educador-1"));
        Assert.Equal("session.completed", exCompleted.Code);

        // Verify previous records remain intact
        Assert.Single(session.Attendance);
        Assert.Equal("person-1", session.Attendance[0].BeneficiaryPersonId);
    }

    [Fact]
    public void Session_Cancelled_RejectsAttendanceRecordingAndStoresCancellationAudit()
    {
        var session = new ActivitySession(
            "act-1", "prog-1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2), "Quadra Poliesportiva", "Treino de Futsal");

        session.Cancel("Chuva forte e alagamento na via de acesso", "coordenador-1");
        Assert.Equal(ActivitySessionStatus.Cancelled, session.Status);
        Assert.Equal("Chuva forte e alagamento na via de acesso", session.CancellationReason);
        Assert.Equal("coordenador-1", session.CancelledByPersonId);

        var exAttendance = Assert.Throws<DomainException>(() =>
            session.RecordAttendance("person-1", AttendanceStatus.Present, "educador-1"));
        Assert.Equal("session.cancelled", exAttendance.Code);

        var exAddFacilitator = Assert.Throws<DomainException>(() =>
            session.AddFacilitator("educador-1", FacilitatorRole.Assistant));
        Assert.Equal("session.not_modifiable", exAddFacilitator.Code);
    }

    [Fact]
    public void Session_LifecycleAndTemporalIntegrity_EnforcesTransitionsAndPreservesStateOnFailure()
    {
        var scheduledStart = new DateTime(2026, 5, 20, 14, 0, 0, DateTimeKind.Utc);
        var scheduledEnd = new DateTime(2026, 5, 20, 16, 0, 0, DateTimeKind.Utc);
        var session = new ActivitySession("act-1", "prog-1", scheduledStart, scheduledEnd, "Sala 2", "Oficina de Robótica");

        // 1. Complete without starting must fail
        var exCompleteWithoutStart = Assert.Throws<DomainException>(() =>
            session.Complete(scheduledEnd));
        Assert.Equal("session.not_in_progress", exCompleteWithoutStart.Code);
        Assert.Equal(ActivitySessionStatus.Scheduled, session.Status);

        // 2. Start before scheduled start must fail
        var exStartBefore = Assert.Throws<DomainException>(() =>
            session.Start(scheduledStart.AddMinutes(-10)));
        Assert.Equal("session.start_before_scheduled", exStartBefore.Code);
        Assert.Equal(ActivitySessionStatus.Scheduled, session.Status);
        Assert.Null(session.ActualStart);

        // 3. Valid start (even with delayed start)
        var actualStart = scheduledStart.AddMinutes(15);
        session.Start(actualStart);
        Assert.Equal(ActivitySessionStatus.InProgress, session.Status);
        Assert.Equal(actualStart, session.ActualStart);
        Assert.Contains(session.DomainEvents, e => e is ActivitySessionStartedEvent);

        // 4. Repeated start must fail
        var exRepeatedStart = Assert.Throws<DomainException>(() =>
            session.Start(actualStart.AddMinutes(5)));
        Assert.Equal("session.already_started", exRepeatedStart.Code);
        Assert.Equal(ActivitySessionStatus.InProgress, session.Status);

        // 5. Complete with time before actual start must fail
        var exEndBeforeStart = Assert.Throws<DomainException>(() =>
            session.Complete(actualStart.AddMinutes(-5)));
        Assert.Equal("session.invalid_end_time", exEndBeforeStart.Code);
        Assert.Equal(ActivitySessionStatus.InProgress, session.Status);
        Assert.Null(session.ActualEnd);

        // 6. Valid completion
        var actualEnd = actualStart.AddHours(2);
        session.Complete(actualEnd);
        Assert.Equal(ActivitySessionStatus.Completed, session.Status);
        Assert.Equal(actualEnd, session.ActualEnd);

        // 7. Repeated completion must fail
        var exRepeatedComplete = Assert.Throws<DomainException>(() =>
            session.Complete(actualEnd.AddMinutes(5)));
        Assert.Equal("session.already_completed", exRepeatedComplete.Code);

        // 8. Cannot start or reschedule a completed session
        var exStartCompleted = Assert.Throws<DomainException>(() =>
            session.Start(actualStart));
        Assert.Equal("session.already_completed", exStartCompleted.Code);

        var exRescheduleCompleted = Assert.Throws<DomainException>(() =>
            session.Reschedule(scheduledStart.AddDays(1), scheduledEnd.AddDays(1), "Motivo", "coordenador-1"));
        Assert.Equal("session.not_modifiable", exRescheduleCompleted.Code);

        // 9. Cancelled session cannot be started or completed
        var cancelledSession = new ActivitySession("act-2", "prog-1", scheduledStart, scheduledEnd, "Sala 3", "Oficina Teatro");
        cancelledSession.Cancel("Falta de energia", "coordenador-1");

        var exStartCancelled = Assert.Throws<DomainException>(() =>
            cancelledSession.Start(scheduledStart));
        Assert.Equal("session.cannot_start_cancelled", exStartCancelled.Code);

        var exCompleteCancelled = Assert.Throws<DomainException>(() =>
            cancelledSession.Complete(scheduledEnd));
        Assert.Equal("session.cannot_complete_cancelled", exCompleteCancelled.Code);

        // 10. Reschedule validations
        var sessionToReschedule = new ActivitySession("act-3", "prog-1", scheduledStart, scheduledEnd, "Sala 4", "Música");
        Assert.Throws<ArgumentException>(() =>
            sessionToReschedule.Reschedule(scheduledStart.AddDays(1), scheduledEnd.AddDays(1), "Motivo", "   "));
        var exRescheduleNoReason = Assert.Throws<DomainException>(() =>
            sessionToReschedule.Reschedule(scheduledStart.AddDays(1), scheduledEnd.AddDays(1), "   ", "coordenador-1"));
        Assert.Equal("session.reschedule_reason_required", exRescheduleNoReason.Code);
        var exRescheduleInvalidSchedule = Assert.Throws<DomainException>(() =>
            sessionToReschedule.Reschedule(scheduledEnd.AddDays(1), scheduledStart.AddDays(1), "Motivo", "coordenador-1"));
        Assert.Equal("session.invalid_schedule", exRescheduleInvalidSchedule.Code);

        // Valid reschedule
        sessionToReschedule.Reschedule(scheduledStart.AddDays(1), scheduledEnd.AddDays(1), "Reforma da sala", "coordenador-1");
        Assert.Equal(ActivitySessionStatus.Rescheduled, sessionToReschedule.Status);
        Assert.Equal("coordenador-1", sessionToReschedule.RescheduledByPersonId);

        // Cannot reschedule when in progress
        sessionToReschedule.Start(scheduledStart.AddDays(1));
        var exRescheduleInProgress = Assert.Throws<DomainException>(() =>
            sessionToReschedule.Reschedule(scheduledStart.AddDays(2), scheduledEnd.AddDays(2), "Outro reagendamento", "coordenador-1"));
        Assert.Equal("session.cannot_reschedule_in_progress", exRescheduleInProgress.Code);

        // 11. RecordCompleted factory produces valid retroactively completed session
        var retroSession = ActivitySession.RecordCompleted(
            "act-4", "prog-1", scheduledStart, scheduledEnd, scheduledStart, scheduledEnd, "Sala 5", "Histórico");
        Assert.Equal(ActivitySessionStatus.Completed, retroSession.Status);
        Assert.Equal(scheduledStart, retroSession.ActualStart);
        Assert.Equal(scheduledEnd, retroSession.ActualEnd);
    }

    [Fact]
    public void SocialActivity_Cancel_RequiresAuthorAndReason()
    {
        var activity = new SocialActivity("prog-1", "Oficina de Xadrez");
        Assert.Throws<ArgumentException>(() => activity.Cancel("Motivo", "   "));
        var exNoReason = Assert.Throws<DomainException>(() => activity.Cancel("   ", "coordenador-1"));
        Assert.Equal("activity.cancellation_reason_required", exNoReason.Code);

        activity.Cancel("Falta de inscritos", "coordenador-1");
        Assert.Equal(ActivityStatus.Cancelled, activity.Status);
        Assert.Equal("Falta de inscritos", activity.CancellationReason);
        Assert.Equal("coordenador-1", activity.CancelledByPersonId);

        // Cannot cancel again
        var exAlreadyCancelled = Assert.Throws<DomainException>(() =>
            activity.Cancel("Outro motivo", "coordenador-2"));
        Assert.Equal("activity.already_cancelled", exAlreadyCancelled.Code);
    }
}
