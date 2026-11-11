# Requirements, MVP, and prioritized backlog

## Method

Priority uses MoSCoW: **M**ust, **S**hould, **C**ould, and **D**eferred.
Requirements are observable; “to implement” does not mean “implemented.”
Dependencies refer to business capabilities and roadmap stages. Validation is
**internal**, **external pending**, or **accepted** only when evidence is recorded.

## Functional requirements

| ID | Requirement and rationale | Actors | Pri. | Dependencies | Acceptance criterion | Validation |
|---|---|---|---|---|---|---|
| FR-01 | Register organization, units, responsibilities, and validity for governance | leadership, admin | M | — | authorized change preserves history and scope | internal |
| FR-02 | Reuse identities and time-bound roles to prevent duplication | admin, team | M | FR-01 | merge requires review, reason, and trail | external pending |
| FR-03 | Register programs, eligibility, participation, attendance, delivery, and exit | social/education team | M | FR-02 | enrollment-to-exit flow is auditable | external pending |
| FR-04 | Register projects, plan, activities, milestones, indicators, and evidence | coordinator | M | FR-03 | activity is not presented as impact without method | external pending |
| FR-05 | Track opportunity, application, decision, instrument, and obligation | fundraising, leadership | M | FR-04 | partial approval and amendment preserve versions | external pending |
| FR-06 | Distinguish provider, source, disbursement, and receipt | fundraising, finance | M | FR-05 | approved value does not appear as received cash | finance pending |
| FR-07 | Version budget and control source, project, budget item, and period | finance, coordinator | M | FR-05/06 | excess allocation is blocked or excepted | finance pending |
| FR-08 | Distinguish planned, approved, allocated, committed, and paid | finance | M | FR-07 | report shows balances with traceable formula | finance pending |
| FR-09 | Record revenue, expense, accounts, installments, splits, transfers, and reversals | finance | M | FR-06/07 | split totals movement and transfer does not duplicate result | finance pending |
| FR-10 | Require authorization, maker-checker, and financial change trail | leadership, finance, auditor | M | FR-09 | requester/executor cannot approve when prohibited | external pending |
| FR-11 | Import statement and close idempotent reconciliation | reconciler | M | FR-09 | line is not reconciled twice without correction | finance pending |
| FR-12 | Produce versioned and reproducible accountability | preparer, funder | M | FR-04/09/11 | snapshot points to all totals and documents | accountant pending |
| FR-13 | Restrict data and evidence by purpose, role, and scope | all | M | FR-01/02 | improper access is denied and audited | privacy pending |
| FR-14 | Allow offline operational records with explicit states | field team | M | FR-03/04 | retry does not duplicate and conflict is visible | technical pending |
| FR-15 | Export management data and accountability package without claiming statutory accounting | leadership, accountant | S | FR-12 | export identifies version and origin | accountant pending |
| FR-16 | Configure indicators, budget items, requirements, and calendars by instrument | coordinator, funder | S | FR-05/07 | rules for two instruments do not mix | external pending |
| FR-17 | Record recurring and in-kind donations and campaigns | fundraising | S | FR-06/09 | promise, assessment, and receipt are distinct | external pending |
| FR-18 | Integrate banks, gateways, and external accountant | finance | D | FR-09/11/15 | only after provider and contract validation | not started |

## Non-functional requirements

| ID | Requirement | Priority | Observable criterion |
|---|---|---|---|
| NFR-01 | Server security and authorization | M | every read/write/sync revalidates identity and scope |
| NFR-02 | Privacy and minimization | M | fields have purpose, classification, retention, and defined access |
| NFR-03 | Auditability | M | financial states and accountability have actor, time, reason, and version |
| NFR-04 | Financial integrity | M | safe decimal, balanced splits, and non-destructive postings |
| NFR-05 | Offline resilience | M | queue, retry, cursor, and conflict have visible idempotent results |
| NFR-06 | Availability and recovery | S | backups/restores and provider limits are exercised before production |
| NFR-07 | Performance | S | operation targets measured with real anonymized volumes |
| NFR-08 | Accessibility | S | critical flows navigable and understandable by keyboard/screen reader |
| NFR-09 | Maintainability | M | domain independent of EF, Npgsql, HTTP, MAUI, and UI |
| NFR-10 | Compatibility | S | API/local/sync versions explicitly handle old offline clients |

## MVP boundary

The MVP should cover essential organization/actors, non-clinical programs and
social records, projects with basic activities/indicators, opportunity and
instrument, multiple sources, versioned budget, commitments and operational
movements, authorization, evidence, basic reconciliation, and reproducible
accountability. The order depends on acceptance of stages 01–10 and then 11–13.

Candidates for later, subject to decision, include complete double-entry
accounting, payroll, taxes, gateway and direct debit, bank integrations,
advanced fundraising automation, complex electronic signatures, broad clinical
records/social assessments, multi-currency, and cross-installation
aggregation. “Later” means out of MVP scope, not that it will never be built.
