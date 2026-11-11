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
    public void DeliveredBenefit_FullLifecycleWithDispatch_PreservesAllAuditTrail()
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
        Assert.Equal("assistente-social-1", benefit.GrantedByPersonId);
        Assert.Equal(grantDate, benefit.GrantedDate);
        Assert.Equal("proj-seguranca-alimentar-2026", benefit.ProjectId);
        Assert.Equal("edital-conselho-municipal", benefit.FundingSourceId);
        Assert.Contains(benefit.DomainEvents, e => e is BenefitGrantedEvent);

        // Dispatch
        var dispatchDate = new DateOnly(2026, 3, 3);
        benefit.Dispatch(dispatchDate, "motorista-logistica-1");
        Assert.Equal(BenefitDeliveryStatus.Dispatched, benefit.Status);
        Assert.Equal(dispatchDate, benefit.DispatchedDate);
        Assert.Equal("motorista-logistica-1", benefit.DispatchedByPersonId);

        // Cannot dispatch again
        var exAlreadyDispatched = Assert.Throws<DomainException>(() =>
            benefit.Dispatch(new DateOnly(2026, 3, 4), "outro-motorista"));
        Assert.Equal("benefit.already_dispatched", exAlreadyDispatched.Code);

        // Delivery date cannot be earlier than dispatch date
        var exDeliveryBeforeDispatch = Assert.Throws<DomainException>(() =>
            benefit.MarkAsDelivered(new DateOnly(2026, 3, 2), "staff-1", guardianMother.Id));
        Assert.Equal("benefit.invalid_delivery_date", exDeliveryBeforeDispatch.Code);

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
    public void DeliveredBenefit_Revoke_PreservesAuditAndOnlyAllowedBeforeDelivery()
    {
        var grantDate = new DateOnly(2026, 3, 1);
        var benefit = new DeliveredBenefit(
            "child-1", "prog-1", BenefitType.HygieneKit, "Kit higiene",
            new BenefitQuantity(1, "kit"), grantDate, "staff-1");

        // Cannot revoke with date earlier than grant
        var exRevokeDate = Assert.Throws<DomainException>(() =>
            benefit.Revoke("Critérios revisados", "assistente-social-1", new DateOnly(2026, 2, 28)));
        Assert.Equal("benefit.invalid_revocation_date", exRevokeDate.Code);

        var revokeDate = new DateOnly(2026, 3, 2);
        benefit.Revoke("Critérios revisados; família reenquadrada em outro benefício", "assistente-social-1", revokeDate);
        Assert.Equal(BenefitDeliveryStatus.Revoked, benefit.Status);
        Assert.Equal("Critérios revisados; família reenquadrada em outro benefício", benefit.RevocationReason);
        Assert.Equal("assistente-social-1", benefit.RevokedByPersonId);
        Assert.Equal(revokeDate, benefit.RevokedDate);
        Assert.Contains(benefit.DomainEvents, e => e is BenefitRevokedEvent);

        var exRevokeAgain = Assert.Throws<DomainException>(() =>
            benefit.Revoke("Outro motivo", "staff-1"));
        Assert.Equal("benefit.already_inactive", exRevokeAgain.Code);
    }

    [Fact]
    public void DeliveredBenefit_Return_PreservesAuditAndOnlyAllowedForDeliveredBenefits()
    {
        var benefit = new DeliveredBenefit(
            "person-1", "prog-1", BenefitType.Clothing, "Uniforme escolar",
            new BenefitQuantity(2, "unidades"), new DateOnly(2026, 3, 1), "staff-1");

        var exReturnUndelivered = Assert.Throws<DomainException>(() =>
            benefit.Return(new DateOnly(2026, 3, 2), "Tamanho incorreto", "staff-1"));
        Assert.Equal("benefit.cannot_return_undelivered", exReturnUndelivered.Code);

        var deliveryDate = new DateOnly(2026, 3, 2);
        benefit.MarkAsDelivered(deliveryDate, "staff-1", "person-1");

        // Return date cannot be earlier than delivery date
        var exReturnDate = Assert.Throws<DomainException>(() =>
            benefit.Return(new DateOnly(2026, 3, 1), "Tamanho incorreto", "staff-1"));
        Assert.Equal("benefit.invalid_return_date", exReturnDate.Code);

        var returnDate = new DateOnly(2026, 3, 5);
        benefit.Return(returnDate, "Tamanho incompatível, devolvido para troca", "almoxarife-1");

        Assert.Equal(BenefitDeliveryStatus.Returned, benefit.Status);
        Assert.Equal("Tamanho incompatível, devolvido para troca", benefit.ReturnReason);
        Assert.Equal("almoxarife-1", benefit.ReturnedByPersonId);
        Assert.Equal(returnDate, benefit.ReturnedDate);
    }

    [Fact]
    public void DeliveredService_Constructor_RejectsDirectCompletedOrCancelledStatus()
    {
        var exCompleted = Assert.Throws<DomainException>(() =>
            new DeliveredService(
                "person-1", "prog-1", "ATENDIMENTO", "staff-1", new DateOnly(2026, 3, 1),
                initialStatus: ServiceDeliveryStatus.Completed));
        Assert.Equal("service_delivery.invalid_initial_status", exCompleted.Code);

        var exCancelled = Assert.Throws<DomainException>(() =>
            new DeliveredService(
                "person-1", "prog-1", "ATENDIMENTO", "staff-1", new DateOnly(2026, 3, 1),
                initialStatus: ServiceDeliveryStatus.Cancelled));
        Assert.Equal("service_delivery.invalid_initial_status", exCancelled.Code);
    }

    [Fact]
    public void DeliveredService_ScheduleAndMarkAsCompleted_PreservesAuditAndEmitsEventOnce()
    {
        var service = new DeliveredService(
            beneficiaryPersonId: "person-1",
            programId: "prog-1",
            serviceDefinitionId: "OFICINA_MUSICA",
            scheduledByPersonId: "coordenador-1",
            scheduledDate: new DateOnly(2026, 3, 10),
            description: "Oficina individual de nivelamento de flauta",
            initialStatus: ServiceDeliveryStatus.Scheduled);

        Assert.Equal(ServiceDeliveryStatus.Scheduled, service.Status);
        Assert.Equal("coordenador-1", service.ScheduledByPersonId);
        Assert.Empty(service.DomainEvents);

        // Completion date cannot precede scheduled date
        var exInvalidDate = Assert.Throws<DomainException>(() =>
            service.MarkAsCompleted(new DateOnly(2026, 3, 9), "educador-1"));
        Assert.Equal("service_delivery.invalid_completion_date", exInvalidDate.Code);

        service.MarkAsCompleted(new DateOnly(2026, 3, 10), "educador-1");
        Assert.Equal(ServiceDeliveryStatus.Completed, service.Status);
        Assert.Equal("educador-1", service.DeliveredByPersonId);
        Assert.Single(service.DomainEvents);
        Assert.IsType<ServiceDeliveredEvent>(service.DomainEvents.First());

        // Cannot complete again
        var exCompleteAgain = Assert.Throws<DomainException>(() =>
            service.MarkAsCompleted(new DateOnly(2026, 3, 11), "educador-1"));
        Assert.Equal("service_delivery.already_completed", exCompleteAgain.Code);

        // Cannot cancel completed
        var exCancel = Assert.Throws<DomainException>(() =>
            service.Cancel("Motivo", "staff-1"));
        Assert.Equal("service_delivery.cannot_cancel_completed", exCancel.Code);
    }

    [Fact]
    public void DeliveredService_RecordCompleted_FactoryMethodExplicitlyCreatesCompleted()
    {
        var service = DeliveredService.RecordCompleted(
            beneficiaryPersonId: "person-1",
            programId: "prog-1",
            serviceDefinitionId: "ACOLHIMENTO_IMEDIATO",
            deliveredByPersonId: "assistente-social-1",
            deliveryDate: new DateOnly(2026, 3, 1),
            description: "Atendimento imediato não agendado");

        Assert.Equal(ServiceDeliveryStatus.Completed, service.Status);
        Assert.Equal("assistente-social-1", service.DeliveredByPersonId);
        Assert.Single(service.DomainEvents);
        Assert.IsType<ServiceDeliveredEvent>(service.DomainEvents.First());
    }

    [Fact]
    public void DeliveredService_Cancel_PreservesAuditAndRejectsFurtherModification()
    {
        var service = new DeliveredService(
            "person-1", "prog-1", "OFICINA_ARTE", "staff-1", new DateOnly(2026, 3, 10));

        var cancelDate = new DateOnly(2026, 3, 8);
        service.Cancel("Beneficiário comunicou desistência do serviço", "coordenador-1", cancelDate);

        Assert.Equal(ServiceDeliveryStatus.Cancelled, service.Status);
        Assert.Equal("Beneficiário comunicou desistência do serviço", service.CancellationReason);
        Assert.Equal("coordenador-1", service.CancelledByPersonId);
        Assert.Equal(cancelDate, service.CancelledDate);

        var exEvidence = Assert.Throws<DomainException>(() =>
            service.AttachEvidence("ev-001"));
        Assert.Equal("service_delivery.cannot_modify_cancelled", exEvidence.Code);
    }

    [Fact]
    public void DeliveredService_TransitionsAndCronology_EnforcesInvariantsAndPreservesStateOnFailure()
    {
        var scheduledDate = new DateOnly(2026, 4, 15);
        var service = new DeliveredService("person-1", "prog-1", "OFICINA_ARTE", "staff-1", scheduledDate);

        // 1. Invalid enum rejected in constructor
        var exInvalidEnum = Assert.Throws<DomainException>(() =>
            new DeliveredService("person-1", "prog-1", "SERV-1", "staff-1", scheduledDate, initialStatus: (ServiceDeliveryStatus)999));
        Assert.Equal("service_delivery.invalid_status", exInvalidEnum.Code);

        // 2. Initial Completed/Cancelled rejected in constructor
        var exInitialCompleted = Assert.Throws<DomainException>(() =>
            new DeliveredService("person-1", "prog-1", "SERV-1", "staff-1", scheduledDate, initialStatus: ServiceDeliveryStatus.Completed));
        Assert.Equal("service_delivery.invalid_initial_status", exInitialCompleted.Code);

        var exInitialCancelled = Assert.Throws<DomainException>(() =>
            new DeliveredService("person-1", "prog-1", "SERV-1", "staff-1", scheduledDate, initialStatus: ServiceDeliveryStatus.Cancelled));
        Assert.Equal("service_delivery.invalid_initial_status", exInitialCancelled.Code);

        // 3. MarkAsCompleted invalid arguments and chronology
        Assert.Throws<ArgumentException>(() => service.MarkAsCompleted(scheduledDate, "   "));
        var exCompleteBeforeScheduled = Assert.Throws<DomainException>(() =>
            service.MarkAsCompleted(scheduledDate.AddDays(-1), "staff-1"));
        Assert.Equal("service_delivery.invalid_completion_date", exCompleteBeforeScheduled.Code);

        // State preserved
        Assert.Equal(ServiceDeliveryStatus.Scheduled, service.Status);
        Assert.Null(service.DeliveredByPersonId);

        // 4. Cancel invalid arguments
        Assert.Throws<ArgumentException>(() => service.Cancel("Motivo", "   "));
        var exCancelNoReason = Assert.Throws<DomainException>(() => service.Cancel("   ", "staff-1"));
        Assert.Equal("service_delivery.cancellation_reason_required", exCancelNoReason.Code);

        // State preserved
        Assert.Equal(ServiceDeliveryStatus.Scheduled, service.Status);
        Assert.Null(service.CancelledByPersonId);
        Assert.Null(service.CancellationReason);

        // 5. Valid cancel (even in advance of scheduled date) and terminal protection
        var cancelDate = scheduledDate.AddDays(-2);
        service.Cancel("Cancelado por motivo legítimo", "coordenador-1", cancelDate);
        Assert.Equal(ServiceDeliveryStatus.Cancelled, service.Status);
        Assert.Equal(cancelDate, service.CancelledDate);

        var exCompleteCancelled = Assert.Throws<DomainException>(() =>
            service.MarkAsCompleted(scheduledDate, "staff-1"));
        Assert.Equal("service_delivery.cannot_complete_cancelled", exCompleteCancelled.Code);

        var exCancelAgain = Assert.Throws<DomainException>(() =>
            service.Cancel(scheduledDate, "Outro cancelamento", "coordenador-1"));
        Assert.Equal("service_delivery.already_cancelled", exCancelAgain.Code);
    }

    [Fact]
    public void DeliveredBenefit_EnforcesEnumAndDispatchToRevocationChronology()
    {
        // 1. Invalid benefit type rejected
        var exInvalidType = Assert.Throws<DomainException>(() =>
            new DeliveredBenefit("child-1", "prog-1", (BenefitType)999, "Desc", new BenefitQuantity(1, "un"), new DateOnly(2026, 3, 1), "staff-1"));
        Assert.Equal("benefit.invalid_type", exInvalidType.Code);

        // 2. Revocation before dispatch date is rejected when dispatched
        var grantDate = new DateOnly(2026, 3, 1);
        var dispatchDate = new DateOnly(2026, 3, 5);
        var benefit = new DeliveredBenefit("child-1", "prog-1", BenefitType.SchoolSupplies, "Kit escolar", new BenefitQuantity(1, "kit"), grantDate, "staff-1");
        benefit.Dispatch(dispatchDate, "motorista-1");

        var exRevokeBeforeDispatch = Assert.Throws<DomainException>(() =>
            benefit.Revoke("Revogado por erro de cadastro", "staff-1", dispatchDate.AddDays(-1)));
        Assert.Equal("benefit.invalid_revocation_date", exRevokeBeforeDispatch.Code);

        // State preserved
        Assert.Equal(BenefitDeliveryStatus.Dispatched, benefit.Status);
        Assert.Null(benefit.RevokedDate);

        // Valid revocation
        benefit.Revoke("Revogado por duplicidade", "staff-1", dispatchDate.AddDays(1));
        Assert.Equal(BenefitDeliveryStatus.Revoked, benefit.Status);
        Assert.Equal(dispatchDate.AddDays(1), benefit.RevokedDate);
    }
}
