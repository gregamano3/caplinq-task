# How We Built This App

This document describes the end-to-end process: planning, layered implementation, validation, and how AI assistants fit in. It is meant for reviewers and future contributors.

## 1. Planning first (before code)

- **Written plan:** Requirements from the assessment were turned into a structured plan in [`plan.md`](./plan.md): API shape, patterns (Strategy, Adapter, OCP), business rules for disabling carriers, caching, JWT vs header-only auth (we chose minimal JWT), Result pattern for partial success, and a phased delivery order.
- **Design rationale:** Decisions and tradeoffs (carrier keys vs CRUD, sealed records, Infrastructure as runtime center, patterns) live in [`thoughtprocess.md`](./thoughtprocess.md). That file evolved as the code grew.
- **Checkpoints:** Work was deliberately **not** done in one shot. Each layer was reviewed before the next: Domain → Application → Infrastructure → API.

## 2. Role of AI agents

- **What the AI was used for:** Scaffolding, implementing interfaces and concrete classes, Docker/Swagger wiring, tests, documentation, and iterative refactors (e.g. removing FluentAssertions, adding retry logic, `.gitignore` for build artifacts).
- **What you (the human) owned:** Approving direction (e.g. .NET 8 only, no full Identity, seed credentials, deployment stack), validating smoke runs, port conflicts, and aligning the solution with assessment criteria.
- **How to read the repo alongside this:** Treat `plan.md` and `thoughtprocess.md` as the “conversation compressed into artifacts”; this file is the narrative thread.

## 3. Step-by-step build sequence

### Phase A — Domain

1. Pin SDK with `global.json` to **.NET 8**.
2. Create solution and `CarrierRates.Domain` class library.
3. Add entities (`CarrierConfig`, `AppUser`, disable requests, shipments, settlements), enums, and value-style models (`RateQuery`, `ShippingRateResponse`).
4. Add `CarrierKeys` constants **only** for initial seed convenience; runtime truth remains DB config.

### Phase B — Application

1. Create `CarrierRates.Application` and reference Domain.
2. Add `Result` / `AggregateResult`, repository and service **interfaces**, JWT/password abstractions, `ICarrierRateStrategy`, `ICarrierRateAggregator`, and request DTOs.
3. Keep Application free of EF, HTTP, and ASP.NET specifics.

### Phase C — Infrastructure

1. Create `CarrierRates.Infrastructure` with EF Core InMemory, repositories, `UnitOfWork`, seed data, minimal JWT + password hasher, in-memory rate cache.
2. Implement **Strategy** (per carrier) + **Adapter** (map carrier JSON to unified model) + **Aggregator** (parallel calls, partial success).
3. Add **retry** on transient HTTP failures inside strategies.
4. Register everything in `DependencyInjection.AddInfrastructure`.

### Phase D — API

1. Create `CarrierRates.Api` (controllers, JWT bearer, Swagger with **Authorize** scheme).
2. Implement `HttpUserContext` from claims for `IUserContext`.
3. Controllers: auth, rates, carriers, mock carrier endpoints for local/demo.
4. Smoke test: login → authorize Swagger → query rates (fix seed `BaseUrl` vs actual port if needed).

### Phase E — Tests

1. Add `CarrierRates.UnitTests` (xUnit + Moq; **no** FluentAssertions to avoid licensing noise).
2. Cover: login, cache behavior, carrier disable rules, aggregator partial success, FedEx retry with a fake `HttpMessageHandler`.

### Phase F — DevOps and docs

1. Add `Dockerfile` and `docker-compose.yml`.
2. Update [`README.md`](./README.md): run, test, Docker, seeded users, API usage, deployment notes (Cloudflare, NPM, Portainer).
3. Remove duplicate `CarrierRates.slnx` if present; keep a single `.sln`.
4. `.gitignore` for `bin/` / `obj/` and untrack any committed build outputs.

## 4. Key files to read in order

| Order | File | Purpose |
|------|------|--------|
| 1 | `plan.md` | Original requirement breakdown and execution checklist |
| 2 | `thoughtprocess.md` | Patterns, infra focus, testing notes |
| 3 | `README.md` | Run, test, Docker, Swagger flow, seeded users |
| 4 | `BUILD_JOURNEY.md` | This narrative |

## 5. What we would improve next (optional)

- Seed carrier `BaseUrl` from configuration or the current request base URL to avoid manual updates across environments.
- Integration tests for a few HTTP endpoints.
- Stronger password hashing (e.g. PBKDF2) for non-demo deployments.
- Centralize carrier HTTP + retry in one helper to reduce duplication in strategies.

---

*This project was built iteratively with human direction and AI-assisted implementation; the artifacts above preserve the “why” as much as the “what.”*
