# Implementation roadmap

This ordered roadmap describes planned work for the early repository at `src/`. It is not evidence that any phase is implemented.

| Order | File | Outcome |
|---|---|---|
| 01 | [01-foundations.md](01-foundations.md) | Requirements, domain discovery, architecture decisions, and repository baseline |
| 02 | [02-domain-data-model.md](02-domain-data-model.md) | Domain, within-installation authorization, identifiers, and schema design |
| 03 | [03-central-store-validation.md](03-central-store-validation.md) | Managed PostgreSQL and EF Core/Npgsql compatibility gate |
| 04 | [04-api-auth-tenancy.md](04-api-auth-tenancy.md) | REST contracts, identity, device registration, and single-installation authorization |
| 05 | [05-server-persistence.md](05-server-persistence.md) | Server persistence, migrations, constraints, and change-feed records |
| 06 | [06-local-persistence.md](06-local-persistence.md) | MAUI shell and versioned local SQLite repository |
| 07 | [07-sync-protocol.md](07-sync-protocol.md) | Transactional Outbox/Inbox, idempotent Push, ordered Pull cursor |
| 08 | [08-concurrency-conflicts.md](08-concurrency-conflicts.md) | Optimistic concurrency and entity-specific conflict resolution |
| 09 | [09-maui-sync-media.md](09-maui-sync-media.md) | Offline UX, scheduling, device lifecycle, and external media transfer |
| 10 | [10-tests-observability.md](10-tests-observability.md) | Automated verification, diagnostics, and operational metrics |
| 11 | [11-deployment-rollout.md](11-deployment-rollout.md) | Secure per-NGO deployment, staged rollout, recovery, and support |

The [per-NGO deployment manual](../deployment-manual.md) records provider selection and the dated official free-tier comparison.

Phases are ordered by dependency. Revisit later phases if compatibility or domain discovery changes earlier decisions, and record the reason rather than silently changing protocol assumptions.
