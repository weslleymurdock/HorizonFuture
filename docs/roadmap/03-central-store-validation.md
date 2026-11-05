# 03 — PostgreSQL provider compatibility gate

## Objective

Validate the planned managed PostgreSQL provider and EF Core/Npgsql setup against server persistence and sync invariants.

## Dependencies

Phases 01–02.

## Work

- Build an isolated compatibility spike using the intended .NET, EF Core, and Npgsql versions against PostgreSQL.
- Exercise transactions covering domain writes plus Inbox and ordered change-feed writes; verify rollback and duplicate handling.
- Verify required query translation, within-installation authorization queries, constraints, generated values, migrations, concurrency tokens, paging/order, connection pooling, and retry behavior.
- Test failure, reconnect, backup/restore, and deployment connectivity in an isolated environment.
- Record version support, limitations, reproducible commands/results, operational cost/limits, and provider decision.
- Keep provider-specific code behind Infrastructure abstractions and migrations portable where practical; do not treat provider choice as a sync protocol change.

## Acceptance criteria

- A repeatable spike and written evidence cover every required capability.
- The PostgreSQL provider decision includes exact package/runtime versions and limitations.
- No production schema or sync contract depends on unvalidated provider behavior.

## Risks and open decisions

Provider maintenance, EF Core compatibility, PostgreSQL semantics, and operational terms can change. Revalidate before upgrades and production rollout.
