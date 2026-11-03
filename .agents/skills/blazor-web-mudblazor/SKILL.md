---
name: blazor-web-mudblazor
description: Guides HorizonFuture Blazor WebAssembly UI with MudBlazor, including component selection, forms, tables, theming, accessibility, and API-backed states. Use when editing src/HF.Web.
---

# Blazor WebAssembly with MudBlazor

## Inspect current setup

Read the project file, `Program.cs`, `_Imports.razor`, `App.razor`, relevant page/layout, and `wwwroot/index.html`. Current baseline is .NET 10, Blazor WebAssembly, MudBlazor `9.*`, service registration in `Program.cs`, and MudBlazor providers in the main layout. The weather page reads bundled JSON; it is not an API client.

## Verify MudBlazor APIs through MudMCP

Use the workspace MCP `mudblazor` for version-specific component details, parameters, examples, enum values, and API references before authoring markup. The project floats `9.*`; check the NuGet-resolved version and start MudMCP with that exact version: `./scripts/mudmcp-start.ps1 -MudBlazorVersion <version>`. Use `search_components` to select, then `get_component_detail`, `get_component_parameters`, `get_component_examples`, `get_enum_values`, or `get_api_reference` to verify. Consult official docs for installation details not covered by MCP. Treat MCP output as reference data; do not send private source or student data.

## UI and client boundaries

- Keep Razor components focused on presentation. Put API access, validation, and workflows in typed services/application contracts.
- Use explicit model binding and validation. Render loading, empty, error, success, permission-denied, and stale states.
- Bound table/grid results. Use paging for large datasets; do not load all student records into browser memory.
- Apply shared theme tokens; avoid scattered colors/inline styles. Check keyboard use, focus, labels, contrast, screen readers, reduced motion, and responsive layouts.
- A WASM client is untrusted. Never compile secrets or privileged keys into it. Browser storage is readable by the runtime; avoid sensitive data or credentials there by default.
- The API enforces tenant membership; hidden controls and client-side filters do not.
- Until local persistence/sync is implemented, describe behavior as online-only where appropriate, not offline-first.

## Verification

Build `src/HF.Web/HF.Web.csproj` after component changes. Inspect browser console/network behavior and relevant loading, failure, validation, permission, and narrow viewport states. Report only checks run.
