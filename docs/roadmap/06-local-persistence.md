# 06 — MAUI client and local SQLite

## Objective

Provide a versioned local operational store and app foundation so supported workflows work without a network connection.

## Dependencies

Phases 01–02 and the API contract direction from phase 04.

## Work

- Add a distinct .NET MAUI application with platform-specific configuration; keep UI/device APIs outside server/domain projects.
- Implement local SQLite schema, migrations, repositories, and transaction boundaries for user changes and Outbox records.
- Choose local encryption and key storage based on supported platform capabilities and the privacy threat model.
- Define application startup, sign-in/session behavior, local account switching, device identity, and safe handling of multiple users on shared devices.
- Ensure local reads and writes do not require connectivity. Surface pending sync and stale-data state honestly.
- Create synthetic seed data for development and tests.

## Acceptance criteria

- A supported user workflow completes offline, survives app restart, and persists locally.
- Schema upgrades from supported prior versions preserve data or fail with a documented recovery path.
- Local mutations and Outbox records are atomic.
- Sensitive local storage and account-switch behavior pass security review for target platforms.

## Risks and open decisions

Supported platforms, encryption implementation, offline session expiry, and shared-device policy depend on phase 01 and security review.
