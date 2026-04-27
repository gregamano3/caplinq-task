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
