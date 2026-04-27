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
