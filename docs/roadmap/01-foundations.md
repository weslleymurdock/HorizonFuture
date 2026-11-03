# 01 — Foundations and domain discovery

## Objective

Turn the starter repository into an agreed, testable implementation plan grounded in the NGO's actual workflows before choosing schemas or sync behavior.

## Dependencies

None. Current baseline: .NET 10 projects under `src/`; `HF.Mobile` is a MAUI Blazor Hybrid template, `HF.Web` is a Blazor WebAssembly template with MudBlazor 9.*, and `HF.API` contains the generated weather sample. No product domain, production client workflows, API integration, tests, authentication, persistence, or sync code is present.

## Work

- Interview stakeholders and map roles, organizations, school units, users, devices, student/family records, enrollment, attendance, and workflows actually in scope.
- Define offline use cases, expected outage duration, shared-device use, account lifecycle, data volumes, supported platforms, accessibility, and connectivity constraints.
- Define data classification, minimum necessary collection, retention/deletion, audit needs, and who may view/change each record.
- Record architecture decisions and assumptions, including stable identifier strategy, API compatibility expectations, local schema support window, and provider evaluation criteria.
- Replace starter template content only as implementation work begins; keep this documentation explicit that the templates currently remain.

## Acceptance criteria

- A reviewed domain glossary, actor/permission matrix, prioritized offline use cases, data inventory, and threat/privacy review exist.
- Stakeholders agree which workflows must work fully offline and which require a server.
- Architecture decisions and unresolved questions are recorded with owners/evidence needed.
- Baseline build and repository structure are documented without claiming unimplemented features.

## Risks and open decisions

Exact school-unit relationships, roles, sensitive fields, required offline duration, device sharing, legal retention obligations, and operational ownership are unknown until discovery.
