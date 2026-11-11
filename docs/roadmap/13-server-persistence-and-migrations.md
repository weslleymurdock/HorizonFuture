# 13 — PostgreSQL persistence, migrations, and financial audit records

## Objective
Implement the approved backend domain model in PostgreSQL with reliable migrations, constraints, audit, and transaction boundaries.

## Dependencies
Stages 01–12.

## Work
- Map aggregates and value objects to PostgreSQL using EF Core; enforce relationships, uniqueness, ownership, and lifecycle constraints.
- Use PostgreSQL numeric/decimal columns for money and explicit precision/scale; define rounding at domain boundaries.
- Implement versioned migrations, repeatable test initialization, deployment ordering, expand/migrate/contract, rollback/recovery, and schema compatibility.
- Ensure financial posting, allocation, reversal, and reconciliation state changes are atomic and concurrency-safe.
- Persist approval/audit history, evidence metadata, idempotency records, and ordered change-feed entries with required indexes.
- Define retention for audit, inbox, feed, and tombstones without undermining long-offline clients or accountability obligations.
- Store binary media/documents in external object storage; PostgreSQL stores authorized metadata and stable references.
- Exercise realistic synthetic data volumes and backup/restore.

## Acceptance criteria
- Fresh database creation and supported upgrades are reproducible.
- Integration tests prove financial operations and related audit/change-feed writes commit or roll back together.
- Constraints prevent invalid allocation totals, duplicate idempotency keys, and invalid state transitions where expressible.
- Backup/restore and migration recovery are exercised in non-production.

## Risks and open decisions
Do not encode complex funder-specific business rules only as database constraints. Retention and legal hold need explicit policy.
