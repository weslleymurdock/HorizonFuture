# Architecture and data boundaries

## Current repository baseline

The repository contains an ASP.NET Core API project and separate Domain, Application, Infrastructure, and Composition projects, plus two UI scaffolds: `HF.Mobile` is a MAUI Blazor Hybrid template and `HF.Web` is a Blazor WebAssembly template using MudBlazor 9.*. The mobile scaffold does not yet reference MudBlazor. Both clients still lack production workflows, API integration, authentication, local persistence, and sync. The API still contains a weather sample. These are templates, not completed product capabilities.

## Planned system

The target is an offline-first .NET MAUI client with local SQLite and an ASP.NET Core REST backend. Provision one independent backend deployment and PostgreSQL database per NGO. Configure each distributed MAUI app with that installation’s backend URL. A browser Blazor client, if used, is also configured for that installation. Normal operation does not depend on a shared cross-NGO database or service. Domain rules belong in Domain; use cases and ports in Application; storage/network adapters in Infrastructure; HTTP transport and authorization in API; host wiring in Composition; UI/device integration stay in their respective clients.

## Database provider

PostgreSQL is the planned managed server database; Neon is selected for the largest verified free PostgreSQL storage among the evaluated providers as of 2026-11-03 (1 GB/project versus Supabase Free at 500 MB/project). Free limits are evaluation/development allowances, not production availability guarantees. See the [deployment manual](../deployment-manual.md) for source URLs and current caveats. Keep provider-specific code behind Infrastructure abstractions and migrations portable where practical. Validate exact EF Core/Npgsql versions, transactions, generated values, concurrency, retries, backup/restore, and migration behavior before production.

The client SQLite schema and server schema are separately versioned. Offline clients may miss several server releases, so API and sync payload changes require explicit compatibility policy and migrations.

## Installation and school-unit boundaries

Each backend/database installation belongs to one NGO. Do not add tenant partitioning or tenant identifiers to the ordinary per-installation application data path. Within an NGO, authenticate users and enforce role, ownership, and school-unit permissions on the server for every operation; client filters are not authorization. An optional future cross-installation reporting/aggregation service must be separately deployed, explicitly authorized by each participating NGO, and isolated from normal app availability and sync.

Media is stored outside relational rows. Relational records carry only authorized metadata and stable references. Transfer tokens, access paths, retention, deletion, and retry behavior need independent design.

## Decision records

Document material choices such as provider selection, identity model, per-NGO installation boundaries, stable identifiers, cursor format, payload evolution, and media storage. Include alternatives considered, evidence, consequences, and migration implications.
