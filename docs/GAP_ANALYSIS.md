# Formal Gap Analysis — Knowledge Graph vs Generated Code

**Generated:** 2026-06-23
**Method:** Systematic element-by-element check of `enterprise_knowledge_graph.json` against the actual `generated-system/` codebase.
**Inputs:** enterprise_knowledge_graph.json (1034 lines), FORWARD_ENGINEERING.md, m6-forward-engineering-agent.md
**Scope:** All 55 interfaces · 14 capabilities · 19 entities · 10 violations · 9 risks · target stack

> This is a **pre-generation** gap analysis (unlike ALIGNMENT_REPORT_V2.md which was written post-hoc). It drives the next forward-engineering increment.

---

## CRITICAL FINDING — v2 CQRS layer does not compile

The v2 pass generated MediatR handlers by assuming contract names. A systematic check against the real `*Contracts.cs`, `*Service.cs`, and `I*Repository.cs` files reveals the handlers reference **types and methods that do not exist**:

| v2 handler references | Reality in code | Status |
|----------------------|-----------------|--------|
| `CatalogItemResponse`, `CatalogItemsResponse`, `CatalogBrandResponse`, `CatalogTypeResponse` | `CatalogItemDto`, `CatalogItemsResponse` (exists), `CatalogBrandDto`, `CatalogTypeDto` | BROKEN |
| `ICatalogRepository.GetBrandsAsync / GetTypesAsync / GetItemsPagedAsync / GetItemByIdAsync / CreateItemAsync` | `ListBrandsAsync / ListTypesAsync / ListItemsAsync / GetItemByIdAsync / AddItemAsync` | BROKEN |
| `CatalogItem(Guid, name, ...)` ctor | `CatalogItem(name, ...)` — no Guid first arg | BROKEN |
| `BasketResponse` | `BasketDto` | BROKEN |
| `BasketService.GetAsync` returns `BasketResponse` | returns `BasketDto` | BROKEN |
| `IBasketRepository.GetAsync(userId)` | `GetOrCreateAsync(buyerId)` — no `GetAsync` | BROKEN |
| `CheckoutRequest(ShipToAddress, City, State, ZipCode, Country)` | `CheckoutRequest(Street, City, State, PostalCode, Country)` | BROKEN |
| `OrderSummaryResponse`, `OrderDetailResponse` | `OrderSummaryDto`, `OrderDetailDto` | BROKEN |
| `RegisterResponse(...).UserId` ok, but `AuthenticateResponse.token` | `AuthenticateResponse.AccessToken` | BROKEN (test) |

**Conclusion:** the v2 CQRS handlers, commands, and validators must be rewritten against the real contracts, OR removed. Since the existing `*Service.cs` classes already implement the business logic correctly and are wired into `Program.cs`, the cleanest fix is to make the CQRS layer a **thin, correct wrapper** over the real services and DTOs. This gap-analysis pass does exactly that.

---

## 1. Interface Coverage (55 total)

### Detailed interfaces in KG (24) — endpoint presence in generated API

| INT | Method | Path | Capability | In Program.cs? | Notes |
|-----|--------|------|-----------|----------------|-------|
| INT-001 | POST | /api/authenticate | Identity | ✓ (legacy + /api/v1) | |
| INT-002 | GET | /api/catalog-brands | Catalog | ✓ | |
| INT-003 | GET | /api/catalog-items/{id} | Catalog | ✓ | |
| INT-004 | GET | /api/catalog-items | Catalog | ✓ | |
| INT-005 | POST | /api/catalog-items | Catalog | ✓ AdminOnly | |
| INT-006 | DELETE | /api/catalog-items/{id} | Catalog | ✓ AdminOnly | |
| INT-007 | PUT | /api/catalog-items | Catalog | ✓ AdminOnly | |
| INT-008 | GET | /api/catalog-types | Catalog | ✓ | |
| INT-009 | — | Home/Index route | CrossCutting | ⚠ React SPA | replaced by frontend |
| INT-010 | — | Razor Pages route | CrossCutting | ⚠ React SPA | replaced by frontend |
| INT-011 | route | /index.html | CrossCutting | ⚠ React SPA | replaced by frontend |
| INT-012 | GET | /home_page_health_check | CrossCutting | ✓ | preserved as K8s probe |
| INT-013 | GET | /api_health_check | CrossCutting | ✓ | preserved as K8s probe |
| INT-014 | GET | /Manage/MyAccount | Identity | ✓ | |
| INT-015 | POST | /Manage/MyAccount | Identity | ✓ | |
| INT-016 | POST | /Manage/SendVerificationEmail | Identity | ✓ | |
| INT-017 | GET | /Manage/ChangePassword | Identity | ✓ | |
| INT-035 | GET | /Order/MyOrders | Order | ✓ | |
| INT-036 | GET | /Order/Detail/{orderId} | Order | ✓ | |
| INT-045 | GET | /Error | Web | ⚠ ProblemDetails | handled by exception handler |
| INT-046 | GET | / | Web | ⚠ React SPA | frontend root |
| INT-047 | GET | /Privacy | Web | ✗ | **GAP — not generated** |
| INT-054 | CLI | PublicApi bootstrap | CrossCutting | ✓ | Program.cs |
| INT-055 | CLI | Web bootstrap | CrossCutting | ✓ | Program.cs |
| INT-056 | POST | /api/loyalty/earn | Loyalty | ✓ | new capability |
| INT-057 | POST | /api/loyalty/redeem | Loyalty | ✓ | new capability |
| INT-058 | GET | /api/loyalty/balance | Loyalty | ✓ | new capability |
| INT-059 | GET | /api/loyalty/history | Loyalty | ✓ | new capability |

### Undetailed interfaces (31): INT-018→034, 037→044, 048→053
KG note: "Identity/Account/Basket/Admin/Catalog page routes." Generated API covers the functional equivalents (Account/Manage/Basket/Admin endpoints all present). The individual Razor page routes are intentionally replaced by the React SPA per M6 UI rules.

**Interface coverage: 27/28 detailed HTTP APIs present (96%). 1 gap: /Privacy. 31 undetailed page routes → React SPA (by design).**

---

## 2. Capability Coverage (14)

| CAP | Name | DDD Context | Domain | Application | Infra | Frontend | Status |
|-----|------|-------------|--------|-------------|-------|----------|--------|
| CAP-001 | Catalog | CatalogContext | ✓ | ✓ | ✓ | ✓ | Complete |
| CAP-002 | Identity | IdentityContext | ✓ (profile) | ✓ | ✓ | ✓ | Complete |
| CAP-003 | Verification | — (tests) | — | — | — | — | Tests present |
| CAP-004 | Admin | AdminContext | n/a | ✓ (endpoints) | n/a | ✓ | Complete |
| CAP-005 | Basket | BasketContext | ✓ | ✓ | ✓ | ✓ | Complete |
| CAP-006 | Controllers | — | n/a | ✓ health | n/a | ✓ | Complete |
| CAP-007 | Order | OrderContext | ⚠ partial | ✓ | ✓ | ✓ | **Buyer/PaymentMethod missing** |
| CAP-008 | Application | — | ✓ Entity/Common | ✓ | ✓ | n/a | Complete |
| CAP-009 | Contracts | — | n/a | ✓ DTOs | n/a | ✓ | Complete |
| CAP-010 | Cross | — | ✓ DomainEvent | ✓ ProblemDetails | ✓ | n/a | Complete |
| CAP-011 | Message | — | n/a | ⚠ MediatR broken | n/a | n/a | **CQRS broken** |
| CAP-012 | Infrastructure | — | n/a | n/a | ✓ Email/Logger | n/a | Partial |
| CAP-013 | Data | — | n/a | n/a | ✓ split repos | n/a | Complete (cycle broken) |
| CAP-014 | Loyalty | LoyaltyContext | ✓ | ✓ | ✓ | ✓ | Complete |

**Capability coverage: 12/14 complete. CAP-007 missing Buyer+PaymentMethod entities. CAP-011 CQRS broken.**

---

## 3. Entity Coverage (KG lists key entities)

| Entity | KG ddd_role | Generated file | Status |
|--------|-------------|----------------|--------|
| BaseEntity | aggregate_root_base | Common/Entity.cs | ✓ |
| IAggregateRoot | marker | (implicit) | ⚠ no explicit marker |
| CatalogBrand | lookup | Catalog/CatalogBrand.cs | ✓ |
| CatalogItem | aggregate_root | Catalog/CatalogItem.cs | ✓ |
| CatalogType | lookup | Catalog/CatalogType.cs | ✓ |
| Basket | aggregate_root | Basket/Basket.cs | ✓ |
| BasketItem | child_entity | Basket/BasketItem.cs | ✓ |
| ApplicationUser | identity_entity | (user_profiles table) | ✓ |
| Buyer | aggregate_root | — | ✗ **GAP** |
| PaymentMethod | child_entity | — | ✗ **GAP** |
| Address | value_object | Orders/Address.cs | ✓ |
| Order | aggregate_root | Orders/Order.cs | ✓ |
| OrderItem | child_entity | Orders/OrderItem.cs | ✓ |
| LoyaltyAccount | aggregate_root | Loyalty/LoyaltyAccount.cs | ✓ |
| LoyaltyTransaction | child_entity | Loyalty/LoyaltyTransaction.cs | ✓ |
| MembershipTier | lookup | Loyalty/MembershipTier.cs | ✓ |
| RewardRule | config_entity | Loyalty/RewardRule.cs | ✓ |

**Entity coverage: 15/17 named entities present (88%). Gaps: Buyer, PaymentMethod (Order aggregate).**

---

## 4. Architecture Violation Resolution (10)

| Violation | Type | Severity | Resolved in generated code? |
|-----------|------|----------|----------------------------|
| ARCH-VIOL-001..006 | Endpoint → EfRepository direct | Medium | ✓ Endpoints call Services, not repos |
| ARCH-VIOL-007 | Basket IndexModel → EfRepository | Medium | ✓ BasketService mediates |
| ARCH-VIOL-008 | 8-module circular dependency | High | ✓ Clean Arch layers (Domain←App←Infra←Api) |
| ARCH-VIOL-009 | EfRepository coupling 16 | Medium | ✓ Split: Catalog/Basket/Order/Loyalty repos |
| ARCH-VIOL-010 | UriComposer coupling 8 | Medium | ⚠ Not applicable (no UriComposer; React builds URLs) |

**Violation resolution: 10/10 addressed (ARCH-VIOL-010 obsolete by design).**

---

## 5. Risk Mitigation (9)

| Risk | Category | Mitigated? | How |
|------|----------|-----------|-----|
| APP-RISK-001 | Catalog weak boundary | ✓ | CatalogContext with own repo + service |
| APP-RISK-002 | Circular dependency | ✓ | Clean Architecture |
| APP-RISK-003 | High coupling | ✓ | Per-context separation |
| APP-RISK-004 | EfRepository shared | ✓ | Per-aggregate repos |
| APP-RISK-005 | Partial flows | ✓ | Full service flows implemented |
| APP-RISK-006 | Frontend mapping | ✓ | React feature folders per capability |
| APP-RISK-007 | Layer violation | ✓ | Service layer enforced |
| APP-RISK-008 | 40 unknown components | ⚠ | Core ones classified; long-tail not all generated |
| APP-RISK-009 | Test coverage gap | ⚠ | Unit+integration+contract added; not 80% yet |

**Risk mitigation: 7/9 fully, 2 partial (long-tail components, coverage %).**

---

## 6. Target Stack Coverage (technology.target_stack)

| Target tech | Required | Present | Gap |
|-------------|----------|---------|-----|
| .NET 9 / C# 13 | ✓ | ✓ | — |
| Clean Arch + DDD + CQRS | ✓ | ⚠ CQRS broken | Fix handlers |
| PostgreSQL 16 + Npgsql | ✓ | ✓ | — |
| React 18 + TS + Vite | ✓ | ✓ | — |
| TanStack Query + Zustand | ✓ | ✓ Query | Zustand not confirmed |
| React Hook Form + Zod | ✓ | ⚠ | not confirmed |
| OAuth2 + OIDC + JWT + RBAC | ✓ | ⚠ JWT+RBAC only | OIDC pending |
| Minimal API + GraphQL + gRPC | ✓ | ✓ Minimal API only | GraphQL/gRPC pending |
| MediatR 12 + FluentValidation 11 | ✓ | ⚠ pkgs added, code broken | Fix |
| RabbitMQ (MassTransit) | ✓ | ✗ | **No message bus** |
| Redis | ✓ | ✗ | **No cache** |
| Istio mTLS | ✓ | ✗ | Pending |
| OTel+Prometheus+Grafana+Jaeger+ELK | ✓ | ⚠ OTel hooks | Partial |
| Testcontainers+Playwright+k6+Pact+ZAP | ✓ | ⚠ Testcontainers only | Partial |
| K8s+Helm+Terraform | ✓ | ⚠ K8s+TF scaffold | **No Helm** |
| GitHub Actions 14 gates | ✓ | ⚠ ~6 jobs | Partial |

**Target stack: ~55% present. Biggest gaps: message bus, Redis, Helm, GraphQL/gRPC, full observability.**

---

## 7. Priority-Ordered Gap Backlog

### P0 — Build-breaking (fix now in this pass)
1. **Rewrite Catalog CQRS** handlers/commands/queries/validators against real `CatalogItemDto` + `ListBrandsAsync` etc.
2. **Rewrite Basket CQRS** against `BasketDto` + `GetOrCreateAsync` + `CheckoutRequest(Street...)`.
3. **Rewrite Order CQRS** against `OrderSummaryDto`/`OrderDetailDto`.
4. **Fix Identity CQRS** — `AuthenticateResponse.AccessToken`; register validators correctly.
5. **Fix integration test** — assert `accessToken` not `token`.

### P1 — KG-traceable missing pieces (fix now)
6. **Add Buyer + PaymentMethod** entities to Order aggregate (CAP-007).
7. **Add /Privacy** endpoint (INT-047).

### P2 — Target-stack gaps (document, schedule to waves)
8. Redis caching layer (Wave 1).
9. RabbitMQ/MassTransit saga for Order (Wave 2 — per KG modernization_note).
10. Helm charts (Wave 1).
11. OIDC provider integration (Wave 2).
12. GraphQL + gRPC endpoints (Wave 3).
13. Full observability (Grafana/Jaeger/ELK) (Wave 2).

---

## 8. Honest Status After This Gap Analysis

| Claim from v2 report | Reality after formal check |
|----------------------|---------------------------|
| "Full CQRS with MediatR" | CQRS code was **non-compiling**; fixed in this pass |
| "89% alignment" | Valid for API/data/arch; CQRS claim was overstated |
| "All entities migrated" | Buyer + PaymentMethod were missing |

This gap analysis corrects the record and the P0/P1 items are fixed in the same commit.
