using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Events;
using HF.Domain.SocialServices.Evidences;
using HF.Domain.SocialServices.Referrals;
using Xunit;

namespace HF.UnitTests.Domain.SocialServices;

public sealed class SocialReferralAndEvidenceDomainTests
{
    [Fact]
    public void SocialReferral_External_DispatchesAndRecordsOutcomeFromDestination()
    {
        var referral = new SocialReferral(
            beneficiaryPersonId: "person-1",
            destinationType: ReferralDestinationType.External,
            destinationName: "CRAS Vila Esperança",
            reason: "Encaminhamento para inclusão no Cadastro Único (CadÚnico)",
            referralDate: new DateOnly(2026, 3, 1),
            referredByPersonId: "assistente-social-1",
            sourceEncounterId: "encounter-001");

        Assert.Equal(ReferralStatus.Pending, referral.Status);
        Assert.Equal(ReferralDestinationType.External, referral.DestinationType);
        Assert.Equal("CRAS Vila Esperança", referral.DestinationName);
        Assert.Contains(referral.DomainEvents, e => e is SocialReferralCreatedEvent);

        referral.Dispatch();
        Assert.Equal(ReferralStatus.Dispatched, referral.Status);

        referral.AcknowledgeReceipt();
        Assert.Equal(ReferralStatus.Acknowledged, referral.Status);

        // Outcome reported by CRAS
        referral.RecordOutcome(
            outcomeDate: new DateOnly(2026, 3, 15),
            outcomeNotes: "Família acolhida pelo CRAS; CadÚnico atualizado com sucesso e NIS emitido.",
            isSuccessful: true);

        Assert.Equal(ReferralStatus.Completed, referral.Status);
        Assert.Equal(new DateOnly(2026, 3, 15), referral.OutcomeDate);
        Assert.True(referral.IsSuccessful);
        Assert.Contains(referral.DomainEvents, e => e is SocialReferralCompletedEvent);
    }

    [Fact]
    public void SocialReferral_Cancel_RequiresReasonAndBlocksCompleted()
    {
        var referral = new SocialReferral(
            "person-1", ReferralDestinationType.Internal, "Departamento Jurídico", "Orientação civil",
            new DateOnly(2026, 3, 1), "staff-1");

        var exNoReason = Assert.Throws<DomainException>(() => referral.Cancel("   "));
        Assert.Equal("referral.cancellation_reason_required", exNoReason.Code);

        referral.Cancel("Demanda solucionada internamente pelo serviço social");
        Assert.Equal(ReferralStatus.Cancelled, referral.Status);
        Assert.Contains(referral.DomainEvents, e => e is SocialReferralCancelledEvent);

        var exOutcome = Assert.Throws<DomainException>(() =>
            referral.RecordOutcome(new DateOnly(2026, 3, 2), "Notas", true));
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
