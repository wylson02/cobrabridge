# ADR-0006 — Database-per-service on a shared PostgreSQL server

**Status:** Accepted
**Date:** 2026-06-19

## Context

With AccountsService (Phase 3b) and CustomersService (Phase 3c), CobraBridge
now has two independent microservices that both need PostgreSQL. They have
no business sharing data directly — AccountsService never needs a customer's
KYC status, CustomersService never needs an account balance. The only
sanctioned integration point is the gateway/HTTP boundary.

## Decision

Each service owns **its own database** — `cobrabridge_accounts`,
`cobrabridge_customers` — created on **one shared PostgreSQL server**
(one `postgres` container in docker-compose, not one container per service).
A service only ever holds a connection string to its own database; nothing
queries across databases.

This is the standard "database-per-service" microservices pattern, scoped
down for a local/demo footprint: it gets the structural isolation (each
service's schema can evolve independently, no hidden coupling through
shared tables) without paying for N separate Postgres processes on a
developer laptop.

## Consequences

- **Positive:** each service's EF Core migrations are independent — adding
  a column to `customers` can never accidentally affect `accounts`.
- **Positive:** matches how this would actually scale: moving a service's
  database to its own server later is a connection-string change, not a
  schema untangling exercise.
- **Positive:** makes the "no cross-service queries" rule structurally true
  rather than just a convention someone could quietly violate with a join.
- **Neutral:** still one Postgres *process* to operate locally — full
  process-per-service isolation (separate containers, separate failure
  domains) is a later, real-infra concern, not a local-dev one.
- **Negative:** the one-time database creation now needs an init step
  (`postgres/init/*.sql`, run by the official Postgres image on first boot)
  instead of relying on `POSTGRES_DB` alone — one extra moving part to keep
  in sync when a future service needs its own database.
