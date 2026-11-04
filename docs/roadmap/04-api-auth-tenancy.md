# 04 — REST API, identity, devices, and single-installation authorization

## Objective

Establish the server trust boundary for one NGO installation before exposing student data. The filename and phase number are retained for roadmap stability; this phase does not establish shared-database tenant isolation.

## Dependencies

Phases 01–03.

## Work

- Replace the generated sample endpoint as API implementation proceeds; define versioned REST contracts with explicit request/response DTOs and error semantics.
- Choose and document identity, token issuance/refresh/revocation, role/capability model, and membership lifecycle.
- Define device registration, user/device association, credential rotation and revocation, lost-device handling, and secure client storage requirements.
- The API and database serve one NGO installation. Resolve user, role, and school-unit access from authenticated identity and server-side authorization data. Treat submitted scope IDs as untrusted and verify each operation; do not require a tenant identifier to select among NGOs.
- Apply authorization consistently to reads, writes, Push, Pull, media metadata, and administrative operations.
- Add rate limits, input validation, request size controls, secure headers, and audit events as appropriate to the threat model.

## Acceptance criteria

- API contract and identity/device lifecycle are documented and versioned.
- Automated tests prove unauthenticated, unauthorized-user, cross-unit, revoked-user, and revoked-device access is denied.
- Scope checks are server-side and cannot be bypassed by changing a request identifier.
- No sensitive values or student payloads are written to logs.

## Risks and open decisions

Identity provider, invitation/provisioning process, offline token policy, role definitions, and unit reassignment behavior require stakeholder/security review.
