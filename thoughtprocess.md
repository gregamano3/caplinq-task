# Thought Process Notes

## 1) Carrier keys vs CRUD-configured carriers
- `CarrierKeys` exists only for initial seed convenience (`fedex`, `ups`, `dhl`) and typo safety.
- Runtime source of truth is the in-memory database (`CarrierConfig` records).
- New carriers should be introduced by:
  1. adding carrier config data via CRUD,
  2. implementing strategy + adapter for that carrier,
  3. registering the strategy in DI.
- This keeps behavior aligned with Open/Closed Principle while allowing dynamic enable/disable from data.

## 2) Why sealed records for query/response models
- Used `sealed record` for immutable, value-like models:
  - `RateQuery`, `Address`, `PackageDetails`, `Dimensions`
  - `ShippingRateResponse`, `RateOption`, `Money`
- Benefits:
  - concise syntax with value equality,
  - easier testing/assertions,
  - clear intent that these are data contracts.
- `sealed` prevents unnecessary inheritance for DTO-like shapes and keeps contracts predictable.

## 3) Why classes for entities
- Used classes for persisted entities:
  - `CarrierConfig`, `CarrierDisableRequest`, `ShipmentProcess`, `Settlement`, `AppUser`
- Reason:
  - mutable lifecycle/state transitions are expected,
  - EF Core InMemory mapping is straightforward with classes and settable properties.

## 4) Auth scope decision (assessment-appropriate)
- Requirement explicitly differentiates admin vs regular user behavior.
- Chosen approach:
  - minimal JWT-based role enforcement (`Admin`, `User`),
  - seeded in-memory users,
  - avoid full ASP.NET Identity to prevent over-engineering.
- This provides real endpoint protection while staying lightweight.

## 5) Partial success and Result pattern
- Rate aggregation should not fail entirely when one carrier fails.
- Use Result-style return models for expected outcomes:
  - success + warnings for partial results,
  - error collections for business rule violations.
- Reserve exceptions for unexpected system failures only.

## 6) Step-by-step delivery strategy
- Build in strict layers and review checkpoints:
  1. Domain
  2. Application
  3. Infrastructure
  4. API
- This reduces rework and makes requirement validation easier per stage.

## 7) Application layer decisions
- Application layer only defines contracts and use-case boundaries; it does not know EF, HTTP details, or ASP.NET controllers.
- Result modeling choices:
  - `Result` / `Result<T>` for command-style and single-object outcomes,
  - `AggregateResult<T>` for carrier aggregation where partial success is expected.
- This split avoids overloading one result type and keeps controller mapping straightforward.

## 8) Why repository abstractions are separated
- Separate interfaces by concern (`ICarrierConfigRepository`, `IShipmentProcessRepository`, etc.) instead of one generic repository.
- Reason: business rules map cleanly to domain concepts and remain readable/testable.
- `IUnitOfWork` is included to keep transaction/save boundary explicit in services.

## 9) Why `IUserContext` with JWT
- JWT validates identity/role at API boundary.
- `IUserContext` gives Application services a framework-agnostic way to read current user id/email/role.
- This avoids direct dependency on `HttpContext` in business logic.

## 10) Carrier strategy extensibility choice
- `ICarrierRateStrategy` exposes `CanHandle(carrierKey)` instead of hard-coded switch logic.
- Benefits:
  - adding a carrier does not require editing a central switch statement,
  - runtime matching stays aligned with CRUD-configured carrier keys.
- `CarrierKeys` remains useful for seed defaults only, not as runtime authority.

## 11) Infrastructure checkpoint assessment
- Current Infrastructure implementation is functional and aligned with requirements baseline:
  - repositories + unit of work,
  - minimal JWT support + seeded users,
  - caching support,
  - carrier adapters, strategies, and aggregation.
- This is good enough to proceed to API wiring.

## 12) Carrier infrastructure improvements (recommended)
- Remove repeated strategy boilerplate:
  - strategy classes currently duplicate status-code/error/deserialize patterns,
  - refactor toward shared helper/base pattern for consistency.
- Apply resilience policies:
  - add retry + timeout policies (Polly) per carrier client,
  - keep partial success behavior while surfacing carrier-specific warnings.
- Strengthen carrier config usage:
  - ensure `ApiKey` from `CarrierConfig` is applied as outbound auth header,
  - configure base URL and default headers in client setup rather than mutating per call.
- Tighten edge handling:
  - explicitly handle malformed payloads and non-JSON responses with stable error codes.
- Improve testability:
  - add focused tests for adapter mapping and strategy error branches before API expansion.

## 13) Infrastructure is the runtime center
- Yes, most of the real behavior currently lives in Infrastructure by design:
  - data access and state checks (repositories),
  - auth/token concerns,
  - carrier I/O and response adaptation,
  - orchestration/caching through concrete services.
- Application defines the "what"; Infrastructure currently defines the "how".
- This is acceptable for this assessment, but it means infra code quality directly determines reliability.

## 14) Infrastructure flow (current end-to-end path)
- Rate query flow:
  1. `RateQueryService` checks cache.
  2. cache miss triggers `CarrierRateAggregator`.
  3. aggregator loads enabled carriers from DB and resolves matching strategies.
  4. strategies call external carrier endpoints and map responses via adapters.
  5. partial success is returned with carrier-level warnings/errors.
  6. successful aggregate response is cached.
- Carrier disable flow:
  1. `CarrierManagementService` enforces role and business rules.
  2. reads enabled-carrier count + shipment/settlement blocking flags.
  3. writes carrier state and disable request audit record.
  4. persists through unit-of-work boundary.

## 15) Key tradeoffs and risks in current infra
- Tradeoff: fast delivery vs strict layering purity.
  - service implementations currently sit in Infrastructure for speed.
  - acceptable now, but long-term this can blur boundaries.
- Tradeoff: simple hashing vs production-grade password security.
  - SHA256 implementation is assessment-friendly, but PBKDF2/bcrypt would be preferred in real systems.
- Risk: per-strategy duplicated HTTP handling can diverge over time.
- Risk: client base URL/header mutation inside strategies can become error-prone under extension.
- Risk: transient upstream failures not yet protected by explicit retry/timeout policy.

## 16) Infra refinement plan before/while API wiring
- Priority 1: stabilize outbound client behavior.
  - configure per-carrier named clients with base URL + auth header from config,
  - keep strategy focused on payload conversion and response interpretation.
- Priority 2: add resilience policy.
  - retry + timeout for transient failures,
  - preserve partial-success contract.
- Priority 3: reduce duplication.
  - extract shared strategy helper for request execution and error mapping.
- Priority 4: add test seam clarity.
  - make strategy/adapters easily unit-testable with fake handlers.
- Priority 5: define stable error catalog.
  - ensure error codes/messages are consistent across all carrier paths.

## 17) Design patterns used (and why)

### Strategy Pattern
- Where used:
  - `ICarrierRateStrategy`
  - `FedExRateStrategy`, `UpsRateStrategy`, `DhlRateStrategy`
  - `CarrierRateAggregator` selects strategy by `CanHandle(carrierKey)`.
- Why:
  - each carrier has different request/response contract and failure behavior,
  - avoids `switch`/`if-else` branching explosion in one service,
  - enables adding carriers without modifying existing strategy classes (OCP-friendly).
- Current gap:
  - strategy implementations share repeated HTTP/error boilerplate.
- Improvement:
  - introduce shared strategy helper/base executor to reduce duplication.

### Adapter Pattern
- Where used:
  - `FedExAdapter`, `UpsAdapter`, `DhlAdapter`.
- Why:
  - each carrier response shape differs (`serviceOptions`, `services`, `options`),
  - adapters map carrier-specific contracts to unified `ShippingRateResponse`.
- Benefit:
  - keeps mapping logic isolated from orchestration and controller concerns.
- Improvement:
  - move currency behavior to config/policy object if multi-currency becomes needed.

### Unit of Work Pattern
- Where used:
  - `IUnitOfWork` + `UnitOfWork` wrapping `AppDbContext.SaveChangesAsync`.
  - service methods call repositories then commit once.
- Why:
  - defines explicit persistence boundary for each use case,
  - keeps services in control of when state transitions are finalized.
- Benefit:
  - easier to reason about transactional intent in business flows.
- Current constraint:
  - EF InMemory is not a true transactional store; behavior is sufficient for assessment but not equivalent to relational DB transactions.

### Repository Pattern
- Where used:
  - `ICarrierConfigRepository`, `ICarrierDisableRequestRepository`, `IShipmentProcessRepository`, `ISettlementRepository`, `IAppUserRepository`.
- Why:
  - abstracts storage details from service logic,
  - supports cleaner unit testing by mocking repository interfaces.
- Deliberate choice:
  - domain-specific repositories instead of a generic repository to keep business rule code readable.

### Dependency Injection Pattern
- Where used:
  - `DependencyInjection.AddInfrastructure(...)` registers concrete implementations for abstractions.
- Why:
  - decouples construction from usage,
  - allows swapping implementations (e.g., cache, auth, repositories) with minimal code changes.

### Result Pattern (operational pattern)
- Where used:
  - `Result`, `Result<T>`, `AggregateResult<T>` returned by services/strategies.
- Why:
  - represents expected business and integration outcomes without exception-driven flow,
  - supports partial success for multi-carrier aggregation with carrier-level errors.
- Benefit:
  - easier controller mapping and test assertions.

## 18) Pattern alignment vs assessment requirements
- Explicitly required and covered:
  - Strategy Pattern: implemented in carrier selection and execution.
  - Adapter Pattern: implemented in response normalization.
  - Open/Closed Principle: supported via new strategy + adapter + config path.
- Supportive patterns added:
  - Repository + Unit of Work for persistence consistency,
  - Result pattern for robust partial-success/error handling.
- Summary judgment:
  - current pattern usage is appropriate for mid-level assessment expectations, with main improvement area being strategy boilerplate reduction.
