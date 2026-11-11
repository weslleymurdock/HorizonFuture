using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Events;
using HF.Domain.SocialServices.Evidences;
using HF.Domain.SocialServices.Referrals;
using Xunit;

namespace HF.UnitTests.Domain.SocialServices;

public sealed class SocialReferralAndEvidenceDomainTests
{
    [Fact]
    public void SocialReferral_External_DispatchesAndRecordsOutcomeFromDestination_WithFullAudit()
    {
        var referralDate = new DateOnly(2026, 3, 1);
        var referral = new SocialReferral(
            beneficiaryPersonId: "person-1",
            destinationType: ReferralDestinationType.External,
            destinationName: "CRAS Vila Esperança",
            reason: "Encaminhamento para inclusão no Cadastro Único (CadÚnico)",
            referralDate: referralDate,
            referredByPersonId: "assistente-social-1",
            sourceEncounterId: "encounter-001");

        Assert.Equal(ReferralStatus.Pending, referral.Status);
        Assert.Equal(ReferralDestinationType.External, referral.DestinationType);
        Assert.Equal("CRAS Vila Esperança", referral.DestinationName);
        Assert.Contains(referral.DomainEvents, e => e is SocialReferralCreatedEvent);

        // Dispatch with audit
        var dispatchDate = new DateOnly(2026, 3, 2);
        referral.Dispatch(dispatchDate, "assistente-social-1");
        Assert.Equal(ReferralStatus.Dispatched, referral.Status);
        Assert.Equal(dispatchDate, referral.DispatchedDate);
        Assert.Equal("assistente-social-1", referral.DispatchedByPersonId);

        // Acknowledge with audit
        var ackDate = new DateOnly(2026, 3, 5);
        referral.AcknowledgeReceipt(ackDate, "recepcao-cras");
        Assert.Equal(ReferralStatus.Acknowledged, referral.Status);
        Assert.Equal(ackDate, referral.AcknowledgedDate);
        Assert.Equal("recepcao-cras", referral.AcknowledgedBy);

        // Outcome reported by CRAS
        var outcomeDate = new DateOnly(2026, 3, 15);
        referral.RecordOutcome(
            outcomeDate: outcomeDate,
            outcomeNotes: "Família acolhida pelo CRAS; CadÚnico atualizado com sucesso e NIS emitido.",
            isSuccessful: true,
            recordedByPersonId: "assistente-social-1");

        Assert.Equal(ReferralStatus.Completed, referral.Status);
        Assert.Equal(outcomeDate, referral.OutcomeDate);
        Assert.True(referral.IsSuccessful);
        Assert.Equal("assistente-social-1", referral.OutcomeRecordedByPersonId);
        Assert.Contains(referral.DomainEvents, e => e is SocialReferralCompletedEvent);
    }

    [Fact]
    public void SocialReferral_Cancel_RequiresReasonAndStoresAudit()
    {
        var referral = new SocialReferral(
            "person-1", ReferralDestinationType.Internal, "Departamento Jurídico", "Orientação civil",
            new DateOnly(2026, 3, 1), "staff-1");

        var exNoReason = Assert.Throws<DomainException>(() =>
            referral.Cancel(new DateOnly(2026, 3, 2), "   ", "staff-1"));
        Assert.Equal("referral.cancellation_reason_required", exNoReason.Code);

        var exNoAuthor = Assert.Throws<ArgumentException>(() =>
            referral.Cancel(new DateOnly(2026, 3, 2), "Motivo", "   "));

        var cancelDate = new DateOnly(2026, 3, 3);
        referral.Cancel(cancelDate, "Demanda solucionada internamente pelo serviço social", "assistente-social-1");
        Assert.Equal(ReferralStatus.Cancelled, referral.Status);
        Assert.Equal("assistente-social-1", referral.CancelledByPersonId);
        Assert.Equal(cancelDate, referral.CancellationDate);
        Assert.Contains(referral.DomainEvents, e => e is SocialReferralCancelledEvent);

        var exOutcome = Assert.Throws<DomainException>(() =>
            referral.RecordOutcome(new DateOnly(2026, 3, 4), "Notas", true, "assistente-social-1"));
        Assert.Equal("referral.cannot_complete_cancelled", exOutcome.Code);
    }

    [Fact]
    public void SocialReferral_AuthorshipAndChronology_EnforcedAcrossAllTransitions_AndPreservesStateOnFailure()
    {
        var referralDate = new DateOnly(2026, 4, 10);
        var referral = new SocialReferral(
            "person-1", ReferralDestinationType.External, "UBS Central", "Avaliação nutricional",
            referralDate, "assistente-social-1");

        // 1. Dispatch validation
        Assert.Throws<ArgumentException>(() => referral.Dispatch(new DateOnly(2026, 4, 11), "   "));
        var exDispatchBefore = Assert.Throws<DomainException>(() =>
            referral.Dispatch(new DateOnly(2026, 4, 9), "staff-1"));
        Assert.Equal("referral.invalid_dispatch_date", exDispatchBefore.Code);

        // State preserved
        Assert.Equal(ReferralStatus.Pending, referral.Status);
        Assert.Null(referral.DispatchedDate);
        Assert.Null(referral.DispatchedByPersonId);

        // Valid dispatch
        referral.Dispatch(new DateOnly(2026, 4, 12), "staff-1");
        Assert.Equal(ReferralStatus.Dispatched, referral.Status);

        // 2. Acknowledgement validation
        Assert.Throws<ArgumentException>(() => referral.AcknowledgeReceipt(new DateOnly(2026, 4, 13), "   "));
        var exAckBeforeDispatch = Assert.Throws<DomainException>(() =>
            referral.AcknowledgeReceipt(new DateOnly(2026, 4, 11), "ubs-recepcao"));
        Assert.Equal("referral.invalid_acknowledgement_date", exAckBeforeDispatch.Code);

        // State preserved
        Assert.Equal(ReferralStatus.Dispatched, referral.Status);
        Assert.Null(referral.AcknowledgedDate);

        // Valid acknowledgement
        referral.AcknowledgeReceipt(new DateOnly(2026, 4, 14), "ubs-recepcao");
        Assert.Equal(ReferralStatus.Acknowledged, referral.Status);

        // 3. Outcome validation
        Assert.Throws<ArgumentException>(() =>
            referral.RecordOutcome(new DateOnly(2026, 4, 20), "Atendido", true, "   "));
        var exOutcomeNoNotes = Assert.Throws<DomainException>(() =>
            referral.RecordOutcome(new DateOnly(2026, 4, 20), "   ", true, "staff-1"));
        Assert.Equal("referral.outcome_notes_required", exOutcomeNoNotes.Code);
        var exOutcomeBeforeAck = Assert.Throws<DomainException>(() =>
            referral.RecordOutcome(new DateOnly(2026, 4, 13), "Atendido", true, "staff-1"));
        Assert.Equal("referral.invalid_outcome_date", exOutcomeBeforeAck.Code);

        // State preserved
        Assert.Equal(ReferralStatus.Acknowledged, referral.Status);
        Assert.Null(referral.OutcomeDate);

        // Valid outcome
        referral.RecordOutcome(new DateOnly(2026, 4, 20), "Paciente atendido com orientações", true, "staff-1");
        Assert.Equal(ReferralStatus.Completed, referral.Status);

        // 4. Transitions on Completed must be rejected
        var exDispatchCompleted = Assert.Throws<DomainException>(() =>
            referral.Dispatch(new DateOnly(2026, 4, 21), "staff-2"));
        Assert.Equal("referral.cannot_dispatch_completed", exDispatchCompleted.Code);

        var exAckCompleted = Assert.Throws<DomainException>(() =>
            referral.AcknowledgeReceipt(new DateOnly(2026, 4, 21), "staff-2"));
        Assert.Equal("referral.cannot_acknowledge_completed", exAckCompleted.Code);

        var exCancelCompleted = Assert.Throws<DomainException>(() =>
            referral.Cancel(new DateOnly(2026, 4, 21), "Motivo", "staff-2"));
        Assert.Equal("referral.cannot_cancel_completed", exCancelCompleted.Code);

        // 5. Constructor rejects invalid enum
        var exInvalidEnum = Assert.Throws<DomainException>(() =>
            new SocialReferral("person-1", (ReferralDestinationType)999, "Destino", "Motivo", referralDate, "staff-1"));
        Assert.Equal("referral.invalid_destination_type", exInvalidEnum.Code);
    }

    [Fact]
    public void SocialEvidence_StoresMetadataOnlyAndClassifiesSensitivity()
    {
        var evidence = new SocialEvidence(
            evidenceType: EvidenceType.SignedReceipt,
            storageReference: "s3://horizonfuture-evidences/receipts/2026/03/recibo-cesta-001.pdf",
            fileName: "recibo-cesta-001.pdf",
            contentType: "application/pdf",
            fileSizeBytes: 245_120,
            uploadedByPersonId: "staff-almoxarifado-1",
            description: "Termo de recebimento de cesta básica assinado pelo responsável legal",
            sensitivityLevel: EvidenceSensitivityLevel.Confidential,
            associatedEntityType: "DeliveredBenefit",
            associatedEntityId: "benefit-001");

        Assert.Equal(EvidenceType.SignedReceipt, evidence.EvidenceType);
        Assert.Equal("s3://horizonfuture-evidences/receipts/2026/03/recibo-cesta-001.pdf", evidence.StorageReference);
        Assert.Equal("recibo-cesta-001.pdf", evidence.FileName);
        Assert.Equal(245_120, evidence.FileSizeBytes);
        Assert.Equal(EvidenceSensitivityLevel.Confidential, evidence.SensitivityLevel);
        Assert.Equal("DeliveredBenefit", evidence.AssociatedEntityType);
        Assert.Equal("benefit-001", evidence.AssociatedEntityId);

        evidence.UpdateSensitivity(EvidenceSensitivityLevel.Restricted);
        Assert.Equal(EvidenceSensitivityLevel.Restricted, evidence.SensitivityLevel);
    }

    [Fact]
    public void SocialEvidence_RejectsNonPositiveSize()
    {
        var exZero = Assert.Throws<DomainException>(() =>
            new SocialEvidence(EvidenceType.Photo, "uri://ref", "foto.jpg", "image/jpeg", 0, "staff-1"));
        Assert.Equal("evidence.invalid_size", exZero.Code);

        var exNegative = Assert.Throws<DomainException>(() =>
            new SocialEvidence(EvidenceType.Photo, "uri://ref", "foto.jpg", "image/jpeg", -10, "staff-1"));
        Assert.Equal("evidence.invalid_size", exNegative.Code);

        // Invalid enums rejected
        var exInvalidType = Assert.Throws<DomainException>(() =>
            new SocialEvidence((EvidenceType)999, "uri://ref", "foto.jpg", "image/jpeg", 100, "staff-1"));
        Assert.Equal("evidence.invalid_type", exInvalidType.Code);

        var exInvalidSensitivity = Assert.Throws<DomainException>(() =>
            new SocialEvidence(EvidenceType.Photo, "uri://ref", "foto.jpg", "image/jpeg", 100, "staff-1", sensitivityLevel: (EvidenceSensitivityLevel)999));
        Assert.Equal("evidence.invalid_sensitivity_level", exInvalidSensitivity.Code);

        var validEvidence = new SocialEvidence(EvidenceType.Photo, "uri://ref", "foto.jpg", "image/jpeg", 100, "staff-1");
        var exUpdateInvalid = Assert.Throws<DomainException>(() => validEvidence.UpdateSensitivity((EvidenceSensitivityLevel)999));
        Assert.Equal("evidence.invalid_sensitivity_level", exUpdateInvalid.Code);
    }
}
