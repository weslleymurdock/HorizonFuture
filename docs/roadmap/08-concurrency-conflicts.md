# 08 — Optimistic concurrency and conflict policies

## Objective

Detect concurrent edits and resolve them according to explicit domain semantics instead of silent overwrite.

## Dependencies

Phases 02, 05, and 07.

## Work

- Select server/client version tokens and define stale-write responses in the sync contract.
- For each mutable entity and meaningful field, document mergeable fields, append-only operations, server-authoritative values, rejection/reload behavior, or user-mediated review.
- Preserve both local intent and current server state where a conflict requires resolution.
- Define safe retry, superseding edits, conflict audit, and deletion-versus-update behavior.
- Expose actionable conflict state in the client without leaking records beyond the user's scope.

## Acceptance criteria

- Every synchronized mutable entity has an approved conflict policy.
- Tests cover concurrent edits from two devices, stale base version, independent-field changes, delete/update races, and duplicate retries.
- No generic last-write-wins rule silently resolves sensitive records.
- Conflict outcomes remain idempotent and auditable.

## Risks and open decisions

Policies depend on domain and user workflows. The correct behavior may vary between attendance events and personal profile fields.
