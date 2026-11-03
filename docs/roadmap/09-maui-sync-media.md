# 09 — MAUI integration, scheduling, and media

## Objective

Integrate the protocol with usable offline UX, platform schedulers, device lifecycle, and separately stored media.

## Dependencies

Phases 06–08.

## Work

- Build a sync coordinator that safely serializes or bounds concurrent runs, resumes after restart, and reacts to connectivity without making connectivity a prerequisite for normal local use.
- Use platform background scheduling as best-effort; provide foreground/manual sync and truthful last-sync/pending/conflict status.
- Implement secure device registration, credential refresh/revocation, local account switching, and logout/remote-wipe or local-cleanup policy as approved.
- Define external media storage, authorized upload/download metadata, resumable transfer or safe restart, integrity checks, size/type limits, retention, and deletion.
- Queue media references and domain records consistently while handling media transfer independently from relational sync where appropriate.
- Provide accessible states for offline, retrying, stale data, conflicts, and required user action.

## Acceptance criteria

- Airplane-mode and intermittent-network exercises show local workflows remain available and sync resumes safely.
- Background scheduling failures do not lose queued work; manual/foreground sync remains usable.
- Interrupted media transfer can resume or restart without corrupting domain records or exposing unauthorized content.
- Device revocation and account switching behavior are verified on supported platforms.

## Risks and open decisions

Platform scheduler guarantees, secure storage, media provider, bandwidth constraints, and deletion obligations need platform and operational validation.
