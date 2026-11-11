# 03 — Social programs, beneficiaries, and service delivery

## Objective
Represent the social operation delivered by an NGO: who is served, through which programs, services, activities, and attendances, with a traceable history.

## Dependencies
Stages 01–02.

## Implemented baseline

The repository contains the framework-independent Stage 03 domain baseline under `HF.Domain.SocialServices`. It includes:
- **Programs & Offerings (`HF.Domain.SocialServices.Programs`):** `SocialProgram`, `OperatingPeriod`, `ServiceOffering`, `ProgramStatus` with strict lifecycle transitions (`Draft`, `Active`, `Suspended`, `Completed`, `Archived`) and acceptance guards for enrollments.
- **Eligibility (`HF.Domain.SocialServices.Eligibility`):** Typed and extensible eligibility criteria (`AgeRangeEligibilityCriterion`, `TargetGroupEligibilityCriterion`, `TerritoryEligibilityCriterion`, `DocumentRequirementCriterion`, `CustomEligibilityCriterion`), `ApplicantProfile`, and `EligibilityAssessmentResult`.
- **Enrollments & Participation (`HF.Domain.SocialServices.Enrollments`):** Canonical reuse of `HF.Domain.Stakeholders.Person`. `ProgramEnrollment` with review, waitlist, rejection, cancellation, and withdrawal lifecycles; independent `ProgramParticipation` with an immutable audit history trail (`ParticipationHistoryEntry`) and strict chronological validation.
- **Social Care Encounters (`HF.Domain.SocialServices.Encounters`):** `SocialCareEncounter`, `EncounterType`, `EncounterStatus`, and encapsulated `SensitiveCareNotes` with mandatory legitimate purpose and confidentiality classification (no clinical or medical diagnostic scope).
- **Service & Benefit Delivery (`HF.Domain.SocialServices.Deliveries`):** `DeliveredService` with completion/cancellation lifecycles; `DeliveredBenefit` with strictly positive `BenefitQuantity`, delivery recipient tracking (beneficiary or legal guardian via `PersonRelationship`), and hooks for future accounting/funder links (`ProjectId`, `FundingSourceId`).
- **Referrals (`HF.Domain.SocialServices.Referrals`):** `SocialReferral` for internal and external networks (CRAS, CREAS, health clinics, public defender), preserving the distinction between a referral and a service executed by the NGO.
- **Activities & Sessions (`HF.Domain.SocialServices.Activities`):** `SocialActivity`, `ActivitySession` with facilitator assignments, schedule guards, and attendance recording (`Present`, `Absent`, `JustifiedAbsence` requiring mandatory justification, `Excused`, `NotRecorded`), with guards against attendance on cancelled sessions or duplicate recording.
- **Evidences (`HF.Domain.SocialServices.Evidences`):** `SocialEvidence` storing metadata, file size, content type, logical references, and sensitivity levels without binary storage in relational rows.
- **Domain Events (`HF.Domain.SocialServices.Events`):** Typed domain events inheriting from `DomainEvent` for all major lifecycle state changes.

Detailed guidelines, lifecycle diagrams, and privacy rules are documented in `docs/guidelines/social-service-delivery.md`.

## Stabilization status

Backend domain stabilization and acceptance testing are complete. All tests run locally on Windows using the .NET SDK.

Key domain stabilizations applied:
- **`ProgramParticipation` instantiation:** Restricted to internal scope and coupled with `ProgramEnrollment`. Direct unvetted constructor access is prevented.
- **Participation duplicate prevention:** Enforced via `ProgramEnrollment.HasParticipation` and `ParticipationId`. Repeated creation attempts fail immediately.
- **Atomic state transitions:** `ProgramParticipation` (`Suspend`, `Resume`, `Complete`, `Terminate`) validates all arguments and constructs history entries prior to mutating state or updating timestamps.
- **Eligibility consistency & approval guards:** `EligibilityAssessmentResult` enforces coherence between `IsEligible` and `mandatoryCriteriaMet`. Ineligible enrollments or enrollments missing mandatory criteria assessment cannot be approved.
- **Benefit auditability:** Full dispatch, revocation, and return lifecycle tracking with timestamps and actors (`DispatchedDate`, `DispatchedByPersonId`, `RevokedDate`, `RevokedByPersonId`, `ReturnedDate`, `ReturnedByPersonId`).
- **Service lifecycle integrity:** Initial `Completed` or `Cancelled` states rejected in default construction; explicit `RecordCompleted` factory provided; audit actors captured.
- **Session attendance sealing:** Attendance recording/modification blocked on completed or cancelled sessions; invalid or unrecorded statuses rejected; state preserved on failures.

Required local validation commands:

```powershell
dotnet restore HorizonFuture.slnx
dotnet build HorizonFuture.slnx --no-restore
dotnet test tests\HF.UnitTests\HF.UnitTests.csproj
```

Execution result on Windows host with .NET SDK `10.0.403`:
- `HF.Domain -> HF.Domain.dll`: 0 warnings, 0 errors.
- `HF.UnitTests`: 172 passed, 0 failed, 0 skipped (including 51 domain tests covering all social service delivery scenarios, error flows, and state preservation).

## Work
- [x] Model program/service lines, target audiences, eligibility criteria, locations, and operating periods.
- [x] Model beneficiary and family composition, guardianship/relationships, enrollment/participation, referrals, attendance, appointments, and service/benefit delivery.
- [x] Model activity plans and sessions, facilitators, participant lists, attendance outcomes, notes, and evidence.
- [x] Define typed eligibility criteria with explicit evaluation and decision separation.
- [x] Define case/history records within an approved privacy scope and encapsulate sensitive notes.
- [x] Define documents and evidence metadata while keeping binary media outside relational storage.
- [x] Establish links from delivered services and beneficiaries to programs and funded projects without duplicating records.
- [x] Stabilize aggregate roots, transition atomicity, duplicate prevention, and lifecycle sealing against invalid states.

## Acceptance criteria
- [x] Lifecycle diagrams cover enrollment through participation, attendance, referral, and benefit delivery.
- [x] Authorization and privacy guidelines protect beneficiary records and sensitive notes.
- [x] Required operational rules and invariants are enforced in domain aggregates.
- [x] Domain invariants and representative synthetic scenarios are covered by automated unit tests.
- [x] Aggregate integrity, state transition atomicity, and error path preservation are verified by unit tests.

## Risks and open decisions
1. **Clinical/Medical Scope:** The domain explicitly refuses to become an electronic health record. No ICD/medical diagnoses or prescriptions are accepted in `SocialCareEncounter`.
2. **Post-Release Piloting:** Operational assumptions (such as waitlist auto-promotion policies and guardian pickup delegation rules) will be validated with the pilot NGO partner following the first stable release.
