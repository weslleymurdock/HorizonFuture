---
name: maui-blazor-hybrid
description: Guides HorizonFuture MAUI Blazor Hybrid development, including MudBlazor UI, platform integration, offline-first behavior, and REST consumption. Use when editing src/HF.Mobile or designing mobile workflows.
---

# MAUI Blazor Hybrid

## Inspect the project

Read `HF.Mobile.csproj`, `MauiProgram.cs`, `MainPage.xaml`, Razor routes/layouts, and platform files first. Current app uses .NET 10, `Microsoft.NET.Sdk.Razor`, `UseMaui`, `SingleProject`, and `BlazorWebView`. Targets differ by host OS. The project currently lacks MudBlazor, REST integration, authentication, SQLite, and production workflows.

Keep UI/device code in Mobile; shared domain/use-case behavior belongs in its corresponding project. Don't claim template capabilities are implemented business behavior.

## MudBlazor workflow

1. Read the package version actually resolved for the project.
2. Query the workspace MCP named `mudblazor` before writing component markup. Use `search_components`, then `get_component_detail`, `get_component_parameters`, or `get_component_examples`; query enum values/API reference as needed.
3. Confirm the returned API matches the installed version. If the MCP is unavailable or mismatched, consult exact-version official docs rather than guessing.
4. If MudBlazor is not referenced, add it only as a deliberate dependency, register its services in `MauiProgram.cs`, and configure required styles/assets in the BlazorWebView host page. Verify current package guidance and target-platform behavior.
5. Keep HTTP, domain rules, persistence, and sync out of Razor markup.

MudBlazor runs inside embedded `BlazorWebView`. Design for narrow screens, touch, safe areas, keyboard and screen readers, and light/dark themes. Don't assume normal browser storage or an unrestricted desktop viewport.

## Offline and privacy

- Once local persistence exists, normal supported reads/writes use local SQLite and do not wait for network access.
- Use application/repository abstractions. Commit user change and Outbox record in one transaction; recover safely after app termination.
- Use platform secure storage for device credentials. Do not store secrets or student PII in logs, URLs, preferences, or sample fixtures.
- Client filtering is not authorization. API checks organization/unit membership on every operation.
- Render offline, stale, pending, retry, and conflict states truthfully.
- Background scheduling is best-effort; foreground app use must recover queued work.

## Platform lifecycle and verification

Consider suspension, termination, network changes, permissions, and target-specific behavior. Pass cancellation tokens to I/O boundaries and dispose subscriptions with component lifetimes. Build only frameworks supported by the current host and report the exact target checked. Add deterministic service/domain tests for behavior; inspect representative narrow-screen UI states when available.
