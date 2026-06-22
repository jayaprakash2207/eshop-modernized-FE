# Generation Sequence

This file turns the M6 prompt into an incremental execution order for this workspace.

## Completed in this execution

1. Created a separate `generated-system/` folder.
2. Established the workspace structure requested by the M6 prompt.
3. Chose the first safe task from the authoritative evidence.
4. Recorded assumptions and confidence.
5. Defined the target architecture baseline for future generation.

## Recommended next tasks

1. Generate backend solution structure for the modular monolith:
   - domain, application, infrastructure, api, shared building blocks
2. Generate preserved API contract specifications for Wave 0 endpoints:
   - `/api/authenticate`
   - `/api/catalog-brands`
   - `/api/catalog-items`
   - `/api/catalog-items/{catalogItemId}`
   - `/api/catalog-types`
3. Generate database ownership plan and schema-per-context migration blueprint.
4. Generate React route map from the preserved UI and API capabilities.
5. Generate contract tests for preserved endpoints before service extraction.

## Deferred tasks

These are intentionally deferred because the evidence marks them as risky:

- direct extraction of `Catalog`
- direct extraction of `Identity`
- direct extraction of `Basket`
- direct extraction of `Order`
- separate `Admin` service extraction
- full microservice split of shared `DataAccess`

## Exit criteria for moving beyond the baseline

The workspace can move from modular monolith to service extraction when:

- preserved API contracts have tests
- `EfRepository` has been decomposed
- the module dependency cycle is broken
- entity ownership is explicit per bounded context
- open boundary questions have been resolved
