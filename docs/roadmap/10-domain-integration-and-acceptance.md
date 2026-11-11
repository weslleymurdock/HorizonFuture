# 10 — Domain integration, invariants, and model acceptance

## Objective
Consolidate the social, project, funding, and financial models into a consistent backend domain before infrastructure and client implementation begin.

## Dependencies
Stages 01–09.

## Work
- Define bounded contexts and aggregates for organization/identity, people/service delivery, projects/impact, funding, financial management, accountability, and documents.
- Define aggregate ownership, transactional boundaries, domain events, identifiers, lifecycle transitions, and cross-context references.
- Document critical invariants: source restrictions; project/source/rubric allocations; no overspending beyond approved rules; balanced transaction splits; immutable posted transactions; transfer neutrality; idempotent receipts; reconciled statement uniqueness; and traceable report snapshots.
- Define whether aggregate interactions require synchronous application transactions or durable domain/integration events; avoid a distributed transaction across external systems.
- Review financial workflows with a domain expert/accountant and validate funding/accountability scenarios with NGO stakeholders.
- Define API-facing use cases and authorization requirements without designing UI-specific models.
- Define offline-created financial operations and server-authoritative decisions, including approval and posting rules.
- Produce ER diagrams, state machines, context map, glossary, decision records, and synthetic reference scenarios.

## Acceptance criteria
- Domain model review is signed off before MAUI implementation.
- Every core entity has lifecycle, ownership, authorization, audit, deletion, and concurrency semantics.
- Financial invariants have executable domain tests once implemented and traceability to acceptance scenarios.
- Explicit MVP/deferred scope prevents accidental full ERP/accounting expansion.

## Risks and open decisions
Do not prematurely split into microservices. Start with a modular monolith and clear bounded contexts unless measured operational requirements justify distribution.
