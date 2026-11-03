---
name: rest-api-consumer
description: Guides implementation of HorizonFuture REST consumers in MAUI and Blazor Web, including contract discovery, typed clients, auth-aware errors, tenant boundaries, and sync-safe retries.
---

# REST API consumer

## Discover the contract

Inspect `src/HF.API` routes, DTOs, OpenAPI setup, and relevant `HF.Application` use cases before editing a consumer. The API currently has a generated weather sample; production auth, school resources, and sync endpoints are not implemented. Never invent routes.

Confirm base URLs, auth flow, DTOs, validation/error format, pagination, tenant scope, and version support from actual API code or approved contracts. Keep transport concerns out of Domain. Share contract types only where a build-safe boundary is deliberately established.

## Client patterns

- Register purpose-specific or typed `HttpClient` through `MauiProgram.cs` or WebAssembly `Program.cs`.
- Configure base URLs per environment. Never compile secrets, private signing keys, or privileged API credentials into a client.
- Use separate DTOs where transport and UI/domain lifecycles differ.
- Use async APIs and CancellationToken; avoid sync-over-async; bound response sizes.
- Map validation, unauthorized, forbidden, conflict, throttling, and server errors to explicit outcomes. Don't show raw exception/server details.
- Keep HTTP calls out of Razor components.

## Tenant, privacy, and sync

Treat the API as the authorization boundary. Organization/unit IDs, route parameters, hidden controls, and local filters are untrusted; server membership checks are required on every request. Do not log tokens, student PII, or request bodies. Use synthetic test data.

Until local storage and synchronization exist, do not claim offline behavior. Once sync is implemented, persist local edits and Outbox entries atomically, send stable idempotency IDs, retry safely, and handle each operation result. Pull cursors are opaque and server-issued; apply a page and next cursor atomically. Never drop unsent work after a request failure.

## Verification

Use contract/integration tests against real routes and DTOs, including unauthorized and cross-tenant cases, network loss, cancellation, malformed responses, duplicate operation IDs, and supported client versions. Keep live service suites isolated from normal tests.
