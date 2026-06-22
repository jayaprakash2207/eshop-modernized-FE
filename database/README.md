# Database Generation Plan

## Database baseline

The target relational store is `PostgreSQL 16`.

## Current decision

This execution does not generate final DDL because the evidence still calls for ownership clarification before extraction. Instead, it records the database generation direction:

- schema-per-bounded-context
- UUID primary keys for new forward-engineered designs unless a preserved contract requires otherwise
- audit columns on owned tables
- explicit foreign keys and indexes
- migrations generated per module

## Initial ownership map

- `catalog` schema -> `CatalogContext`
- `identity` schema -> `IdentityContext`
- `basket` schema -> `BasketContext`
- `orders` schema -> `OrderContext`
- `admin` schema only if admin owns data rather than orchestrating other domains

## Required prerequisite

The shared `EfRepository` and shared data access patterns must be decomposed before service extraction or hard data ownership claims are finalized.
