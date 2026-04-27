# Carrier Rates API (Assessment)

Web API assessment project for aggregating shipping rates from multiple carriers (FedEx, UPS, DHL) with unified response formatting, carrier management, caching, and business rule enforcement.

## Current Status
- `Domain` layer: implemented
- `Application` layer: implemented (contracts/abstractions only)
- `Infrastructure` layer: pending
- `API` layer: pending

Implementation is intentionally staged and reviewed per layer:
1. Domain
2. Application
3. Infrastructure
4. API

## Tech Stack
- .NET 8 (pinned via `global.json`)
- C#
- EF Core InMemory (planned in Infrastructure)
- Swagger/OpenAPI (planned in API)
- xUnit + mocking (planned in tests)

## Solution Structure
- `src/CarrierRates.Domain` - entities, enums, value models, constants
- `src/CarrierRates.Application` - use-case contracts, abstractions, result models
- `src/CarrierRates.Infrastructure` - pending
- `src/CarrierRates.Api` - pending
- `tests` - pending test projects

## Prerequisites
- .NET SDK 8.0.x installed

Verify:
```bash
dotnet --list-sdks
```

## Build
From repository root:
```bash
dotnet build CarrierRates.sln
```

## Notes on Design Direction
- Carrier keys:
  - Built-in seed keys (`fedex`, `ups`, `dhl`) may exist as constants for seed safety.
  - Runtime source of truth is carrier configuration in database (in-memory for assessment).
- Auth direction:
  - Minimal JWT with seeded users/roles (`Admin`, `User`) is planned.
  - Full ASP.NET Identity is intentionally out of scope to avoid over-engineering.
- Result pattern:
  - `Result<T>` for standard command/query outcomes.
  - `AggregateResult<T>` for rate aggregation partial-success behavior.

## Planned Next Step
- Implement `Infrastructure` layer:
  - EF Core InMemory context and repositories
  - JWT token service + password hasher
  - in-memory rate cache implementation
  - carrier API clients, adapters, and strategies

