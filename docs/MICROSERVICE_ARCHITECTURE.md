# Microservice Architecture — Loyalty Capability

Overview

This document shows where the Loyalty capability sits in the microservice topology and how it communicates with existing services.

Deployment Options

1. In-Process (MVP): Implement Loyalty inside `PlatformApp.Api` under `PlatformApp.Application.Loyalty` and `PlatformApp.Infrastructure.Loyalty`. Pros: faster delivery, shared DbContext. Cons: slower extraction later.

2. Extracted Microservice: `platform-app-loyalty` service with its own repository, database schema, and API. Communicates via:
   - Synchronous internal HTTP for controller endpoints
   - Asynchronous events for integration with Order (publish/subscribe via RabbitMQ)

Service Boundaries

- LoyaltyService (Application): Core business rules, calculate points, enforce tiers, redeem logic.
- LoyaltyApi (PublicApi): Exposes customer and admin endpoints.
- LoyaltyRepository (Infrastructure): Persists accounts, transactions, tiers, and rules.
- BackgroundWorker: scheduled expiry of points.

Inter-service communication

- Incoming: `OrderPlaced` event from Order service (contains order id, buyer id, total)
- Outgoing: `PointsEarned` and `PointsRedeemed` events for downstream analytics and notification services
- Identity: queries `user_profiles` by `user_id` for display; internal linkage via user id

Operational concerns

- Idempotency on `OrderPlaced` processing: store `source_event_id` in `loyalty_transactions` to deduplicate
- Consistency: eventual consistency accepted for points; however redeem operations are strongly consistent
- Scaling: read-heavy queries for balance and history; add read-optimized views or caching as needed

Security

- All APIs protected by JWT; admin endpoints require `Admin` role.

