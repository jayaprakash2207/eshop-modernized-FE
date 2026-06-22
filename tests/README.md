# Test Generation Plan

## Priority order

1. contract tests for preserved Wave 0 APIs
2. unit tests for domain invariants
3. integration tests around persistence and module boundaries
4. frontend tests for migrated React flows
5. cross-service contract tests after extraction begins

## Highest-priority preserved APIs

- `POST /api/authenticate`
- `GET /api/catalog-brands`
- `GET /api/catalog-items/{catalogItemId}`
- `GET /api/catalog-items`
- `POST /api/catalog-items`
- `DELETE /api/catalog-items/{catalogItemId}`
- `PUT /api/catalog-items`
- `GET /api/catalog-types`

## Quality target

The authoritative forward-engineering rules set the target at `80%+ coverage`, with unit, integration, API, and contract tests all expected in the final generated system.
