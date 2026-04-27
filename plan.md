# Carrier Rates Backend Developer Assessment - Implementation Plan

## Goal and Working Style
- Build a .NET 8 Web API that aggregates shipping rates from FedEx, UPS, and DHL into a unified response format.
- Keep implementation at a solid mid-level backend standard (clear layering, pragmatic abstractions, testable components).
- Do not over-engineer; prioritize correctness, maintainability, and straightforward extensibility.

## 1) Clarify Scope and Assumptions (Before Coding)
- Carrier endpoints will be mocked locally first (deterministic test/demo behavior).
- Include minimal JWT auth for requirement compliance:
  - support `Admin` and `User` roles
  - no full ASP.NET Identity setup (keep it lightweight)
  - seed in-memory users and issue JWT via simple login endpoint
- Confirm what counts as "ongoing shipment process" and "pending invoices/settlements" in in-memory data.
- Currency is fixed to `USD` for all unified responses.
- Rate results should support partial success when one/more carriers fail.

## 2) Project Bootstrap
- Create solution structure:
  - `src/CarrierRates.Api` (ASP.NET Core Web API)
  - `src/CarrierRates.Application` (services, interfaces, business rules)
  - `src/CarrierRates.Domain` (entities, enums, value objects)
  - `src/CarrierRates.Infrastructure` (carrier clients/adapters, persistence, caching)
  - `tests/CarrierRates.UnitTests`
- Add required packages:
  - Swagger/OpenAPI
  - EF Core InMemory provider
  - FluentValidation (optional but good for request validation)
  - Polly (for retry policy bonus)
  - xUnit + Moq + FluentAssertions (or NUnit equivalent)
- Configure dependency injection and environment-based config.

## 3) Domain Modeling
- Core entities/value objects:
  - `CarrierConfig` (id, name, baseUrl, credential fields, enabled, timestamps)
  - `CarrierDisableRequest` (requester, reason, status, approvedBy, approvedAt)
  - `ShipmentProcess` (carrierId, status like `AwaitingConfirmation`)
  - `Settlement` or `Invoice` (carrierId, status like `Pending`)
- Enums:
  - `DisableReason` (`Maintenance`, `ContractTermination`, `UserRequest`, `Other`)
  - `DisableRequestStatus` (`PendingApproval`, `Approved`, `Rejected`)
- Carrier identity model:
  - Use string carrier keys/constants (e.g., `fedex`, `ups`, `dhl`) instead of enum.
  - Basic comment: this keeps carrier handling config-friendly and avoids enum churn when adding providers.
- Shared rate query model:
  - Origin/Destination postal and country
  - Package weight + dimensions

## 4) API Contract Design (RESTful)
- Auth:
  - `POST /api/auth/login` (returns JWT for seeded user)
- `POST /api/rates/query`
  - Input: package + origin + destination
  - Output: list of `ShippingRateResponseDto` per carrier
- Carrier management:
  - `GET /api/carriers`
  - `POST /api/carriers`
  - `PUT /api/carriers/{id}`
  - `DELETE /api/carriers/{id}`
  - `PATCH /api/carriers/{id}/enable`
  - `PATCH /api/carriers/{id}/disable` (requires `Admin` role + reason)
  - `POST /api/carriers/{id}/disable-requests` (request disable)
  - `PATCH /api/carriers/{id}/disable-requests/{requestId}/approve` (requires `Admin` role)
  - `PATCH /api/carriers/{id}/disable-requests/{requestId}/reject` (requires `Admin` role)
- Keep DTOs aligned with provided `ShippingRateResponseDto`, `RateOption`, `MoneyDto`.
- Basic comment: secure protected endpoints with JWT bearer auth and role claims (`Admin`, `User`).

## 5) Design Patterns Plan

### Strategy Pattern
- Define `ICarrierRateStrategy` with methods like:
  - `string SupportedCarrierKey`
  - `Task<ShippingRateResponseDto> GetRatesAsync(...)`
- Implement three strategies:
  - `FedExRateStrategy`
  - `UpsRateStrategy`
  - `DhlRateStrategy`
- Use a strategy resolver/factory that loads enabled carriers from configuration store and executes matching strategies.

### Adapter Pattern
- Define adapter contracts to transform each carrier-specific response into standard DTO.
- Adapters:
  - `FedExRateAdapter`
  - `UpsRateAdapter`
  - `DhlRateAdapter`
- Keep mapping logic out of controllers/services to avoid duplication and preserve OCP.

### Open/Closed Principle
- Register strategies/adapters through DI by interface.
- Add new carrier by creating new strategy + adapter + config entry, without changing existing strategy execution flow.

## 6) External Carrier Client Layer
- Use `IHttpClientFactory` with named clients (`FedExClient`, `UpsClient`, `DhlClient`).
- For each client:
  - Base address and auth headers from `CarrierConfig`.
  - Timeout and retry policy (Polly) for transient errors (bonus requirement).
- Implement robust error handling:
  - Distinguish timeout, network, 4xx, 5xx.
  - Log structured error details and continue aggregating other carriers where possible.

## 7) Rate Aggregation Flow
- Validate incoming rate query.
- Build deterministic cache key from origin/destination/package dimensions/weight + active carrier set.
- Check in-memory cache first:
  - Cache hit -> return cached result.
  - Cache miss -> execute enabled carrier strategies in parallel (`Task.WhenAll`).
- Standardize all responses to `ShippingRateResponseDto`.
- Cache successful aggregate response with short TTL (e.g., 2-5 minutes configurable).
- Return unified response payload.

## 8) Carrier Disable/Enable Business Rules
- Before disabling a carrier, enforce:
  1. Not the only active carrier remaining.
  2. No ongoing shipment process in blocking statuses (e.g., `AwaitingConfirmation`).
  3. No pending invoice/settlement records.
  4. Reason is mandatory and persisted for audit.
  5. Only `Admin` role can directly disable.
- For `User` role:
  - Allow disable request creation.
  - Store as `PendingApproval`.
  - Disable only when approved by `Admin`.
- Add audit logging for every state change and rule rejection.
- Basic comment: `Admin/User` distinction is enforced via JWT role claims (minimal auth), without full Identity complexity.

## 9) Persistence Plan (In-Memory DB)
- Use EF Core InMemory for:
  - Users
  - Carrier configs
  - Disable requests
  - Shipment processes
  - Invoices/settlements
  - Audit logs (optional but useful)
- Seed default active carriers (FedEx, UPS, DHL) and test users (`Admin`, `User`) on startup.
- Keep repository/service boundaries simple (no heavy generic repository needed).

## 10) Validation and Error Response Standards
- Add request validation for:
  - Positive dimensions and weight
  - Required postal/country fields
  - Supported country code format
- Use consistent problem details responses (`application/problem+json`) for invalid input and rule violations.
- Reserve exceptions for unexpected/system failures; use Result pattern for expected business/validation outcomes.

## 11) Result Pattern (Recommended)
- Introduce `Result<T>` for command-style operations:
  - `bool IsSuccess`
  - `T? Value`
  - `List<Error> Errors`
  - `List<string> Warnings` (optional)
- Introduce `AggregateResult<T>` (or `QueryResult<T>`) for rate aggregation:
  - `bool IsSuccess` (true when at least one carrier returns valid rates)
  - `T Value` (aggregate payload)
  - `List<CarrierError>` per failed carrier (for partial success visibility)
  - `List<string> Warnings` for degraded responses
- Error model:
  - `Code` (e.g., `carrier.only_active`, `carrier.pending_settlement`)
  - `Message` (human-readable)
  - `Target` (optional: field/carrier key)
- Controller mapping guideline:
  - `IsSuccess=true` with warnings -> `200 OK`
  - Validation/business rule failures -> `400 BadRequest` or `409 Conflict`
  - Not found -> `404 NotFound`
  - Upstream total failure (no carrier succeeds) -> `503 ServiceUnavailable`
- Basic comment: this prevents try/catch-heavy flow and keeps tests clean/assertable.

## 12) Testing Strategy (xUnit + Moq)
- Unit tests for:
  - Rate aggregation service (success, partial failure, all failure, cache behavior)
  - Each adapter mapping correctness from carrier-specific payload to unified DTO
  - Disable/enable business rule service
  - Role-sensitive flows (`Admin` vs `User`)
  - JWT token generation/validation behavior for login flow
  - Result mapping tests (`Result` to expected HTTP responses)
- Mock external carrier calls:
  - Mock strategy/client responses to avoid real network dependency.
- Add focused business-rule tests:
  - Only active carrier cannot be disabled
  - Ongoing shipment blocks disable
  - Pending settlement blocks disable
  - Reason required and saved
  - `User`-initiated disable requires `Admin` approval

## 13) Swagger and Developer Experience
- Enable Swagger with:
  - Endpoint summaries and example payloads for rate query and carrier management.
  - Response examples for success/error.
- Add Swagger bearer token support for trying protected endpoints.
- Ensure app startup clearly exposes Swagger URL.
- Add README section:
  - prerequisites
  - run commands
  - test commands
  - seeded user credentials for demo login
  - Swagger access URL

## 14) Step-by-Step Execution Plan
1. Create solution/projects and wire references.
2. Define domain models, string carrier keys/constants, and DTO contracts.
3. Define `Result<T>`, `AggregateResult<T>`, and error code catalog.
4. Implement minimal auth domain (`User`, password hash, role claim model) and JWT configuration.
5. Build EF InMemory context, seed data (carriers + users), and basic repositories/services.
6. Implement auth endpoint (`/api/auth/login`) and JWT bearer middleware/policies.
7. Implement carrier management endpoints (CRUD + enable/disable + disable request flow).
8. Implement business rule service for disable flow and hook into endpoints using Result returns.
9. Implement `IHttpClientFactory` clients for FedEx/UPS/DHL.
10. Implement adapters for each carrier response model.
11. Implement strategy classes and strategy resolver.
12. Build rate aggregation endpoint with parallel fetch + normalization + partial-success Result.
13. Add in-memory cache and key strategy for recent rates.
14. Add retry policies and error handling middleware.
15. Add unit tests for services, adapters, business rules, auth flow, and Result-to-HTTP mapping.
16. Polish Swagger docs and README run instructions.
17. Final pass: verify requirements checklist and clean up.

## 15) Acceptance Checklist
- Login endpoint returns JWT for seeded users.
- Protected endpoints enforce role checks correctly.
- Rate query endpoint returns unified DTO format for at least 3 carriers.
- Carrier config endpoints support add/update/remove and enable/disable.
- All disable business rules are enforced and tested.
- `User` can request disable; `Admin` approval flow works.
- In-memory caching reduces repeated outbound calls for same query.
- Result pattern is used consistently for expected outcomes (including partial success).
- Strategy + Adapter patterns are clearly implemented.
- New carrier can be added with extension, not modification (OCP).
- Unit tests pass reliably with mocked carrier responses.
- Swagger is accessible and complete.
- README includes clone/run/test instructions.

## 16) Risk and Mitigation Notes
- Risk: ambiguous "ongoing shipment" definition.
  - Mitigation: model explicit blocking statuses and document assumption.
- Risk: partial carrier outages causing aggregate failure.
  - Mitigation: isolate per-carrier failures, return available rates, log failures.
- Risk: excessive abstraction for a small assignment.
  - Mitigation: keep interfaces minimal and purposeful.
- Risk: auth scope expands beyond assignment.
  - Mitigation: keep auth minimal (JWT + seeded users only), skip full Identity stack and advanced token lifecycle.
