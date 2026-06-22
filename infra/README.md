# Infrastructure Generation Plan

## Platform target

- containers with `Docker`
- orchestration with `Kubernetes`
- provisioning with `Terraform`
- telemetry with `OpenTelemetry`

## First-stage deployment model

The first deployment target should support a modular monolith and later service extraction without redesigning the platform foundation.

## Planned infrastructure areas

```text
infra/
  docker/
  k8s/
  terraform/
  observability/
  security/
```

## Shared platform dependencies

- PostgreSQL 16
- Redis
- RabbitMQ
- Prometheus
- Grafana
- Jaeger
- centralized structured logging

## Security posture

- OAuth2/OIDC at the edge
- JWT validation for APIs
- RBAC policies for admin and user access
- externalized secrets only
- mTLS once the system moves to multi-service deployment
