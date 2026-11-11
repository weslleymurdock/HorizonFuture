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
    public void Session_Attendance_RecordsPresentAbsentAndJustifiedAbsence()
    {
        var session = new ActivitySession(
            "act-1", "prog-1", DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddHours(-1), "Sala 1", "Sessão 01");

        session.Start(DateTime.UtcNow.AddHours(-3));

        // Present
        session.RecordAttendance("person-1", AttendanceStatus.Present, "educador-1");

        // Absent
        session.RecordAttendance("person-2", AttendanceStatus.Absent, "educador-1");

        // Justified absence without justification throws
        var exJustification = Assert.Throws<DomainException>(() =>
            session.RecordAttendance("person-3", AttendanceStatus.JustifiedAbsence, "educador-1", "   "));
        Assert.Equal("attendance.justification_required", exJustification.Code);

        // Justified absence with valid justification
        session.RecordAttendance("person-3", AttendanceStatus.JustifiedAbsence, "educador-1", "Atestado médico de consulta pediátrica");

        Assert.Equal(3, session.Attendance.Count);
        Assert.Equal(AttendanceStatus.Present, session.Attendance.First(a => a.BeneficiaryPersonId == "person-1").Status);
        Assert.Equal(AttendanceStatus.Absent, session.Attendance.First(a => a.BeneficiaryPersonId == "person-2").Status);
        Assert.Equal(AttendanceStatus.JustifiedAbsence, session.Attendance.First(a => a.BeneficiaryPersonId == "person-3").Status);
        Assert.Equal("Atestado médico de consulta pediátrica", session.Attendance.First(a => a.BeneficiaryPersonId == "person-3").Justification);

        // Duplicate attendance for same person is rejected
        var exDuplicate = Assert.Throws<DomainException>(() =>
            session.RecordAttendance("person-1", AttendanceStatus.Present, "educador-1"));
        Assert.Equal("attendance.duplicate_record", exDuplicate.Code);

        // Complete session
        session.Complete(DateTime.UtcNow.AddHours(-1));
        Assert.Equal(ActivitySessionStatus.Completed, session.Status);
        Assert.Contains(session.DomainEvents, e => e is ActivitySessionCompletedEvent);
    }

    [Fact]
    public void Session_Cancelled_RejectsAttendanceRecordingAndFurtherAlteration()
    {
        var session = new ActivitySession(
            "act-1", "prog-1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2), "Quadra Poliesportiva", "Treino de Futsal");

        session.Cancel("Chuva forte e alagamento na via de acesso", "coordenador-1");
        Assert.Equal(ActivitySessionStatus.Cancelled, session.Status);
        Assert.Equal("Chuva forte e alagamento na via de acesso", session.CancellationReason);

        var exAttendance = Assert.Throws<DomainException>(() =>
            session.RecordAttendance("person-1", AttendanceStatus.Present, "educador-1"));
        Assert.Equal("session.cancelled", exAttendance.Code);

        var exAddFacilitator = Assert.Throws<DomainException>(() =>
            session.AddFacilitator("educador-1", FacilitatorRole.Assistant));
        Assert.Equal("session.not_modifiable", exAddFacilitator.Code);
    }
}
