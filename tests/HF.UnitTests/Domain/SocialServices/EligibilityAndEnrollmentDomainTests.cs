using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Eligibility;
using HF.Domain.SocialServices.Enrollments;
using HF.Domain.SocialServices.Events;
using HF.Domain.SocialServices.Programs;
using HF.Domain.Stakeholders;
using Xunit;

namespace HF.UnitTests.Domain.SocialServices;

public sealed class EligibilityAndEnrollmentDomainTests
{
    [Fact]
    public void AgeRangeCriterion_CorrectlyEvaluatesCandidateAge()
    {
        var criterion = new AgeRangeEligibilityCriterion("AGE_CHECK", "Idade 6 a 12 anos", 6, 12);
        var refDate = new DateOnly(2026, 6, 1);

        // Exactly 10 years old
        var eligibleCandidate = new ApplicantProfile(dateOfBirth: new DateOnly(2016, 6, 1));
        var result1 = criterion.Evaluate(eligibleCandidate, refDate);
        Assert.True(result1.IsMet);

        // 13 years old (too old)
        var tooOldCandidate = new ApplicantProfile(dateOfBirth: new DateOnly(2013, 5, 1));
        var result2 = criterion.Evaluate(tooOldCandidate, refDate);
        Assert.False(result2.IsMet);

        // Missing birth date
        var missingBirthDateCandidate = new ApplicantProfile();
        var result3 = criterion.Evaluate(missingBirthDateCandidate, refDate);
        Assert.False(result3.IsMet);
    }

    [Fact]
    public void Program_EvaluatesAllCriteria_AndFlagsOverallEligibility()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Apoio Socioeducativo", "Crianças", "unit-1", period);

        program.AddEligibilityCriterion(new AgeRangeEligibilityCriterion("AGE", "Faixa Etária", 7, 14, isMandatory: true));
        program.AddEligibilityCriterion(new TerritoryEligibilityCriterion("TERRITORY", "Região Atendida", ["Vila Esperança", "Jardim Glória"], isMandatory: true));
        program.AddEligibilityCriterion(new DocumentRequirementCriterion("DOC_CADUNICO", "Comprovante CadÚnico", "CADUNICO", isMandatory: false));

        var refDate = new DateOnly(2026, 3, 1);

        // Candidate 1: Age 10, Vila Esperança, no CadUnico (still eligible because CadUnico is optional)
        var candidate1 = new ApplicantProfile(
            dateOfBirth: new DateOnly(2016, 1, 1),
            territory: "Vila Esperança");

        var assessment1 = program.EvaluateEligibility(candidate1, refDate, "assistente-social-1");
        Assert.True(assessment1.IsEligible);
        Assert.Equal(3, assessment1.EvaluatedCriteria.Count);
        Assert.True(assessment1.EvaluatedCriteria.First(c => c.CriterionCode == "AGE").IsMet);
        Assert.True(assessment1.EvaluatedCriteria.First(c => c.CriterionCode == "TERRITORY").IsMet);
        Assert.False(assessment1.EvaluatedCriteria.First(c => c.CriterionCode == "DOC_CADUNICO").IsMet);

        // Candidate 2: Age 16 (fails mandatory age)
        var candidate2 = new ApplicantProfile(
            dateOfBirth: new DateOnly(2010, 1, 1),
            territory: "Vila Esperança");

        var assessment2 = program.EvaluateEligibility(candidate2, refDate, "assistente-social-1");
        Assert.False(assessment2.IsEligible);
    }

    [Fact]
    public void Enrollment_CanOnlyBeSubmittedForActiveProgram()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Inclusivo", "Comunidade", "unit-1", period);
        var canonicalPerson = new Person("Lucas da Silva Sauro");

        // Fails while in Draft
        var exDraft = Assert.Throws<DomainException>(() =>
            new ProgramEnrollment(program, canonicalPerson.Id, new DateOnly(2026, 2, 1), "operador-1"));
        Assert.Equal("program.not_active", exDraft.Code);

        program.Activate();

        var enrollment = new ProgramEnrollment(program, canonicalPerson.Id, new DateOnly(2026, 2, 1), "operador-1");
        Assert.Equal(EnrollmentStatus.Pending, enrollment.Status);
        Assert.Equal(program.Id, enrollment.ProgramId);
        Assert.Equal(canonicalPerson.Id, enrollment.BeneficiaryPersonId);
        Assert.Contains(enrollment.DomainEvents, e => e is ProgramEnrollmentSubmittedEvent);
    }

    [Fact]
    public void Enrollment_Lifecycle_TransitionsFromPendingToApprovedAndCreatesParticipation()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Inclusivo", "Comunidade", "unit-1", period);
        program.Activate();

        var canonicalPerson = new Person("Mariana de Souza");
        var enrollment = new ProgramEnrollment(program, canonicalPerson.Id, new DateOnly(2026, 2, 1), "staff-1");

        enrollment.StartReview("assistente-social-1");
        Assert.Equal(EnrollmentStatus.UnderReview, enrollment.Status);

        enrollment.Approve(program, "coordenador-1", new DateOnly(2026, 2, 5));
        Assert.Equal(EnrollmentStatus.Approved, enrollment.Status);
        Assert.Equal("coordenador-1", enrollment.DecidedBy);
        Assert.Equal(new DateOnly(2026, 2, 5), enrollment.DecisionDate);
        Assert.Contains(enrollment.DomainEvents, e => e is ProgramEnrollmentApprovedEvent);

        var participation = enrollment.CreateParticipation(new DateOnly(2026, 2, 10), "coordenador-1");
        Assert.NotNull(participation);
        Assert.Equal(ParticipationStatus.Active, participation.Status);
        Assert.Equal(enrollment.Id, participation.EnrollmentId);
        Assert.Equal(canonicalPerson.Id, participation.BeneficiaryPersonId);
        Assert.Equal(program.Id, participation.ProgramId);
    }

    [Fact]
    public void Enrollment_CannotApproveIfProgramHasBecomeInactive()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Temporário", "Comunidade", "unit-1", period);
        program.Activate();

        var person = new Person("Julia Ferreira");
        var enrollment = new ProgramEnrollment(program, person.Id, new DateOnly(2026, 2, 1), "staff-1");

        program.Suspend("Interrupção técnica");

        var ex = Assert.Throws<DomainException>(() =>
            enrollment.Approve(program, "coordenador-1", new DateOnly(2026, 2, 10)));
        Assert.Equal("program.not_active", ex.Code);
    }

    [Fact]
    public void Enrollment_Reject_RequiresReasonAndBlocksDirectRejectionOfApproved()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Oficina", "Geral", "unit-1", period);
        program.Activate();

        var person = new Person("Carlos Alberto");
        var enrollment = new ProgramEnrollment(program, person.Id, new DateOnly(2026, 2, 1), "staff-1");

        // Blank reason
        var exNoReason = Assert.Throws<DomainException>(() =>
            enrollment.Reject("coordenador-1", new DateOnly(2026, 2, 3), "   "));
        Assert.Equal("enrollment.rejection_reason_required", exNoReason.Code);

        enrollment.Reject("coordenador-1", new DateOnly(2026, 2, 3), "Não atende à faixa etária");
        Assert.Equal(EnrollmentStatus.Rejected, enrollment.Status);
        Assert.Equal("Não atende à faixa etária", enrollment.RejectionReason);

        // Terminal state alteration rejected
        var exTerminal = Assert.Throws<DomainException>(() =>
            enrollment.Approve(program, "diretor-1", new DateOnly(2026, 2, 4)));
        Assert.Equal("enrollment.terminal_state", exTerminal.Code);
    }

    [Fact]
    public void Enrollment_Waitlist_And_Cancel_RequireReasonsAndSetProperStatuses()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Dança", "Geral", "unit-1", period);
        program.Activate();

        var person1 = new Person("Beatriz Santos");
        var enrollment1 = new ProgramEnrollment(program, person1.Id, new DateOnly(2026, 2, 1), "staff-1");

        enrollment1.Waitlist("coordenador-1", new DateOnly(2026, 2, 5), "Turma lotada; aguardando abertura de vaga");
        Assert.Equal(EnrollmentStatus.Waitlisted, enrollment1.Status);

        // Later approved from waitlist
        enrollment1.Approve(program, "coordenador-1", new DateOnly(2026, 2, 15));
        Assert.Equal(EnrollmentStatus.Approved, enrollment1.Status);

        var person2 = new Person("Gustavo Lima");
        var enrollment2 = new ProgramEnrollment(program, person2.Id, new DateOnly(2026, 2, 1), "staff-1");
        enrollment2.Cancel("coordenador-1", new DateOnly(2026, 2, 3), "Mudança de endereço fora da área de abrangência");
        Assert.Equal(EnrollmentStatus.Cancelled, enrollment2.Status);
    }

    [Fact]
    public void Enrollment_Withdrawal_AllowedForApplicantDesistance()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Futebol", "Jovens", "unit-1", period);
        program.Activate();

        var person = new Person("Thiago Ribeiro");
        var enrollment = new ProgramEnrollment(program, person.Id, new DateOnly(2026, 2, 1), "staff-1");
        enrollment.Approve(program, "coordenador-1", new DateOnly(2026, 2, 5));

        enrollment.Withdraw("responsavel-familiar", new DateOnly(2026, 2, 8), "Conflito com horário escolar");
        Assert.Equal(EnrollmentStatus.Withdrawn, enrollment.Status);
        Assert.Equal("Conflito com horário escolar", enrollment.WithdrawalReason);
    }
}
