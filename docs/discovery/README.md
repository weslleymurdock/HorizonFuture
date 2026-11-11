# Product discovery and domain requirements

## Purpose

This set records the initial discovery for HorizonFuture — Olhar do
Futuro for Brazilian nonprofit organizations (NGOs and OSCs).
It defines problems, processes, observable requirements, and still-open
decisions to guide roadmap stages 01–10. It is not a description of
features already implemented.

## Evidence and conventions

- **Implemented**: confirmed by code or existing repository documentation.
- **Established guideline**: an architectural decision already recorded that
  should not be reconsidered at this stage.
- **Discovered requirement**: behavior the product should provide after
  validation and implementation.
- **Hypothesis**: interpretation to validate with the pilot NGO or a qualified specialist after the first stable release.
- **Pending**: cannot be accepted without external evidence or an explicit
  decision.

Examples and monetary values are synthetic. No document in this directory
contains real beneficiary, student, donor, or organization data.

## Documents

| Document | Content |
|---|---|
| [01-product-and-operating-models.md](01-product-and-operating-models.md) | Vision, objectives, NGO profiles, personas, and capability-level competition |
| [02-actors-glossary-and-data.md](02-actors-glossary-and-data.md) | Actors, reusable identity, glossary, and data inventory |
| [03-operational-workflows.md](03-operational-workflows.md) | Governance, social services, projects, fundraising, and financial scenarios |
| [04-finance-budget-and-accountability.md](04-finance-budget-and-accountability.md) | Restricted resources, budgeting, operations, reconciliation, and accountability |
| [05-privacy-security-and-offline.md](05-privacy-security-and-offline.md) | LGPD, security, segregation of duties, evidence, and offline operation |
| [06-requirements-mvp-and-backlog.md](06-requirements-mvp-and-backlog.md) | Prioritized requirements catalog, MVP, and deferred capabilities |
| [07-decisions-and-traceability.md](07-decisions-and-traceability.md) | Decision register, traceability matrix, and acceptance checklist |

## Stage status

Internal discovery documentation is recorded. Acceptance of stage 01 remains
**pending**, because review of financial terminology and accountability
obligations by an NGO finance professional and a qualified accountant or
advisor still has no recorded evidence. See [the acceptance checklist](07-decisions-and-traceability.md#stage-01-acceptance-checklist).

## Boundaries

This discovery does not define entities, aggregates, tables, endpoints, sync
payloads, or persistence contracts. Those decisions belong to later roadmap
stages and must preserve independent installation per NGO, PostgreSQL on the
server, SQLite only in the MAUI client, and separation between domain,
application, infrastructure, API, and presentation.
