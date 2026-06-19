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
| 2 | The bridge: COBOL → JSON over HTTP | ✅ done |
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

## Run the bridge

The bridge parses the legacy fixed-width account master and serves it as
JSON. It needs the .NET 8 SDK locally, or just Docker.

```bash
# locally
dotnet run --project src/CobraBridge.Bridge

# or as part of the full stack (legacy-core + bridge)
docker compose up legacy-core bridge
```

By default it reads `legacy-core/data/ACCOUNTS.DAT` from the repo root;
override the location with the `Legacy:AccountsFile` configuration key
(appsettings.json, `--Legacy:AccountsFile=<path>`, or the
`Legacy__AccountsFile` env var — already set in docker-compose to point at
the shared `legacy-data` volume).

```bash
curl http://localhost:5080/health
curl http://localhost:5080/accounts
curl http://localhost:5080/accounts/ACCT000002
```

Swagger UI is available at `http://localhost:5080/swagger` when running in
the `Development` environment (the default for `dotnet run`).

```bash
dotnet test src/CobraBridge.sln
```

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
