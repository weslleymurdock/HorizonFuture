# 16 — Optimistic concurrency and financial conflict policies

## Objective
Define conflict behavior per domain entity, with stricter safeguards for financial records and approvals.

## Dependencies
Stages 06–09 and 13–15.

## Work
- Define version tokens and stale-write responses.
- Classify fields/operations as mergeable, append-only, server-authoritative, reject-and-refresh, or user-mediated.
- Treat posted financial transactions, approvals, reconciliations, submitted accountability reports, and audit records as immutable; correct through explicit reversal/amendment workflows.
- Define conflicts for budgets changed by another device, competing allocations, exhausted funding limits, duplicate receipts, transfers, and concurrent statement reconciliation.
- Preserve local intent and server state for review; never silently overwrite financial values or approvals.
- Define conflict resolution authorization, audit reason, idempotent retry, and superseding operations.

## Acceptance criteria
- Every synchronized aggregate has an approved policy.
- Tests cover two-device concurrent edits, stale budget versions, allocation races, duplicate financial operations, reversal races, and delete/update.
- No generic last-write-wins rule can silently change posted financial state.
- Conflict resolution is auditable and idempotent.

## Risks and open decisions
Offline editing may create provisional intents, not final financial authorization. Define which decisions require online confirmation.
