# 17 — MAUI synchronization UX, scheduling, and evidence/media

## Objective
Integrate synchronization into the MAUI workflows with honest offline state, reliable recovery, and secure document/evidence transfer.

## Dependencies
Stages 14–16.

## Work
- Build a sync coordinator that serializes or bounds runs, resumes after restart, and reacts to connectivity.
- Provide foreground/manual sync and best-effort platform background scheduling; background failure must not lose work.
- Show pending operations, last successful sync, stale data, conflicts, and server-confirmed versus provisional financial status.
- Implement device registration/revocation, token refresh, account switching, logout, and approved cleanup/wipe policy.
- Implement external object storage integration for receipts, agreements, bank statements, photos, and project evidence with scoped access, resumable/restartable transfer, integrity checks, size/type limits, retention, and deletion.
- Ensure media references and domain records remain consistent while media transfer can retry independently.
- Provide accessible failure/retry and user-action states.

## Acceptance criteria
- Airplane-mode/intermittent-network tests show local workflows remain usable and sync resumes safely.
- Scheduling failures do not lose queued operations.
- Interrupted uploads/downloads recover without corrupting records or exposing documents.
- Device revocation and account switching are verified on supported platforms.

## Risks and open decisions
Background execution is best-effort and varies by platform. Do not represent a pending local expense as a posted cloud transaction.
