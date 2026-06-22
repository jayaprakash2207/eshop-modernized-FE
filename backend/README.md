# Backend Generation Plan

## First-stage shape

The backend should start as a modular monolith with clear internal bounded-context modules, not immediate microservices.

## Planned structure

```text
backend/
  services/
    platform-app/
      src/
        PlatformApp.Api/
        PlatformApp.Application/
        PlatformApp.Domain/
        PlatformApp.Infrastructure/
        Modules/
          Catalog/
          Identity/
          Admin/
          Basket/
          Order/
      tests/
        PlatformApp.UnitTests/
        PlatformApp.IntegrationTests/
        PlatformApp.ContractTests/
  shared/
    BuildingBlocks/
    Observability/
    Security/
  gateway/
    api-gateway/
```

## Design rules

- keep domain code free from EF Core concerns
- use CQRS handlers for commands and queries
- use per-aggregate repositories, not a shared `EfRepository`
- keep each module internally cohesive and externally small
- preserve stable public REST contracts while internal implementation changes

## Candidate module mapping

- `CatalogContext` -> `Modules/Catalog`
- `IdentityContext` -> `Modules/Identity`
- `AdminContext` -> `Modules/Admin`
- `BasketContext` -> `Modules/Basket`
- `OrderContext` -> `Modules/Order`

The other capabilities should be represented as internal supporting modules or cross-cutting libraries until ownership is clearer.
