# Database Design — Loyalty Feature

This document describes the PostgreSQL schema additions for the Loyalty capability. All tables are additive and preserve existing objects.

Schemas

- `loyalty` schema will host all tables for the Loyalty capability.

Tables

1. `loyalty.loyalty_accounts`
- Columns:
  - `id` uuid primary key
  - `user_id` uuid not null references `identity.user_profiles(id)`
  - `points_balance` bigint not null default 0
  - `tier_id` uuid null references `loyalty.membership_tiers(id)`
  - `created_at` timestamptz not null default now()
  - `updated_at` timestamptz not null default now()
- Indexes: `user_id` unique index

2. `loyalty.loyalty_transactions`
- Columns:
  - `id` uuid primary key
  - `account_id` uuid not null references `loyalty.loyalty_accounts(id)`
  - `type` varchar(20) not null check (type in ('EARN','REDEEM','EXPIRE'))
  - `points` bigint not null
  - `order_id` uuid null references `orders.orders(id)`
  - `source_event_id` varchar(128) null -- for idempotency
  - `created_at` timestamptz not null default now()
  - `expires_at` timestamptz null
- Indexes: `account_id`, `order_id`, `source_event_id` (unique where not null)

3. `loyalty.membership_tiers`
- Columns:
  - `id` uuid primary key
  - `name` varchar(32) not null
  - `min_points` bigint not null default 0
  - `max_points` bigint null
  - `created_at` timestamptz not null default now()

4. `loyalty.reward_rules`
- Columns:
  - `id` uuid primary key
  - `name` varchar(128) not null
  - `earn_multiplier` numeric(5,2) not null default 1.0 -- points per unit currency
  - `min_order_total` numeric(12,2) not null default 0.00
  - `valid_from` timestamptz null
  - `valid_to` timestamptz null
  - `is_active` boolean not null default true
  - `created_at` timestamptz not null default now()

Expiry Policy

- Expiry implemented as scheduled job: transactions with `type = 'EARN'` will have `expires_at` set by rule; expiry job will insert `EXPIRE` transactions and decrement `points_balance` accordingly.

Migration Strategy

- Add `loyalty` schema and tables using idempotent `CREATE SCHEMA IF NOT EXISTS` and `CREATE TABLE IF NOT EXISTS`.
- Populate `membership_tiers` default rows: Bronze, Silver, Gold.
- Add unique index on `loyalty_accounts(user_id)`.

