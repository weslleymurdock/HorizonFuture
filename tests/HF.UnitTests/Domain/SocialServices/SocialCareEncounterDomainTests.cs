using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Encounters;
using HF.Domain.SocialServices.Events;
using HF.Domain.SocialServices.Evidences;
using HF.Domain.Stakeholders;
using Xunit;

namespace HF.UnitTests.Domain.SocialServices;

public sealed class SocialCareEncounterDomainTests
{
    [Fact]
    public void Schedule_InitializesScheduledEncounter()
    {
        var person = new Person("Joaquim de Oliveira");
        var scheduledTime = DateTime.UtcNow.AddDays(2);

        var encounter = new SocialCareEncounter(
            beneficiaryPersonId: person.Id,
            organizationalUnitId: "unit-1",
            attendantPersonId: "staff-social-1",
            encounterType: EncounterType.Individual,
            scheduledDateTime: scheduledTime,
            programId: "prog-1",
            initialNotes: "Atendimento inicial de acolhimento");

        Assert.Equal(EncounterStatus.Scheduled, encounter.Status);
        Assert.Equal(person.Id, encounter.BeneficiaryPersonId);
        Assert.Equal("prog-1", encounter.ProgramId);
        Assert.Equal("unit-1", encounter.OrganizationalUnitId);
        Assert.Contains(encounter.DomainEvents, e => e is SocialCareEncounterScheduledEvent);
    }

    [Fact]
    public void Complete_SetsOperationalSummaryAndEncapsulatedSensitiveNotes()
    {
        var person = new Person("Lucia Helena");
        var encounter = new SocialCareEncounter(
            person.Id, "unit-1", "staff-1", EncounterType.HomeVisit, DateTime.UtcNow.AddHours(-2));

        var sensitiveNotes = new SensitiveCareNotes(
            content: "Família reside em moradia cedida; situação de desemprego recente do responsável familiar.",
            legitimatePurpose: "Avaliação socioeconômica para inclusão em programa de auxílio nutricional",
            authorPersonId: "assistente-social-1",
            sensitivityLevel: EvidenceSensitivityLevel.Confidential);

        var completedTime = DateTime.UtcNow.AddMinutes(-30);
        encounter.Complete(
            completedTime,
            attendantPersonId: "assistente-social-1",
            operationalSummary: "Visita domiciliar realizada; verificação das condições de acolhimento e escuta social.",
            sensitiveNotes: sensitiveNotes);

        Assert.Equal(EncounterStatus.Completed, encounter.Status);
        Assert.Equal(completedTime, encounter.EncounterDateTime);
        Assert.NotNull(encounter.SensitiveNotes);
        Assert.Equal("Avaliação socioeconômica para inclusão em programa de auxílio nutricional", encounter.SensitiveNotes.LegitimatePurpose);
        Assert.Contains(encounter.DomainEvents, e => e is SocialCareEncounterCompletedEvent);
    }

    [Fact]
    public void Complete_RejectsFutureDateAndEmptySummary()
    {
        var encounter = new SocialCareEncounter(
            "person-1", "unit-1", "staff-1", EncounterType.Orientation, DateTime.UtcNow);

        // Future date
        var exFuture = Assert.Throws<DomainException>(() =>
            encounter.Complete(DateTime.UtcNow.AddDays(2), "staff-1", "Resumo"));
        Assert.Equal("encounter.future_date", exFuture.Code);

        // Empty summary
        var exEmpty = Assert.Throws<DomainException>(() =>
            encounter.Complete(DateTime.UtcNow.AddMinutes(-5), "staff-1", "   "));
        Assert.Equal("encounter.summary_required", exEmpty.Code);
    }

    [Fact]
    public void SensitiveCareNotes_RequiresContentAndLegitimatePurpose()
    {
        var exContent = Assert.Throws<DomainException>(() =>
            new SensitiveCareNotes("   ", "Finalidade", "staff-1"));
        Assert.Equal("care_notes.empty_content", exContent.Code);

        var exPurpose = Assert.Throws<DomainException>(() =>
            new SensitiveCareNotes("Observação sobre a família", "   ", "staff-1"));
        Assert.Equal("care_notes.purpose_required", exPurpose.Code);
    }

    [Fact]
    public void Cancel_PreservesAuditAndBlocksCompletedOrCancelledModifications()
    {
        var encounter = new SocialCareEncounter(
            "person-1", "unit-1", "staff-1", EncounterType.Screening, DateTime.UtcNow);

        var exNoReason = Assert.Throws<DomainException>(() =>
            encounter.Cancel("   ", "staff-1"));
        Assert.Equal("encounter.cancellation_reason_required", exNoReason.Code);

        encounter.Cancel("Beneficiário comunicou impossibilidade de comparecimento", "staff-1");
        Assert.Equal(EncounterStatus.Cancelled, encounter.Status);
        Assert.Equal("Beneficiário comunicou impossibilidade de comparecimento", encounter.CancellationReason);
        Assert.Equal("staff-1", encounter.CancelledByPersonId);

        // Cannot complete cancelled
        var exComplete = Assert.Throws<DomainException>(() =>
            encounter.Complete(DateTime.UtcNow.AddMinutes(-1), "staff-1", "Resumo"));
        Assert.Equal("encounter.cannot_complete", exComplete.Code);

        // Cannot attach evidence when cancelled
        var exAttach = Assert.Throws<DomainException>(() =>
            encounter.AttachEvidence("ev-01"));
        Assert.Equal("encounter.cannot_modify_cancelled", exAttach.Code);

        // Invalid enum in constructor rejected
        var exInvalidType = Assert.Throws<DomainException>(() =>
            new SocialCareEncounter("person-1", "unit-1", "staff-1", (EncounterType)999, DateTime.UtcNow.AddDays(1)));
        Assert.Equal("encounter.invalid_type", exInvalidType.Code);
    }

    [Fact]
    public void RecordNoShow_PreservesAuditAndRejectsOnCompleted()
    {
        var encounter = new SocialCareEncounter(
            "person-1", "unit-1", "staff-1", EncounterType.Screening, DateTime.UtcNow);

        encounter.RecordNoShow("recepcionista-1");
        Assert.Equal(EncounterStatus.NoShow, encounter.Status);
        Assert.Equal("recepcionista-1", encounter.NoShowRecordedByPersonId);

        // Completed encounter rejects no-show
        var completedEncounter = new SocialCareEncounter(
            "person-2", "unit-1", "staff-1", EncounterType.Individual, DateTime.UtcNow.AddHours(-1));
        completedEncounter.Complete(DateTime.UtcNow.AddMinutes(-10), "staff-1", "Atendimento realizado");

        var exNoShow = Assert.Throws<DomainException>(() =>
            completedEncounter.RecordNoShow("recepcionista-1"));
        Assert.Equal("encounter.cannot_record_noshow", exNoShow.Code);
    }
}
