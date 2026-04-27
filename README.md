# Carrier Rates API (Assessment)

Web API assessment project for aggregating shipping rates from multiple carriers (FedEx, UPS, DHL) with unified response formatting, carrier management, caching, and business rule enforcement.

## Current Status
- `Domain` layer: implemented
- `Application` layer: implemented (contracts/abstractions only)
- `Infrastructure` layer: implemented
- `API` layer: implemented
- `Unit tests`: implemented (xUnit + Moq)

Implementation is intentionally staged and reviewed per layer:
1. Domain
2. Application
3. Infrastructure
4. API

## Tech Stack
- .NET 8 (pinned via `global.json`)
- C#
- EF Core InMemory
- JWT bearer auth (minimal, seeded users)
- In-memory cache (`IMemoryCache`)
- HttpClientFactory for carrier calls
- Swagger/OpenAPI (with bearer token support)
- xUnit + Moq

## Solution Structure
- `src/CarrierRates.Domain` - entities, enums, value models, constants
- `src/CarrierRates.Application` - use-case contracts, abstractions, result models
- `src/CarrierRates.Infrastructure` - EF context, repositories, auth, cache, carrier adapters/strategies, service implementations
- `src/CarrierRates.Api` - controllers, JWT auth wiring, Swagger, mock carrier endpoints
- `tests/CarrierRates.UnitTests` - service-level unit tests

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

## Test
From repository root:
```bash
dotnet test CarrierRates.sln
```

## Seeded Users
Default users seeded at startup (in-memory database):
- Admin:
  - email: `admin@test.com`
  - password: `Admin123!`
- User:
  - email: `user@test.com`
  - password: `User123!`

## Notes on Design Direction
- Carrier keys:
  - Built-in seed keys (`fedex`, `ups`, `dhl`) may exist as constants for seed safety.
  - Runtime source of truth is carrier configuration in database (in-memory for assessment).
- Auth direction:
  - Minimal JWT with seeded users/roles (`Admin`, `User`) is implemented in Infrastructure.
  - Full ASP.NET Identity is intentionally out of scope to avoid over-engineering.
- Result pattern:
  - `Result<T>` for standard command/query outcomes.
  - `AggregateResult<T>` for rate aggregation partial-success behavior.

## Planned Next Step
- Harden and refine:
  - carrier strategy resiliency (retry/timeout),
  - centralize carrier HTTP error handling,
  - improve config-driven client setup (base URL + headers),
  - expand tests (adapters, aggregator failures, controller mappings).

## Infrastructure Improvement Backlog (Carrier Module)
- Centralize repeated HTTP error handling and mapping logic across carrier strategies.
- Add retry policy (Polly) and timeout policy per carrier endpoint.
- Include carrier credential/header application from `CarrierConfig` (`ApiKey` usage).
- Move base address/headers setup into client factory configuration to avoid per-call mutation.
- Add explicit carrier integration tests for adapter mapping and strategy failure paths.

## Design Notes
- Detailed implementation reasoning and pattern decisions are documented in [`thoughtprocess.md`](./thoughtprocess.md).

