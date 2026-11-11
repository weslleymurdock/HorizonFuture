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

        var exNoReason = Assert.Throws<DomainException>(() => referral.Cancel("   "));
        Assert.Equal("referral.cancellation_reason_required", exNoReason.Code);

        var cancelDate = new DateOnly(2026, 3, 3);
        referral.Cancel("Demanda solucionada internamente pelo serviço social", "assistente-social-1", cancelDate);
        Assert.Equal(ReferralStatus.Cancelled, referral.Status);
        Assert.Equal("assistente-social-1", referral.CancelledByPersonId);
        Assert.Equal(cancelDate, referral.CancellationDate);
        Assert.Contains(referral.DomainEvents, e => e is SocialReferralCancelledEvent);

        var exOutcome = Assert.Throws<DomainException>(() =>
            referral.RecordOutcome(new DateOnly(2026, 3, 4), "Notas", true));
        Assert.Equal("referral.cannot_complete_cancelled", exOutcome.Code);
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
    }
}
