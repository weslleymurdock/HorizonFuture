# 01 — Product discovery and shared domain foundation

## Objective

Preserve the product discovery baseline and implement the minimal executable shared kernel required by later NGO domain stages. This stage does not implement organization, stakeholder, social-service, project, funding, or financial business capabilities; those belong to Stages 02–09.

## Current status

Discovery documentation and the shared domain kernel exist under [docs/discovery](../discovery/README.md). Stage 01 implementation is complete; final acceptance remains **pending automated test execution** because the current test adapter discovers no tests on this host.

External validation with the pilot NGO and specialists is intentionally deferred until after the first stable release. This is a project sequencing decision, not a missing Stage 01 deliverable. See the [post-release pilot validation plan](../discovery/07-decisions-and-traceability.md#post-release-pilot-validation-plan).

## Dependencies

None.

## Existing baseline

The repository already contains `HF.Domain.Common.Entities.IEntityBase` and `EntityBase`. The latter generates string GUID v7 identifiers and carries CreatedAt/By and UpdatedAt/By audit fields. Existing educational and operational models (students, teachers, volunteers, collaborators, courses, enrollments, attendance, assets, and liabilities) must be inspected and preserved unless a concrete compatibility issue is demonstrated.

Do not rewrite `EntityBase` merely to fit a preferred pattern.

## Work

### Discovery baseline

- Preserve and use existing discovery artifacts to guide Stages 02–10.
- Keep assumptions, open decisions, and synthetic scenarios explicit.
- Do not require interviews or approval from the pilot NGO during this stage.

### Shared domain kernel

Implement only reusable, framework-independent primitives needed by later bounded contexts:

1. A value-object abstraction with value equality based on explicitly supplied equality components.
2. A domain-event contract carrying a UTC occurrence timestamp.
3. An aggregate-root base type extending the existing `EntityBase`, exposing pending domain events read-only and supporting controlled registration and clearing.
4. A domain exception carrying a stable machine-readable error code and human-readable message.

Intended contracts and relationships:

```text
IEntityBase
  └── EntityBase
        └── AggregateRoot ── owns 0..N IDomainEvent

ValueObject (independent equality abstraction)
DomainException (framework-independent rule failure)
```

Rules:

- Keep `IEntityBase` and current `EntityBase` identity/audit semantics compatible.
- `AggregateRoot` derives from `EntityBase`; only aggregate roots own/raise domain events. Child entities do not publish independently.
- `IDomainEvent` exposes `DateTimeOffset OccurredOnUtc`; later bounded contexts define immutable event records.
- Aggregate roots expose events as `IReadOnlyCollection<IDomainEvent>`; registration is protected for derived aggregate behavior, and clearing is explicit for a future dispatch boundary.
- `ValueObject` implements equality and hash code from ordered equality components supplied by each derived value object. No EF Core or serialization dependency.
- `DomainException` contains a non-empty stable `Code`, message, and optional inner exception. No HTTP/ProblemDetails dependency.
- Do not introduce event dispatcher, event bus, mediator integration, repository, Unit of Work, domain service, or persistence mapping in this stage.

## Explicit exclusions

Do not implement business entities/aggregates for Stages 02–09, including Organization, Unit, Person/Stakeholder, Program, Beneficiary, Service, Project, Indicator, FundingSource, FundingInstrument, Grant, FinancialAccount, Budget, Allocation, Commitment, FinancialTransaction, Reconciliation, AccountabilityReport, or Document.

Do not modify API routes, application use cases, EF Core mappings, migrations, authentication, clients, MAUI, synchronization, or deployment.

## Acceptance criteria

- Shared domain kernel contracts and base types are implemented under `HF.Domain`.
- Existing `IEntityBase` and `EntityBase` behavior remains compatible.
- Unit tests cover GUID v7 identity, audit defaults, value-object equality/hash semantics, aggregate event registration/read-only exposure/clearing, UTC event contract, and domain exception code/message/inner exception.
- `HF.Domain` has no dependency on Application, Infrastructure, API, EF Core, Npgsql, HTTP, MAUI, or UI frameworks.
- Full solution builds without regressions.
- Existing tests pass; new tests are deterministic and use no external services.
- No business model belonging to Stages 02–09 is introduced.
- Documentation does not present discovery assumptions as stakeholder-validated facts.

## Risks and open decisions

The discovery baseline contains assumptions about Brazilian NGO funding and accounting workflows. They guide initial implementation but are not externally validated facts. After the first stable release, the pilot NGO and relevant specialists will validate real workflows; discrepancies must be recorded and prioritized before broad production adoption. Do not claim statutory accounting or legal compliance without qualified review.
