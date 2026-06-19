-- Database-per-service (see docs/architecture.md and ADR-0006): each
-- service owns its own database on this one shared Postgres server.
-- POSTGRES_DB (docker-compose) creates "cobrabridge_accounts" for
-- AccountsService automatically; this script creates the second database,
-- for CustomersService. Runs once, only when the postgres-data volume is
-- first initialized.
CREATE DATABASE cobrabridge_customers;
