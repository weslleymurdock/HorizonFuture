using HF.Domain.Common.Exceptions;
using HF.Domain.SocialServices.Deliveries;
using HF.Domain.SocialServices.Events;
using HF.Domain.Stakeholders;
using Xunit;

namespace HF.UnitTests.Domain.SocialServices;

public sealed class DeliveredServiceAndBenefitDomainTests
{
    [Fact]
    public void BenefitQuantity_RequiresPositiveAmountAndUnit()
    {
        var exZero = Assert.Throws<DomainException>(() => new BenefitQuantity(0, "cestas"));
        Assert.Equal("benefit.invalid_quantity", exZero.Code);

        var exNegative = Assert.Throws<DomainException>(() => new BenefitQuantity(-2.5m, "kg"));
        Assert.Equal("benefit.invalid_quantity", exNegative.Code);

        var exNoUnit = Assert.Throws<DomainException>(() => new BenefitQuantity(1, "   "));
        Assert.Equal("benefit.unit_required", exNoUnit.Code);

        var quantity = new BenefitQuantity(2, " cestas ");
        Assert.Equal(2, quantity.Amount);
        Assert.Equal("cestas", quantity.UnitOfMeasure);
    }

    [Fact]
    public void DeliveredBenefit_GrantAndMarkAsDelivered_SetsRecipientAndEmitsEvent()
    {
        var beneficiaryChild = new Person("Pedro Henrique", "child-1");
        var guardianMother = new Person("Maria Aparecida", "guardian-1");

        // Verify guardianship relationship from Stage 02
        var relationship = new PersonRelationship(
            beneficiaryChild.Id,
            guardianMother.Id,
            PersonRelationshipType.LegalGuardian,
            new DateOnly(2026, 1, 1));
        Assert.Equal(PersonRelationshipType.LegalGuardian, relationship.Type);

        var grantDate = new DateOnly(2026, 3, 1);
        var benefit = new DeliveredBenefit(
            beneficiaryPersonId: beneficiaryChild.Id,
            programId: "prog-nutricao",
            benefitType: BenefitType.FoodBasket,
            description: "Cesta de alimentos não-perecíveis 15kg",
            quantity: new BenefitQuantity(1, "cesta"),
            grantedDate: grantDate,
            grantedByPersonId: "assistente-social-1",
            projectId: "proj-seguranca-alimentar-2026",
            fundingSourceId: "edital-conselho-municipal");

        Assert.Equal(BenefitDeliveryStatus.Granted, benefit.Status);
        Assert.Equal("proj-seguranca-alimentar-2026", benefit.ProjectId);
        Assert.Equal("edital-conselho-municipal", benefit.FundingSourceId);
        Assert.Contains(benefit.DomainEvents, e => e is BenefitGrantedEvent);

        // Deliver to guardian
        var deliveryDate = new DateOnly(2026, 3, 5);
        benefit.MarkAsDelivered(
            deliveryDate: deliveryDate,
            deliveredByPersonId: "staff-almoxarifado-1",
            receivedByPersonId: guardianMother.Id,
            deliveryReceiptEvidenceId: "doc-recibo-assinado-001");

        Assert.Equal(BenefitDeliveryStatus.Delivered, benefit.Status);
        Assert.Equal(deliveryDate, benefit.DeliveredDate);
        Assert.Equal(guardianMother.Id, benefit.ReceivedByPersonId);
        Assert.Equal("staff-almoxarifado-1", benefit.DeliveredByPersonId);
        Assert.Equal("doc-recibo-assinado-001", benefit.DeliveryReceiptEvidenceId);

        var deliveryEvent = Assert.Single(benefit.DomainEvents.OfType<BenefitDeliveredEvent>());
        Assert.Equal(guardianMother.Id, deliveryEvent.ReceivedByPersonId);
    }

    [Fact]
    public void DeliveredBenefit_RejectsDeliveryDateEarlierThanGrant()
    {
        var benefit = new DeliveredBenefit(
            "child-1", "prog-1", BenefitType.SchoolSupplies, "Kit de material escolar",
            new BenefitQuantity(1, "kit"), new DateOnly(2026, 3, 10), "staff-1");

        var exDate = Assert.Throws<DomainException>(() =>
            benefit.MarkAsDelivered(new DateOnly(2026, 3, 9), "staff-1", "parent-1"));
        Assert.Equal("benefit.invalid_delivery_date", exDate.Code);
    }

    [Fact]
    public void DeliveredBenefit_Revoke_OnlyAllowedBeforeDelivery()
    {
        var benefit = new DeliveredBenefit(
            "child-1", "prog-1", BenefitType.HygieneKit, "Kit higiene",
            new BenefitQuantity(1, "kit"), new DateOnly(2026, 3, 1), "staff-1");

        benefit.Revoke("Critérios revisados; família reenquadrada em outro benefício", "assistente-social-1");
        Assert.Equal(BenefitDeliveryStatus.Revoked, benefit.Status);
        Assert.Equal("Critérios revisados; família reenquadrada em outro benefício", benefit.RevocationReason);
        Assert.Contains(benefit.DomainEvents, e => e is BenefitRevokedEvent);

        var exRevokeAgain = Assert.Throws<DomainException>(() =>
            benefit.Revoke("Outro motivo", "staff-1"));
        Assert.Equal("benefit.already_inactive", exRevokeAgain.Code);
    }

    [Fact]
    public void DeliveredBenefit_Return_OnlyAllowedForDeliveredBenefits()
    {
        var benefit = new DeliveredBenefit(
            "person-1", "prog-1", BenefitType.Clothing, "Uniforme escolar",
            new BenefitQuantity(2, "unidades"), new DateOnly(2026, 3, 1), "staff-1");

        var exReturnUndelivered = Assert.Throws<DomainException>(() =>
            benefit.Return(new DateOnly(2026, 3, 2), "Tamanho incorreto", "staff-1"));
        Assert.Equal("benefit.cannot_return_undelivered", exReturnUndelivered.Code);

        benefit.MarkAsDelivered(new DateOnly(2026, 3, 2), "staff-1", "person-1");
        benefit.Return(new DateOnly(2026, 3, 5), "Tamanho incompatível, trocado no mesmo dia", "staff-1");

        Assert.Equal(BenefitDeliveryStatus.Returned, benefit.Status);
        Assert.Equal("Tamanho incompatível, trocado no mesmo dia", benefit.ReturnReason);
    }

    [Fact]
    public void DeliveredService_RecordAndCancel_PreservesLifecycle()
    {
        var service = new DeliveredService(
            beneficiaryPersonId: "person-1",
            programId: "prog-1",
            serviceDefinitionId: "OFICINA_MUSICA",
            deliveredByPersonId: "educador-1",
            deliveryDate: new DateOnly(2026, 3, 10),
            description: "Oficina individual de nivelamento de flauta",
            status: ServiceDeliveryStatus.Scheduled);

        Assert.Equal(ServiceDeliveryStatus.Scheduled, service.Status);

        service.MarkAsCompleted(new DateOnly(2026, 3, 10), "educador-1");
        Assert.Equal(ServiceDeliveryStatus.Completed, service.Status);
        Assert.Contains(service.DomainEvents, e => e is ServiceDeliveredEvent);

        // Cannot cancel completed
        var exCancel = Assert.Throws<DomainException>(() =>
            service.Cancel("Motivo", "staff-1"));
        Assert.Equal("service_delivery.cannot_cancel_completed", exCancel.Code);
    }
}
