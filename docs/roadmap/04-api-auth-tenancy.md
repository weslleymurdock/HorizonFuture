# 04 — REST API, identity, devices, and tenant isolation

## Objective

Establish the server trust boundary before exposing organization or student data.

## Dependencies

Phases 01–03.

## Work

- Replace the generated sample endpoint as API implementation proceeds; define versioned REST contracts with explicit request/response DTOs and error semantics.
- Choose and document identity, token issuance/refresh/revocation, role/capability model, and membership lifecycle.
- Define device registration, user/device association, credential rotation and revocation, lost-device handling, and secure client storage requirements.
- Resolve organization/unit access from the authenticated principal and server-side membership. Treat submitted scope IDs as untrusted and verify each operation.
- Apply authorization consistently to reads, writes, Push, Pull, media metadata, and administrative operations.
- Add rate limits, input validation, request size controls, secure headers, and audit events as appropriate to the threat model.

## Acceptance criteria

- API contract and identity/device lifecycle are documented and versioned.
- Automated tests prove unauthenticated, cross-organization, cross-unit, revoked-user, and revoked-device access is denied.
- Scope checks are server-side and cannot be bypassed by changing a request identifier.
- No sensitive values or student payloads are written to logs.

## Risks and open decisions

Identity provider, invitation/provisioning process, offline token policy, role definitions, and unit reassignment behavior require stakeholder/security review.
