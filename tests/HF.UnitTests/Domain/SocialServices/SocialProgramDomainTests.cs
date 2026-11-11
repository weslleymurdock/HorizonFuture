using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Eligibility;
using HF.Domain.SocialServices.Events;
using HF.Domain.SocialServices.Programs;
using Xunit;

namespace HF.UnitTests.Domain.SocialServices;

public sealed class SocialProgramDomainTests
{
    [Fact]
    public void Constructor_InitializesInDraftAndEmitsCreatedEvent()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
        var program = new SocialProgram(
            name: "Oficinas do Futuro",
            targetAudience: "Crianças e adolescentes de 6 a 15 anos em vulnerabilidade social",
            organizationalUnitId: "unit-central",
            operatingPeriod: period,
            description: "Atividades culturais e esportivas no contraturno escolar",
            objectives: "Desenvolvimento integral e fortalecimento de vínculos familiares",
            location: "Espaço Comunitário Norte");

        Assert.Equal("Oficinas do Futuro", program.Name);
        Assert.Equal(ProgramStatus.Draft, program.Status);
        Assert.False(program.CanAcceptEnrollments());
        Assert.Single(program.DomainEvents);
        Assert.IsType<SocialProgramCreatedEvent>(program.DomainEvents.First());
    }

    [Fact]
    public void OperatingPeriod_RejectsEndDateEarlierThanStartDate()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new OperatingPeriod(new DateOnly(2026, 6, 1), new DateOnly(2026, 5, 31)));

        Assert.Equal("operating_period.invalid_range", exception.Code);
    }

    [Fact]
    public void Activate_FromDraftOrSuspended_TransitionsToActiveAndEmitsEvent()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Inclusão", "Jovens aprendizes", "unit-1", period);

        program.Activate();
        Assert.Equal(ProgramStatus.Active, program.Status);
        Assert.True(program.CanAcceptEnrollments());
        Assert.Contains(program.DomainEvents, e => e is SocialProgramActivatedEvent);

        program.Suspend("Recesso anual de reformas");
        Assert.Equal(ProgramStatus.Suspended, program.Status);
        Assert.False(program.CanAcceptEnrollments());

        program.Activate();
        Assert.Equal(ProgramStatus.Active, program.Status);
        Assert.Null(program.SuspensionReason);
    }

    [Fact]
    public void Suspend_RejectsEmptyReasonAndInvalidStates()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Inclusão", "Jovens", "unit-1", period);

        // Cannot suspend from Draft
        var exDraft = Assert.Throws<DomainException>(() => program.Suspend("Motivo"));
        Assert.Equal("program.cannot_suspend", exDraft.Code);

        program.Activate();

        // Rejects blank reason
        var exReason = Assert.Throws<DomainException>(() => program.Suspend("   "));
        Assert.Equal("program.suspension_reason_required", exReason.Code);

        program.Suspend("Falta temporária de repasse");
        Assert.Equal("Falta temporária de repasse", program.SuspensionReason);

        // Cannot suspend already suspended
        var exAlready = Assert.Throws<DomainException>(() => program.Suspend("Outro motivo"));
        Assert.Equal("program.already_suspended", exAlready.Code);
    }

    [Fact]
    public void Complete_FromActiveOrSuspended_SetsCompletionDateAndRejectsInvalidDates()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 2, 1), new DateOnly(2026, 11, 30));
        var program = new SocialProgram("Programa Conclusão", "Famílias", "unit-1", period);
        program.Activate();

        var invalidDateEx = Assert.Throws<DomainException>(() =>
            program.Complete(new DateOnly(2026, 1, 15)));
        Assert.Equal("program.invalid_completion_date", invalidDateEx.Code);

        program.Complete(new DateOnly(2026, 11, 30));
        Assert.Equal(ProgramStatus.Completed, program.Status);
        Assert.Equal(new DateOnly(2026, 11, 30), program.CompletionDate);
        Assert.False(program.CanAcceptEnrollments());

        // Cannot reactivate completed program
        var exReactivate = Assert.Throws<DomainException>(() => program.Activate());
        Assert.Equal("program.already_completed", exReactivate.Code);
    }

    [Fact]
    public void Archive_OnlyAllowedFromDraftSuspendedOrCompleted()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Arquivamento", "Geral", "unit-1", period);
        program.Activate();

        var exActive = Assert.Throws<DomainException>(() => program.Archive());
        Assert.Equal("program.cannot_archive_active", exActive.Code);

        program.Suspend("Suspenso para encerramento");
        program.Archive();

        Assert.Equal(ProgramStatus.Archived, program.Status);
        Assert.False(program.CanAcceptEnrollments());

        var exArchivedAgain = Assert.Throws<DomainException>(() => program.Archive());
        Assert.Equal("program.already_archived", exArchivedAgain.Code);

        var exActivateArchived = Assert.Throws<DomainException>(() => program.Activate());
        Assert.Equal("program.already_archived", exActivateArchived.Code);
    }

    [Fact]
    public void InactiveOrArchivedProgram_RejectsModificationAndCriterionAddition()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Imutável", "Geral", "unit-1", period);
        program.Activate();
        program.Complete(new DateOnly(2026, 12, 31));

        var criterion = new AgeRangeEligibilityCriterion("FAIXA_ETARIA", "Faixa Etária", 7, 14);
        var exCriterion = Assert.Throws<DomainException>(() => program.AddEligibilityCriterion(criterion));
        Assert.Equal("program.not_modifiable", exCriterion.Code);

        var offering = new ServiceOffering("MUSICA", "Oficina de Violão");
        var exOffering = Assert.Throws<DomainException>(() => program.AddOffering(offering));
        Assert.Equal("program.not_modifiable", exOffering.Code);

        var exEnroll = Assert.Throws<DomainException>(() => program.EnsureCanAcceptEnrollments());
        Assert.Equal("program.not_active", exEnroll.Code);
    }

    [Fact]
    public void AddEligibilityCriterion_RejectsDuplicates()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Música", "Jovens", "unit-1", period);
        var criterion1 = new AgeRangeEligibilityCriterion("IDADE", "Idade Alvo", 6, 17);
        var criterion2 = new AgeRangeEligibilityCriterion("idade", "Idade Duplicada", 10, 15);

        program.AddEligibilityCriterion(criterion1);
        var ex = Assert.Throws<DomainException>(() => program.AddEligibilityCriterion(criterion2));
        Assert.Equal("program.duplicate_criterion", ex.Code);
    }

    [Fact]
    public void AddOffering_RejectsDuplicates()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Artes", "Jovens", "unit-1", period);
        var offering1 = new ServiceOffering("TEATRO", "Oficina de Teatro");
        var offering2 = new ServiceOffering("teatro", "Oficina de Teatro Avançado");

        program.AddOffering(offering1);
        var ex = Assert.Throws<DomainException>(() => program.AddOffering(offering2));
        Assert.Equal("program.duplicate_offering", ex.Code);
    }
}
