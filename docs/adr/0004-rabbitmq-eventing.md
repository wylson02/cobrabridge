# ADR-0004 — Use RabbitMQ for asynchronous, event-driven messaging

**Status:** Accepted
**Date:** 2026-06-19

## Context

The real-time dashboard and inter-service workflows need transaction events
to flow asynchronously (e.g. a posting event fans out to balance projection
and to the live dashboard) without tight HTTP coupling.

## Decision

Use **RabbitMQ** as the message broker, with **MassTransit** as the .NET
abstraction over it.

## Consequences

- **Positive:** lightweight to run in docker-compose, easy to reason about,
  excellent .NET tooling via MassTransit; enough to demonstrate event-driven
  architecture convincingly.
- **Negative:** not a log/stream like Kafka — no long retention or replay by
  default. If the narrative needs "enterprise streaming", revisit with Kafka.
  Documented as a conscious trade-off, not an oversight.
