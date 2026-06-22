# Target Architecture Baseline

## Selected baseline

The forward-engineering baseline for this workspace is:

`Modular monolith first, with bounded contexts prepared for later extraction.`

This follows the authoritative recommendation `Option_A_Modular_Monolith_First`.

## Target technology stack

- Frontend: `React 18`, `TypeScript 5`, `Vite 5`, `TanStack Query`
- Backend: `ASP.NET Core 9`, `C# 13`, `Clean Architecture`, `DDD`, `CQRS`, `MediatR`, `FluentValidation`
- Data: `PostgreSQL 16`
- Messaging: `RabbitMQ` with `MassTransit`
- Caching: `Redis`
- Observability: `OpenTelemetry`, `Prometheus`, `Grafana`, `Jaeger`, `ELK`
- Platform: `Docker`, `Kubernetes 1.29`, `Helm 3`, `Terraform 1.8`
- Security: `OAuth2`, `OIDC`, `JWT`, `RBAC`, `Istio mTLS`

## Bounded-context orientation

The graph identifies these named DDD contexts:

- `CatalogContext`
- `IdentityContext`
- `AdminContext`
- `BasketContext`
- `OrderContext`

The remaining capabilities should stay as internal modules until ownership and extraction readiness are clearer:

- `Verification`
- `Controllers`
- `Application`
- `Contracts`
- `Cross`
- `Message`
- `Infrastructure`
- `Data`

## Initial deployable shape

Initial deployables for the first forward-engineering stage:

1. `backend/services/platform-app`
2. `frontend/web-app`
3. `backend/gateway/api-gateway`
4. shared platform services in `infra/`

Within `platform-app`, internal modules should align to the bounded contexts above. Extraction into separate services should only happen after Wave 0 through Wave 3 constraints are resolved.

## Architecture constraints carried forward

- preserve public API behavior first
- split `EfRepository` into per-aggregate repositories before extraction
- eliminate the detected module cycle before moving high-risk capabilities out of process
- keep one data owner per bounded context
- make security, logging, health checks, and validation first-class in every generated backend slice

## Loyalty Capability Integration

The new `Customer Loyalty Management` capability is added as a bounded context (`LoyaltyContext`) and may be deployed either in-process with `platform-app` or as a separately deployed microservice.

Key integration points:
- `OrderContext`: listen for `OrderPlaced` events to award points after successful checkout.
- `IdentityContext`: link `LoyaltyAccount` to `user_profiles.user_id` for ownership and display.
- `Admin`: expose `RewardRule` configuration to admin UI and APIs.

Operational considerations:
- Implement daily expiry job for aged points — can be a hosted background service or a k8s CronJob.
- Ensure idempotency when awarding points (use transaction or event deduplication).
- Expose metrics and alerts for unusual redemption volume to prevent abuse.

Security:
- Customer endpoints require authenticated JWT; admin endpoints require `Admin` role.
- Redemption operations validate balance and lock/update pattern to prevent double spends.

Compatibility:
- No existing endpoints are modified. Points are awarded as a side-effect of order persistence.
- Schema changes are additive (new tables) and preserve existing database objects.

