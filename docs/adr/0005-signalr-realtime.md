# ADR-0005 — Use SignalR for the real-time dashboard transport

**Status:** Accepted
**Date:** 2026-06-19

## Context

The dashboard must push live updates (transaction flow, balances, batch
status, migration progress) to the browser without polling.

## Decision

Use **ASP.NET Core SignalR**. A dashboard/back-end-for-frontend service
subscribes to RabbitMQ events and relays them to connected React clients over
a SignalR hub (WebSockets, with automatic fallback).

## Consequences

- **Positive:** first-party real-time push for .NET; the React client uses the
  official `@microsoft/signalr` package; graceful transport fallback built in.
- **Positive:** keeps the real-time concern in one BFF service rather than
  smeared across microservices.
- **Negative:** SignalR scale-out needs a backplane (e.g. Redis) under load;
  out of scope locally, noted for production.
