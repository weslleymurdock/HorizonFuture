# API application feature pattern

This document defines the required organization for API business features and Mediator use cases.

## Feature organization

- Organize business rules by **Feature**, grouping each feature's related HTTP entry point, commands, queries, handlers, service, and request/response contracts together.
- Each feature has one controller responsible for all HTTP dispatches belonging to that feature. The controller maps HTTP input to a command or query, sends it through Mediator, and translates the returned result into the HTTP response.
- Keep controllers thin. They must not contain business rules, access repositories, or access the DbContext directly.
- Put feature commands and queries in the Application layer. Handlers are the Mediator boundary for those requests.
- A command/query handler delegates the feature's business operation to that feature's service. Keep the handler focused on orchestration and Mediator concerns; do not duplicate business logic in handlers.
- The feature service implements business rules and uses the generic repository abstraction for data access. Do not inject or use EF DbContext directly in controllers, handlers, or feature services.
- Repositories and the Unit of Work are implemented in Infrastructure and registered through Composition. Preserve project boundaries: Domain remains framework-independent, Application owns use cases and abstractions, Infrastructure owns persistence adapters, API owns HTTP concerns, and Composition wires dependencies.

The intended call flow is:

```text
Feature Controller
  -> Mediator command/query
    -> feature command/query handler
      -> feature service
        -> generic repository
          -> scoped DbContext
      <- service result
    <- ResponseBase<T>
  <- HTTP response
```

Handlers return the shared `ResponseBase<T>` contract (or the repository's established equivalent) through Mediator to the controller. Preserve consistent success, validation, and failure information in this envelope; the controller is responsible for mapping it to suitable HTTP status codes and response bodies. Do not return persistence entities as API contracts.

## Dependency injection and lifetime

- Register the Mediator with **Scoped** lifetime. Do not change it to Singleton to work around source-generator or startup errors.
- Keep handlers, feature services, repositories, Unit of Work, and EF Core DbContext scoped so the request and its Mediator dispatch share the correct request scope and can safely use the same DbContext.
- Never resolve scoped services from a singleton. Singleton services must not capture a handler, feature service, repository, Unit of Work, or DbContext.
- A source-generator lifetime mismatch is a registration/build configuration problem, not a reason to change the intended lifetime. Fix package placement, generator discovery, assembly configuration, generated registration, or build wiring so that the generated Mediator registration and runtime options agree on Scoped.
- Ensure the project that invokes `AddMediator` is correctly configured for the Mediator source generator, or move the registration call into the project where the generator is available while preserving the intended composition and assembly scanning. Verify the generated output reflects Scoped and includes the Application handlers. Do not add a second conflicting registration.

## Adding a feature

1. Create the feature's command/query and handler in Application, following the existing naming and folder conventions.
2. Add or reuse the feature service and its abstractions; put business rules in the service.
3. Use the generic repository abstraction in the service for persistence operations.
4. Add one feature controller that dispatches all commands and queries for that feature through Mediator.
5. Return the shared `ResponseBase<T>` envelope from the application flow and map it to HTTP at the controller boundary.
6. Register new services with Scoped lifetime where they depend on request-scoped data access.
7. Add focused tests for service business rules, handler delegation, and controller/API mapping where appropriate. Report only validation actually run.
