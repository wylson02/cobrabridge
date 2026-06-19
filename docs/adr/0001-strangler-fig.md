# ADR-0001 — Use the Strangler Fig pattern to modernize the legacy core

**Status:** Accepted
**Date:** 2026-06-19

## Context

CobraBridge models a fictional bank whose system of record is a COBOL batch
platform operating on fixed-width files. We want to expose modern REST APIs,
event streams and a real-time dashboard without a high-risk "big bang"
rewrite — the kind of rewrite that fails in real banks.

## Decision

We adopt the **Strangler Fig** pattern. New functionality is built as
modern microservices that sit *in front of* the legacy core. Each capability
is incrementally routed through the new layer until the legacy program is no
longer called directly. The legacy core keeps running, unchanged, behind an
**anti-corruption layer** (the "bridge") that translates between fixed-width
mainframe records and clean modern domain models.

Migration progress is itself a first-class, visualized metric on the
dashboard (which capabilities still hit COBOL vs. which are fully modern).

## Consequences

- **Positive:** low-risk, incremental, demonstrable at every step; mirrors how
  modernization is actually done in regulated finance; the migration story is
  visible and sellable to a recruiter.
- **Positive:** the legacy core stays authoritative for data until a capability
  is fully migrated — no dual-write correctness nightmares early on.
- **Negative:** we maintain two paradigms simultaneously for the project's life
  (that is the point, but it is real complexity).
- **Negative:** the anti-corruption layer is non-trivial and must be tested
  carefully — it is where subtle data-translation bugs live.
