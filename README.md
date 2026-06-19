# CobraBridge

> Modernizing a fictional bank's **COBOL** core into modern **.NET 8**
> microservices — without a big-bang rewrite — using the **Strangler Fig**
> pattern.

CobraBridge is a solo, end-to-end portfolio project. It takes a deliberately
old-fashioned banking core (real GnuCOBOL batch programs over fixed-width
flat files) and wraps it, one capability at a time, in a contemporary stack:
an anti-corruption layer, REST microservices, a YARP API gateway,
event-driven messaging, and a real-time React dashboard — all containerized,
with CI/CD and observability.

The thing being demonstrated is not "I can write a CRUD app." It's: **I can
modernize a legacy system the way regulated finance actually does it —
incrementally, safely, and visibly.**

## Why this is built the way it is

The interesting decisions are written down as ADRs, not buried in commits:

- [ADR-0001 — Strangler Fig modernization](docs/adr/0001-strangler-fig.md)
- [ADR-0002 — Real GnuCOBOL legacy core](docs/adr/0002-gnucobol-legacy-core.md)
- [ADR-0003 — YARP API gateway](docs/adr/0003-yarp-api-gateway.md)
- [ADR-0004 — RabbitMQ event-driven messaging](docs/adr/0004-rabbitmq-eventing.md)
- [ADR-0005 — SignalR real-time dashboard](docs/adr/0005-signalr-realtime.md)

Full picture: [docs/architecture.md](docs/architecture.md).

## Tech stack

| Layer | Technology |
|-------|------------|
| Legacy core | GnuCOBOL, fixed-width flat files |
| Anti-corruption layer (the bridge) | C# / .NET 8 |
| Microservices | ASP.NET Core (.NET 8) |
| API gateway | YARP |
| Messaging | RabbitMQ + MassTransit |
| Real-time | SignalR |
| Dashboard | React |
| Infra | Docker, docker-compose, GitHub Actions |
| Observability | OpenTelemetry |

## Status

| Phase | What | State |
|-------|------|-------|
| 0 | Foundations (repo, docs, ADRs, compose, CI) | ✅ done |
| 1 | Legacy core: COBOL batch + data, containerized | ✅ runs today |
| 2 | The bridge: COBOL → JSON over HTTP | 🟡 scaffolded |
| 3 | Microservices behind the gateway | ⬜ planned |
| 4 | Event-driven + real-time dashboard | ⬜ planned |
| 5 | CI/CD hardening, observability, tests | ⬜ planned |

## Run what exists today

The legacy core compiles and runs right now.

```bash
# with Docker
cd legacy-core
docker build -t cobrabridge-legacy .
docker run --rm cobrabridge-legacy

# or locally (needs gnucobol installed)
cd legacy-core
./scripts/run-batch.sh
```

You'll see the daily account batch accrue interest on active savings
accounts and print total assets under management — straight out of COBOL.

## Repository layout

```
cobrabridge/
├── docs/                 architecture + ADRs
├── legacy-core/          the COBOL "mainframe" (Phase 1)
├── src/                  .NET solution: bridge, gateway, services
├── docker-compose.yml    local orchestration
└── .github/workflows/    CI pipeline
```

---

*Fictional bank, real engineering.*
