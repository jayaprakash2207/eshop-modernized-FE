<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:6EE7F7,50:3B82F6,100:8B5CF6&height=220&section=header&text=eShop%20Modernized&fontSize=60&fontColor=ffffff&animation=fadeIn&fontAlignY=38&desc=Legacy%20eShopOnWeb%20%E2%86%92%20Cloud-Native%20Platform&descAlignY=58&descSize=20" width="100%"/>

<br/>

[![.NET](https://img.shields.io/badge/.NET%209-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23%2013-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![React](https://img.shields.io/badge/React%2018-61DAFB?style=for-the-badge&logo=react&logoColor=black)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript%205-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL%2016-336791?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-326CE5?style=for-the-badge&logo=kubernetes&logoColor=white)](https://kubernetes.io/)

<br/>

[![Architecture](https://img.shields.io/badge/Architecture-Clean%20%2B%20DDD%20%2B%20CQRS-blueviolet?style=flat-square)](/)
[![Alignment](https://img.shields.io/badge/Legacy%20Alignment-95%25%20Contract-brightgreen?style=flat-square)](/)
[![Quality](https://img.shields.io/badge/Quality%20Score-87%2F100-blue?style=flat-square)](/)
[![Forward%20Engineering](https://img.shields.io/badge/Forward%20Engineering-AI%20Generated-orange?style=flat-square)](/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

</div>

---

## What Is This?

This repository contains the **forward-engineered, cloud-native successor** to Microsoft's [eShopOnWeb](https://github.com/dotnet-architecture/eShopOnWeb) — a legacy monolithic ASP.NET application used as a reference sample. This system was produced by a **13-agent AI reverse + forward engineering pipeline** that:

1. Extracted the full business, data, application, and technology architecture from the legacy codebase
2. Built an **Enterprise Knowledge Graph** unifying all reverse-engineered evidence
3. Generated a target architecture following **Clean Architecture + DDD + CQRS** principles
4. Scaffolded the implementation with **PostgreSQL 16**, **React 18 + Vite**, and **ASP.NET Core 9 Minimal APIs**
5. Added a brand-new **Customer Loyalty** capability not present in the legacy system

---

## Legacy vs Modern: The Transformation

| Dimension | Legacy eShopOnWeb | This Platform |
|-----------|-------------------|---------------|
| **Architecture** | Layered Monolith | Clean Architecture + DDD |
| **API Style** | MVC Controllers | Minimal APIs (ASP.NET Core 9) |
| **Language** | C# (older) | C# 13 |
| **Frontend** | Blazor WebAssembly + Razor Pages | React 18 + TypeScript + Vite |
| **Database** | SQL Server / InMemory | PostgreSQL 16 + Dapper |
| **Auth** | Basic ASP.NET Identity | JWT + PBKDF2 Password Hashing |
| **Observability** | Minimal | OpenTelemetry + Prometheus hooks |
| **Containers** | None | Docker Compose + Kubernetes manifests |
| **New Capability** | — | Customer Loyalty Program |
| **Migration Pattern** | N/A | Strangler Fig (7 phases) |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                             │
│   React 18 + TypeScript + Vite  ──  SPA + Admin Dashboard      │
└──────────────────────────┬──────────────────────────────────────┘
                           │ REST / JSON
┌──────────────────────────▼──────────────────────────────────────┐
│                       API LAYER                                 │
│         ASP.NET Core 9  ──  Minimal API  ──  JWT Auth           │
│   ┌──────────┬──────────┬──────────┬──────────┬─────────────┐   │
│   │ Catalog  │  Basket  │  Orders  │ Identity │   Loyalty   │   │
│   └────┬─────┴────┬─────┴────┬─────┴────┬─────┴──────┬──────┘   │
└────────│──────────│──────────│──────────│────────────│──────────┘
         │          │          │          │            │
┌────────▼──────────▼──────────▼──────────▼────────────▼──────────┐
│                    APPLICATION LAYER                             │
│   CQRS Handlers  ──  Domain Services  ──  Event Dispatching      │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│                    DOMAIN LAYER (Core)                          │
│   Aggregates  ──  Value Objects  ──  Domain Events  ──  Rules   │
│   Catalog | Basket | Order | Identity | Loyalty                 │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│                 INFRASTRUCTURE LAYER                            │
│   Dapper Repositories  ──  PostgreSQL 16  ──  Event Bus         │
│   EF Migrations  ──  JWT Provider  ──  Payment Gateway          │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| Backend Framework | ASP.NET Core | 9.0 | REST API host |
| Language | C# | 13 | Business logic |
| ORM / Data Access | Dapper | 2.x | SQL performance |
| Auth | JWT + PBKDF2 | — | Stateless auth |
| Frontend | React + Vite | 18 + 5 | SPA |
| UI Language | TypeScript | 5 | Type safety |
| Database | PostgreSQL | 16 | Primary store |
| Observability | OpenTelemetry | 1.x | Traces + metrics |
| Containers | Docker + Compose | — | Local dev |
| Orchestration | Kubernetes | — | Production |
| Migration | Flyway-style SQL | — | Schema versioning |

---

## Project Structure

```
eshop-modernized-FE/
├── backend/
│   └── services/
│       └── platform-app/
│           └── src/
│               ├── PlatformApp.Api/           ← Minimal API endpoints
│               ├── PlatformApp.Application/   ← CQRS handlers, services
│               ├── PlatformApp.Domain/        ← Aggregates, value objects
│               └── PlatformApp.Infrastructure/← Dapper repos, migrations
├── frontend/
│   └── platform-ui/                           ← React 18 + TypeScript + Vite
│       ├── src/
│       │   ├── components/
│       │   ├── pages/
│       │   └── services/
│       └── package.json
├── database/
│   ├── schema.sql                             ← Full PostgreSQL DDL
│   └── migrations/                           ← Versioned SQL migrations
├── infra/
│   ├── docker-compose.yml                    ← Local dev environment
│   └── k8s/                                  ← Kubernetes manifests
├── tests/
│   └── PlatformApp.Tests/                    ← Unit + integration tests
└── docs/
    ├── ARCHITECTURE_DECISIONS.md
    ├── LEGACY_COMPARISON.md                  ← Full alignment report
    ├── IMPLEMENTATION_STATUS.md
    └── NEW_FEATURE_IMPLEMENTATION_REPORT.md  ← Loyalty capability docs
```

---

## Domain Capabilities

| # | Capability | Status | Source |
|---|-----------|--------|--------|
| 1 | Catalog Management | Implemented | Legacy + Enhanced |
| 2 | Basket / Shopping Cart | Implemented | Legacy |
| 3 | Checkout & Order Pipeline | Implemented | Legacy + Enhanced |
| 4 | Identity & Authentication | Implemented | Legacy + JWT upgrade |
| 5 | Payment Processing | Scaffolded | Legacy (in-memory) |
| 6 | Admin Dashboard | Scaffolded | Legacy |
| 7 | **Customer Loyalty Program** | **Implemented** | **New Capability** |

---

## Database Schema

```
catalog schema                    identity schema
┌──────────────────┐              ┌──────────────────────┐
│ catalog_brands   │              │ user_profiles        │
│  brand_id (PK)   │              │  user_id (PK)        │
│  name            │              │  email (UNIQUE)      │
└────────┬─────────┘              │  password_hash       │
         │                        │  role                │
┌────────▼─────────┐              └──────────────────────┘
│ catalog_items    │
│  item_id (PK)    │   basket schema
│  brand_id (FK)   │   ┌────────────────┐    ┌──────────────────┐
│  type_id (FK)    │   │ baskets        │    │ basket_items     │
│  name, price     │   │  basket_id(PK) │◄──►│  basket_id (FK)  │
│  stock_quantity  │   │  buyer_id      │    │  catalog_item_id │
└──────────────────┘   └────────────────┘    │  unit_price      │
                                             └──────────────────┘
loyalty schema
┌──────────────────────┐    ┌───────────────────────┐
│ loyalty_accounts     │    │ loyalty_transactions   │
│  account_id (PK)     │◄──►│  transaction_id (PK)  │
│  user_id (UNIQUE FK) │    │  account_id (FK)       │
│  points_balance      │    │  points_delta          │
│  tier_name           │    │  source_event_id       │
└──────────────────────┘    └───────────────────────┘
```

---

## Alignment Scores

```
Legacy Flow Contract Parity    ████████████████████  95 / 100
Overall Application Alignment  ███████████████░░░░░  75 / 100
Production Readiness (New)     █████████████████░░░  87 / 100
Forward Engineering Complete   ███████████░░░░░░░░░  55 / 100
```

> See [LEGACY_COMPARISON.md](docs/LEGACY_COMPARISON.md) for full scoring methodology and category breakdown.

---

## Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [PostgreSQL 16](https://www.postgresql.org/download/) (or use Docker)

### 1. Start the database

```bash
docker compose -f infra/docker-compose.yml up -d postgres
```

### 2. Run database migrations

```bash
psql -U postgres -d eshop -f database/schema.sql
psql -U postgres -d eshop -f database/migrations/V20260617__create_loyalty_schema.sql
```

### 3. Start the backend

```bash
cd backend/services/platform-app/src/PlatformApp.Api
dotnet run
# API available at http://localhost:5000
```

### 4. Start the frontend

```bash
cd frontend/platform-ui
npm install
npm run dev
# UI available at http://localhost:5173
```

### 5. Start everything with Docker

```bash
docker compose -f infra/docker-compose.yml up --build
```

---

## Roadmap

<details>
<summary><b>Wave 0 — Foundation (Complete)</b></summary>

- [x] Enterprise Knowledge Graph built from legacy codebase
- [x] Target architecture selected (Clean Architecture + DDD)
- [x] Modular monolith scaffold generated
- [x] Domain models for all 7 capabilities
- [x] PostgreSQL schema with full FK + index coverage
- [x] React 18 + Vite frontend scaffold
- [x] Customer Loyalty capability (new)
- [x] Docker Compose dev environment

</details>

<details>
<summary><b>Wave 1 — Core Services (In Progress)</b></summary>

- [ ] Catalog CRUD integration tests
- [ ] Basket checkout pipeline end-to-end
- [ ] Order placement → Loyalty earn points event wiring
- [ ] Identity service with refresh tokens
- [ ] Admin API completeness (all CRUD)
- [ ] PostgreSQL repositories (replace InMemory fallbacks)

</details>

<details>
<summary><b>Wave 2 — Production Hardening (Planned)</b></summary>

- [ ] OpenTelemetry exporters (Jaeger / Prometheus)
- [ ] Health checks and readiness probes
- [ ] Secret management (Azure Key Vault / HashiCorp Vault)
- [ ] Kubernetes manifests for all services
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Contract tests for all legacy API endpoints
- [ ] Load testing baseline

</details>

<details>
<summary><b>Wave 3 — Microservices Extraction (Future)</b></summary>

- [ ] Extract Loyalty as independent microservice
- [ ] Event bus (RabbitMQ / Azure Service Bus)
- [ ] API Gateway (YARP / Ocelot)
- [ ] Per-service databases
- [ ] Service mesh (Istio / Linkerd)
- [ ] Multi-tenant support

</details>

---

## How This Was Built

This codebase was produced by a **13-agent AI reverse + forward engineering pipeline** powered by Claude AI:

```
eShopOnWeb Legacy Source
        │
        ▼
┌───────────────────────────────────────────────────────────┐
│  LAYER 1 — Extraction (Python AST + symbol parsers)       │
│  310 components │ 55 interfaces │ 13 capabilities          │
└───────────────────────────────────┬───────────────────────┘
                                    │
        ┌───────────────────────────┼──────────────────┐
        ▼                           ▼                  ▼
┌───────────────┐          ┌────────────────┐  ┌──────────────────┐
│ BA Layer 2+3  │          │ DA Agents 1+2  │  │ TA Agents 1+2    │
│ (2 agents)    │          │ (2 agents)     │  │ (2 agents)       │
│ Business caps │          │ Data model     │  │ Tech stack       │
└───────────────┘          └────────────────┘  └──────────────────┘
        │                           │                  │
        └───────────────────────────▼──────────────────┘
                                    │
                    ┌───────────────▼───────────────────┐
                    │     AA Pipeline (7 agents)         │
                    │  Inventory → Parser → Evidence     │
                    │  → Final Architecture → Forward    │
                    │  → Quality Review → Audit          │
                    └───────────────┬───────────────────┘
                                    │
                    ┌───────────────▼───────────────────┐
                    │   Enterprise Knowledge Graph       │
                    │   (M2+M3+M4+M5 unified JSON)       │
                    └───────────────┬───────────────────┘
                                    │
                    ┌───────────────▼───────────────────┐
                    │   M6 Forward Engineering Agent     │
                    │   → This Repository                │
                    └───────────────────────────────────┘
```

> The pipeline produces 95% legacy contract parity with a 87/100 quality score.

---

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Follow Clean Architecture boundaries — domain layer must not reference infrastructure
4. Add tests for all new domain logic
5. Submit a PR with a description of what legacy behavior it preserves or what new capability it adds

---

## Documentation

| Document | Description |
|----------|-------------|
| [ARCHITECTURE_DECISIONS.md](docs/ARCHITECTURE_DECISIONS.md) | All ADRs — why Clean Architecture, why PostgreSQL, why Minimal APIs |
| [LEGACY_COMPARISON.md](docs/LEGACY_COMPARISON.md) | Full alignment scoring vs legacy eShopOnWeb |
| [IMPLEMENTATION_STATUS.md](docs/IMPLEMENTATION_STATUS.md) | Current progress and known gaps |
| [NEW_FEATURE_IMPLEMENTATION_REPORT.md](docs/NEW_FEATURE_IMPLEMENTATION_REPORT.md) | Customer Loyalty implementation details |
| [EXECUTION_CONTEXT.md](docs/EXECUTION_CONTEXT.md) | AI pipeline execution context and inputs |

---

<div align="center">

**Built from legacy. Designed for the future.**

[![Stars](https://img.shields.io/github/stars/jayaprakash2207/eshop-modernized-FE?style=social)](https://github.com/jayaprakash2207/eshop-modernized-FE)
[![Forks](https://img.shields.io/github/forks/jayaprakash2207/eshop-modernized-FE?style=social)](https://github.com/jayaprakash2207/eshop-modernized-FE/fork)

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:8B5CF6,50:3B82F6,100:6EE7F7&height=120&section=footer" width="100%"/>

</div>
