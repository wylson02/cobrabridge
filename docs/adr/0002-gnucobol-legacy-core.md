# ADR-0002 — Use real GnuCOBOL for the legacy core

**Status:** Accepted
**Date:** 2026-06-19

## Context

The legacy core could be (a) real COBOL compiled and executed, or (b) a
modern service that merely *pretends* to be a mainframe. Option (b) is faster
to build.

## Decision

Use **real COBOL**, compiled and run with **GnuCOBOL**. Programs operate on
genuine fixed-width flat files with positional record layouts, packed-style
numeric fields and the file-status idioms a real batch system uses.

## Consequences

- **Positive:** the central skill the project advertises — bridging legacy to
  modern — is demonstrated against a genuine legacy artifact, not a mock. A
  bridge over a simulated river is a bridge over nothing.
- **Positive:** forces us to handle the real friction (fixed-width parsing,
  byte alignment, EBCDIC-style assumptions, batch semantics) that the
  anti-corruption layer exists to absorb.
- **Negative:** a COBOL toolchain in the build; a smaller pool of contributors
  who can read it. Mitigated by Docker (the toolchain is fully containerized).
- **Negative:** GnuCOBOL is not a mainframe; we approximate, not emulate, IBM
  Enterprise COBOL. We document the gaps rather than hide them.
