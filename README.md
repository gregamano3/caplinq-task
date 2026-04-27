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
- Docker + Docker Compose
- Portainer (app hosting)
- Nginx Proxy Manager (reverse proxy)
- Cloudflare DNS + Tunnel

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

## Docker Run
Build and run with Docker Compose:
```bash
docker compose up -d --build
```

Stop:
```bash
docker compose down
```

App port exposed by default:
- `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`

## Seeded Users
Default users seeded at startup (in-memory database):
- Admin:
  - email: `admin@test.com`
  - password: `Admin123!`
- User:
  - email: `user@test.com`
  - password: `User123!`

## API Usage Guide
After running the app (`dotnet run` or `docker compose up -d`), open Swagger:
- `http://localhost:8080/swagger` (Docker)
- `http://localhost:5281/swagger` (local dev default)

### 1) Login and get JWT
- Use `POST /api/auth/login`
- Example request body:
```json
{
  "email": "admin@test.com",
  "password": "Admin123!"
}
```
- Copy `accessToken` from the response.

### 2) Authorize in Swagger
- Click **Authorize** (top-right).
- Enter: `Bearer <your_access_token>`
- Confirm authorization.

### 3) Query shipping rates
- Use `POST /api/rates/query`
- Example request body:
```json
{
  "origin": { "postalCode": "12345", "countryCode": "US" },
  "destination": { "postalCode": "67890", "countryCode": "US" },
  "package": {
    "weight": 5,
    "dimensions": { "length": 10, "width": 5, "height": 5 }
  }
}
```
- Expected: unified rates for enabled carriers (`FedEx`, `UPS`, `DHL`) with optional warnings.

### 4) Carrier management (admin)
- `GET /api/carriers` - list carrier configs
- `POST /api/carriers` - add carrier config
- `PUT /api/carriers/{carrierId}` - update config (including `baseUrl`)
- `DELETE /api/carriers/{carrierId}` - remove config
- `PATCH /api/carriers/{carrierId}/enable` - enable carrier
- `PATCH /api/carriers/{carrierId}/disable` - direct disable (admin only, with reason)

### 5) Disable request workflow
- As `User`:
  - `POST /api/carriers/{carrierId}/disable-requests`
- As `Admin`:
  - `PATCH /api/carriers/{carrierId}/disable-requests/{requestId}/approve`
  - `PATCH /api/carriers/{carrierId}/disable-requests/{requestId}/reject`

### 6) Mock carrier endpoints (for local assessment flow)
- `POST /api/fedex/rates`
- `POST /api/dhl/rates`
- `POST /api/ups/shipping-rates`

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

## Deployment Notes
- Intended public URL: [caplinq.gregdoesdev.xyz](https://caplinq.gregdoesdev.xyz)
- Suggested deployment chain:
  1. Run container stack via Docker/Portainer.
  2. Route inbound traffic with Nginx Proxy Manager to `caplinq-api:8080` (or host `:8080`).
  3. Point Cloudflare DNS/tunnel to NPM endpoint.

