# 14 — MAUI local SQLite and offline application store

## Objective
Only after backend domain and API foundations are approved, implement the MAUI local operational store for offline-first use.

## Dependencies
Stages 01–13.

## Work
- Extend HF.Mobile with approved operational workflows, using local SQLite exclusively for the device-side store.
- Define local projections and repositories for supported stakeholder, project, activity, funding, and financial workflows; do not blindly mirror every server table.
- Implement versioned SQLite schema, migrations, indexes, transaction boundaries, and local Outbox records.
- Ensure local user mutation and Outbox operation are committed atomically.
- Define secure storage/encryption, device identity, sign-in/session, shared-device account switching, logout, and local data cleanup.
- Clearly distinguish cached balances and pending local operations from server-confirmed financial balances/approvals.
- Make approved read/create workflows available offline; defer server-authoritative posting/approval where policy requires connectivity.
- Seed synthetic development data and define local retention/backup behavior.

## Acceptance criteria
- At least one complete approved workflow works offline, survives restart, and persists locally.
- Local mutations and Outbox operations are atomic.
- Schema upgrades preserve data or provide a documented recovery path.
- Sensitive local data and account switching pass platform security review.

## Risks and open decisions
Do not promise that every financial operation can be finalized offline. Define stale balance, approval, and posting behavior per use case.
