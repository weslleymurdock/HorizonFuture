# 03 — Central store compatibility gate

## Objective

Determine whether the current EF Core and a maintained Turso/libSQL provider can safely meet required server persistence and sync invariants.

## Dependencies

Phases 01–02.

## Work

- Build a small isolated compatibility spike using the exact intended .NET/EF Core and provider versions.
- Exercise transactions covering domain writes plus Inbox and ordered change-feed writes; verify rollback and duplicate handling.
- Verify required query translation, tenant-filter queries, constraints, generated values, migrations, concurrency tokens, paging/order, connection pooling, and retry behavior.
- Test failure, reconnect, backup/restore, and deployment connectivity against an appropriately isolated environment.
- Record version support, known limitations, reproducible commands/results, operational cost/limits assumptions, and go/no-go decision.
- If required guarantees fail, compare alternatives and document the decision before server persistence work depends on the provider.

## Acceptance criteria

- A repeatable spike and written evidence cover every required capability.
- A provider decision is approved and includes exact package/runtime versions and limitations.
- No production schema or sync contract depends on unvalidated provider behavior.

## Risks and open decisions

Provider maintenance, EF Core compatibility, libSQL semantics, and operational terms can change. Revalidate before upgrades and production rollout.
