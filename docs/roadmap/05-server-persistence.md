# 05 — Server persistence and change records

## Objective

Implement server-side schema and persistence only after the provider gate and domain model are approved.

## Dependencies

Phases 01–04, including a passing phase 03 provider decision.

## Work

- Map confirmed domain entities and tenant ownership to the chosen provider; enforce tenant keys, relationships, unique constraints, and concurrency metadata.
- Create versioned migrations and repeatable local/test database initialization. Define deployment migration ordering and rollback/recovery strategy.
- Design Inbox idempotency records and ordered change-feed entries in a way that can be committed atomically with domain effects.
- Define retention and cleanup policies for deduplication records, change feed, audit records, and tombstones while honoring client offline support.
- Keep media in external object/file storage; store metadata and references in relational data.
- Validate tenant-filtered access patterns and feed indexes with realistic synthetic data.

## Acceptance criteria

- Migrations create a fresh database and upgrade supported prior versions reproducibly.
- Transaction tests prove domain writes, Inbox results, and feed entries commit or roll back together.
- Constraints and queries enforce organization/unit boundaries.
- Backup, restore, retention, and tombstone behavior are documented and exercised in a non-production environment.

## Risks and open decisions

Turso/libSQL viability remains conditional on phase 03. Retention windows must account for long-offline devices and privacy requirements.
