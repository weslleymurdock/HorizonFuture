# 02 — Domain and data model

## Objective

Define stable domain concepts and tenant ownership before implementing persistence or sync contracts.

## Dependencies

Phase 01.

## Work

- Model only entities confirmed in discovery. Establish whether each record is organization-scoped, school-unit-scoped, or linked to both.
- Define identifiers that can be generated offline without collision and remain stable across devices and server storage.
- Define relationships, lifecycle states, soft/hard deletion rules, audit needs, and validation invariants.
- Separate domain models, API DTOs, and persistence mappings where their lifecycles differ.
- Define server and local schema ownership, versioning, and compatibility expectations.
- Define entity-specific concurrency and conflict requirements as inputs to phase 08; do not let storage shape determine policy by accident.

## Acceptance criteria

- Domain glossary and diagrams match stakeholder-approved workflows.
- Tenant ownership and authorization-relevant relationships are explicit for every entity.
- Identifier, deletion, version, and audit semantics are documented.
- Proposed schemas can represent offline-created records and future migration without reusing identifiers.
- Domain tests cover core invariants once implementation begins.

## Risks and open decisions

Avoid importing unconfirmed entities or collecting unnecessary student data. Final model depends on phase 01 discovery.
