# HorizonFuture implementation roadmap

This roadmap has been reordered to make the NGO domain—not the client UI—the first-class product foundation. Stages 01–10 define and validate the complete backend domain before any MAUI client implementation. The roadmap is a plan; it is not evidence that a stage has been implemented or accepted.

## Product direction

HorizonFuture is an open-source management platform for Brazilian NGOs/OSCs. It must go beyond stakeholder CRUD and connect social-service delivery, projects, fundraising, restricted funding, budgets, financial operations, reconciliation, accountability, and impact evidence.

The financial scope is NGO-oriented: trace every resource from origin to project, approved budget/rubric, transaction, supporting evidence, and funder report. Management and accountability features do not automatically make the product a statutory accounting system or replacement for an accountant.

## Stage sequence

| Stage | File | Outcome |
|---|---|---|
| 01 | [01-foundations.md](01-foundations.md) | Product discovery, workflows, privacy, and scope |
| 02 | [02-organization-and-stakeholders.md](02-organization-and-stakeholders.md) | NGO structure, governance, people, roles, stakeholders |
| 03 | [03-social-service-delivery.md](03-social-service-delivery.md) | Programs, beneficiaries, families, services, activities, attendance |
| 04 | [04-projects-activities-and-impact.md](04-projects-activities-and-impact.md) | Project lifecycle, work plans, indicators, evidence, outcomes |
| 05 | [05-funding-and-resource-mobilization.md](05-funding-and-resource-mobilization.md) | Funders, opportunities/editais, applications, grants, agreements, disbursements |
| 06 | [06-financial-foundation-and-chart-of-accounts.md](06-financial-foundation-and-chart-of-accounts.md) | Financial accounts, categories, chart of accounts, rubrics, transaction lifecycle |
| 07 | [07-budgets-and-resource-allocation.md](07-budgets-and-resource-allocation.md) | Project budgets, multiple sources, restrictions, allocations, commitments |
| 08 | [08-financial-operations-and-cashflow.md](08-financial-operations-and-cashflow.md) | Income, expenses, donations, payables/receivables, transfers, cash flow |
| 09 | [09-reconciliation-accountability-and-financial-reporting.md](09-reconciliation-accountability-and-financial-reporting.md) | Bank reconciliation, accountability, audit, financial reports |
| 10 | [10-domain-integration-and-acceptance.md](10-domain-integration-and-acceptance.md) | Bounded contexts, invariants, domain review and acceptance |
| 11 | [11-api-identity-and-authorization.md](11-api-identity-and-authorization.md) | REST contracts, identity, roles, device trust, financial authorization |
| 12 | [12-postgresql-provider-validation.md](12-postgresql-provider-validation.md) | PostgreSQL-only server provider compatibility gate |
| 13 | [13-server-persistence-and-migrations.md](13-server-persistence-and-migrations.md) | PostgreSQL schema, migrations, financial transactions, audit/change feed |
| 14 | [14-maui-local-sqlite.md](14-maui-local-sqlite.md) | MAUI local SQLite and offline operational store |
| 15 | [15-bidirectional-sync-protocol.md](15-bidirectional-sync-protocol.md) | Transactional Outbox/Inbox, idempotent Push, ordered Pull |
| 16 | [16-concurrency-and-financial-conflicts.md](16-concurrency-and-financial-conflicts.md) | Optimistic concurrency and strict financial conflict rules |
| 17 | [17-maui-sync-experience-and-media.md](17-maui-sync-experience-and-media.md) | MAUI sync UX, scheduling, documents and evidence |
| 18 | [18-verification-and-observability.md](18-verification-and-observability.md) | Domain, finance, security, sync tests and observability |
| 19 | [19-deployment-rollout-and-recovery.md](19-deployment-rollout-and-recovery.md) | Per-NGO cloud deployment, rollout, backups and recovery |

## Critical sequencing rule

**Do not start Stage 14 (MAUI client implementation) until Stages 01–13 have been accepted.** In particular, the social/funding/financial domain model, its invariants, API authorization, PostgreSQL provider, and server persistence must be sufficiently stable before local projections and synchronization contracts are designed.

Stages 01–10 are deliberately backend/domain-first. Stages 11–13 establish the cloud backend. Only then do Stages 14–17 implement the offline client and synchronization.

## Storage decision

- **Server:** PostgreSQL only, hosted in the cloud. SQLite is not a server persistence option.
- **Mobile:** SQLite remains the local operational store, supporting offline reads/writes and a durable Outbox.
- **Sync:** authenticated, bidirectional Push/Pull; server PostgreSQL is authoritative for confirmed shared state. Financial approvals/posting may require connectivity according to domain policy.
- **Media:** object/file storage outside relational database rows; PostgreSQL stores authorized metadata and references.

## Architecture guardrails

- One independent backend deployment and PostgreSQL database per NGO. No tenant partition key is required in ordinary per-installation records.
- Modular monolith and explicit bounded contexts first; do not introduce microservices without measured need.
- Domain and Application remain independent of EF Core/Npgsql, HTTP, MAUI, and UI frameworks.
- Separate Domain models, API contracts, persistence mappings, and local SQLite projections.
- Posted financial transactions and accountability submissions are auditable and corrected through explicit reversals/amendments, not destructive edits or generic last-write-wins.
- Keep sensitive stakeholder data and financial documents access-controlled; use synthetic data in tests.

The [per-NGO deployment manual](../deployment-manual.md) describes the current hosting direction. Revalidate provider quotas and pricing before deployment; free tiers are not production availability guarantees.
