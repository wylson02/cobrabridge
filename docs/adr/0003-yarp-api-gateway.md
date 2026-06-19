# ADR-0003 — Use YARP as the API gateway

**Status:** Accepted
**Date:** 2026-06-19

## Context

Clients (the React dashboard, future external consumers) need a single
entry point that fronts several .NET microservices, handles routing, and is
the natural place for cross-cutting concerns (auth, rate limiting, tracing).

## Decision

Use **YARP** (Yet Another Reverse Proxy), Microsoft's reverse-proxy library,
hosted as a dedicated ASP.NET Core gateway service.

## Consequences

- **Positive:** first-party, .NET-native, configuration- and code-driven;
  integrates cleanly with ASP.NET Core middleware, auth and OpenTelemetry.
- **Positive:** recognizable and credible to a .NET-shop reviewer.
- **Neutral:** Ocelot was the main alternative; YARP is the more actively
  developed, more flexible choice today.
- **Negative:** the gateway is a single point of failure in the local topology;
  acceptable for a portfolio, noted for production honesty.
