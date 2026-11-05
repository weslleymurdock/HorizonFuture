# 02 — Domain and data model

## Objective

Define stable domain concepts and within-NGO authorization relationships before implementing persistence or sync contracts. Each installation is single-NGO; do not model a tenant partition key into ordinary application records.

## Dependencies

Phase 01.

## Work

- Model only entities confirmed in discovery. Each application data store belongs to one NGO installation; establish school-unit and user ownership semantics within that NGO.
- Define identifiers that can be generated offline without collision and remain stable across devices and server storage.
- Define relationships, lifecycle states, soft/hard deletion rules, audit needs, and validation invariants.
- Separate domain models, API DTOs, and persistence mappings where their lifecycles differ.
- Define server and local schema ownership, versioning, and compatibility expectations.
- Define entity-specific concurrency and conflict requirements as inputs to phase 08; do not let storage shape determine policy by accident.

## Acceptance criteria

- Domain glossary and diagrams match stakeholder-approved workflows.
- Within-NGO authorization-relevant relationships are explicit for every entity; per-installation records do not require a tenant partition key.
- Identifier, deletion, version, and audit semantics are documented.
- Proposed schemas can represent offline-created records and future migration without reusing identifiers.
- Domain tests cover core invariants once implementation begins.

## Risks and open decisions

Avoid importing unconfirmed entities or collecting unnecessary student data. Final model depends on phase 01 discovery.
