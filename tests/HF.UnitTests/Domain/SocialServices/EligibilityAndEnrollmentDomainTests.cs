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
    public void AssessmentResult_ContradictoryEligibility_ThrowsDomainException()
    {
        var mandatoryFailing = new CriterionEvaluationResult("AGE", "Idade", false, "Idade inadequada", isMandatory: true);
        var optionalFailing = new CriterionEvaluationResult("DOC", "Doc", false, "Doc ausente", isMandatory: false);
        var mandatoryPassing = new CriterionEvaluationResult("TERRITORY", "Região", true, "Região atendida", isMandatory: true);

        // Contradiction 1: IsEligible = true when mandatory criterion failed
        var exContradictory1 = Assert.Throws<DomainException>(() =>
            new EligibilityAssessmentResult(
                isEligible: true,
                evaluatedCriteria: [mandatoryFailing, mandatoryPassing],
                assessedOn: new DateOnly(2026, 3, 1),
                assessedBy: "staff-1"));
        Assert.Equal("eligibility.contradictory_result", exContradictory1.Code);

        // Contradiction 2: IsEligible = false when all mandatory criteria passed and only optional failed
        var exContradictory2 = Assert.Throws<DomainException>(() =>
            new EligibilityAssessmentResult(
                isEligible: false,
                evaluatedCriteria: [mandatoryPassing, optionalFailing],
                assessedOn: new DateOnly(2026, 3, 1),
                assessedBy: "staff-1"));
        Assert.Equal("eligibility.contradictory_result", exContradictory2.Code);

        // Factory Create() computes correctly without contradiction
        var createdPositive = EligibilityAssessmentResult.Create(
            [mandatoryPassing, optionalFailing],
            new DateOnly(2026, 3, 1),
            "staff-1");
        Assert.True(createdPositive.IsEligible);

        var createdNegative = EligibilityAssessmentResult.Create(
            [mandatoryFailing, mandatoryPassing],
            new DateOnly(2026, 3, 1),
            "staff-1");
        Assert.False(createdNegative.IsEligible);
    }

    [Fact]
    public void Enrollment_Approve_WithIneligibleAssessment_ThrowsAndPreservesState()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Inclusivo", "Comunidade", "unit-1", period);
        program.Activate();

        var person = new Person("Marcos Silva");
        var enrollment = new ProgramEnrollment(program, person.Id, new DateOnly(2026, 2, 1), "staff-1");
        enrollment.StartReview("assistente-social-1");

        var failingResult = new CriterionEvaluationResult("AGE", "Idade", false, "Fora da faixa", isMandatory: true);
        var negativeAssessment = EligibilityAssessmentResult.Create([failingResult], new DateOnly(2026, 2, 2), "assistente-social-1");

        var exIneligible = Assert.Throws<DomainException>(() =>
            enrollment.Approve(program, "coordenador-1", new DateOnly(2026, 2, 5), negativeAssessment));
        Assert.Equal("enrollment.ineligible", exIneligible.Code);

        // Verify state is preserved
        Assert.Equal(EnrollmentStatus.UnderReview, enrollment.Status);
        Assert.Null(enrollment.DecisionDate);
        Assert.Null(enrollment.DecidedBy);
        Assert.DoesNotContain(enrollment.DomainEvents, e => e is ProgramEnrollmentApprovedEvent);
    }

    [Fact]
    public void Enrollment_Approve_WithoutAssessment_RejectsIfProgramHasMandatoryCriteria()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Rigoroso", "Comunidade", "unit-1", period);
        program.AddEligibilityCriterion(new AgeRangeEligibilityCriterion("AGE", "Idade", 6, 12, isMandatory: true));
        program.Activate();

        var person = new Person("Ana Clara");
        var enrollment = new ProgramEnrollment(program, person.Id, new DateOnly(2026, 2, 1), "staff-1");

        var exRequired = Assert.Throws<DomainException>(() =>
            enrollment.Approve(program, "coordenador-1", new DateOnly(2026, 2, 5), assessment: null));
        Assert.Equal("enrollment.assessment_required", exRequired.Code);
        Assert.Equal(EnrollmentStatus.Pending, enrollment.Status);
    }

    [Fact]
    public void Enrollment_AttachAssessment_RejectsInIncompatibleStates()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Aprovado", "Comunidade", "unit-1", period);
        program.Activate();

        var person = new Person("Joao Victor");
        var enrollment = new ProgramEnrollment(program, person.Id, new DateOnly(2026, 2, 1), "staff-1");

        var passingResult = new CriterionEvaluationResult("AGE", "Idade", true, "Ok", isMandatory: true);
        var assessment = EligibilityAssessmentResult.Create([passingResult], new DateOnly(2026, 2, 2), "staff-1");

        enrollment.Approve(program, "coordenador-1", new DateOnly(2026, 2, 5), assessment);
        Assert.Equal(EnrollmentStatus.Approved, enrollment.Status);

        // Cannot attach new assessment when already approved
        var exNotAllowed = Assert.Throws<DomainException>(() =>
            enrollment.AttachAssessment(assessment));
        Assert.Equal("enrollment.assessment_not_allowed", exNotAllowed.Code);
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

    [Fact]
    public void Enrollment_StateMachineTransitions_Chronology_AndStatePreservationOnFailure()
    {
        var period = new OperatingPeriod(new DateOnly(2026, 1, 1));
        var program = new SocialProgram("Programa Inclusão", "Crianças", "unit-1", period);
        program.Activate();

        var enrollmentDate = new DateOnly(2026, 3, 10);
        var enrollment = new ProgramEnrollment(program, "person-1", enrollmentDate, "staff-1");

        // 1. StartReview from Pending -> UnderReview
        enrollment.StartReview("revisor-1");
        Assert.Equal(EnrollmentStatus.UnderReview, enrollment.Status);

        // Cannot StartReview again from UnderReview
        var exReviewAgain = Assert.Throws<DomainException>(() => enrollment.StartReview("revisor-2"));
        Assert.Equal("enrollment.invalid_transition", exReviewAgain.Code);
        Assert.Equal(EnrollmentStatus.UnderReview, enrollment.Status);

        // 2. Decision date earlier than enrollment date rejected across all decision methods
        var exDecisionBefore = Assert.Throws<DomainException>(() =>
            enrollment.Approve(program, "coordenador-1", enrollmentDate.AddDays(-1)));
        Assert.Equal("enrollment.invalid_decision_date", exDecisionBefore.Code);
        Assert.Equal(EnrollmentStatus.UnderReview, enrollment.Status);
        Assert.Null(enrollment.DecisionDate);

        var exWaitlistBefore = Assert.Throws<DomainException>(() =>
            enrollment.Waitlist("coordenador-1", enrollmentDate.AddDays(-2), "Motivo"));
        Assert.Equal("enrollment.invalid_decision_date", exWaitlistBefore.Code);
        Assert.Equal(EnrollmentStatus.UnderReview, enrollment.Status);

        var exRejectBefore = Assert.Throws<DomainException>(() =>
            enrollment.Reject("coordenador-1", enrollmentDate.AddDays(-3), "Motivo"));
        Assert.Equal("enrollment.invalid_decision_date", exRejectBefore.Code);
        Assert.Equal(EnrollmentStatus.UnderReview, enrollment.Status);

        var exCancelBefore = Assert.Throws<DomainException>(() =>
            enrollment.Cancel("coordenador-1", enrollmentDate.AddDays(-4), "Motivo"));
        Assert.Equal("enrollment.invalid_cancellation_date", exCancelBefore.Code);
        Assert.Equal(EnrollmentStatus.UnderReview, enrollment.Status);

        var exWithdrawBefore = Assert.Throws<DomainException>(() =>
            enrollment.Withdraw("responsavel", enrollmentDate.AddDays(-5), "Motivo"));
        Assert.Equal("enrollment.invalid_withdrawal_date", exWithdrawBefore.Code);
        Assert.Equal(EnrollmentStatus.UnderReview, enrollment.Status);

        // 3. Attach assessment while in UnderReview
        var assessment = EligibilityAssessmentResult.Create([], enrollmentDate, "avaliador-1");
        enrollment.AttachAssessment(assessment);
        Assert.Same(assessment, enrollment.AssessmentResult);

        // 4. Approve successfully
        enrollment.Approve(program, "diretor-1", enrollmentDate.AddDays(2));
        Assert.Equal(EnrollmentStatus.Approved, enrollment.Status);
        Assert.Equal("diretor-1", enrollment.DecidedBy);
        Assert.Equal(enrollmentDate.AddDays(2), enrollment.DecisionDate);

        // 5. Cannot attach new assessment to an approved enrollment
        var newAssessment = EligibilityAssessmentResult.Create([], enrollmentDate.AddDays(3), "avaliador-2");
        var exAttachApproved = Assert.Throws<DomainException>(() => enrollment.AttachAssessment(newAssessment));
        Assert.Equal("enrollment.assessment_not_allowed", exAttachApproved.Code);
        Assert.Same(assessment, enrollment.AssessmentResult); // Preserved

        // 6. Cannot waitlist or reject an approved enrollment
        var exWaitlistApproved = Assert.Throws<DomainException>(() =>
            enrollment.Waitlist("coordenador-1", enrollmentDate.AddDays(3), "Tentar lista"));
        Assert.Equal("enrollment.invalid_transition", exWaitlistApproved.Code);

        var exRejectApproved = Assert.Throws<DomainException>(() =>
            enrollment.Reject("coordenador-1", enrollmentDate.AddDays(3), "Tentar rejeitar"));
        Assert.Equal("enrollment.cannot_reject_approved", exRejectApproved.Code);

        // 7. Active participation protects enrollment from administrative cancellation and withdrawal
        enrollment.CreateParticipation(enrollmentDate.AddDays(5), "staff-1");
        Assert.True(enrollment.HasParticipation);

        var exCancelWithParticipation = Assert.Throws<DomainException>(() =>
            enrollment.Cancel("coordenador-1", enrollmentDate.AddDays(6), "Tentativa cancelamento"));
        Assert.Equal("enrollment.cannot_cancel_with_active_participation", exCancelWithParticipation.Code);
        Assert.Equal(EnrollmentStatus.Approved, enrollment.Status);

        var exWithdrawWithParticipation = Assert.Throws<DomainException>(() =>
            enrollment.Withdraw("familiar-1", enrollmentDate.AddDays(6), "Tentativa desistencia"));
        Assert.Equal("enrollment.cannot_withdraw_with_active_participation", exWithdrawWithParticipation.Code);
        Assert.Equal(EnrollmentStatus.Approved, enrollment.Status);
    }
}
