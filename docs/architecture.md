# CobraBridge — Architecture

CobraBridge modernizes a fictional bank's COBOL core into modern .NET 8
microservices, fronted by an API gateway and observed through a real-time
React dashboard — following the **Strangler Fig** pattern (see
[ADR-0001](adr/0001-strangler-fig.md)).

## System overview

```
                          ┌───────────────────────────────┐
                          │      React Dashboard (SPA)     │
                          │   live tx flow · balances ·    │
                          │   batch status · migration %   │
                          └───────────────┬───────────────┘
                                          │ HTTPS + SignalR (WebSocket)
                          ┌───────────────▼───────────────┐
                          │        API Gateway (YARP)      │
                          │   routing · auth · tracing     │
                          └───┬───────────┬───────────┬────┘
                              │           │           │
                ┌─────────────▼──┐ ┌──────▼──────┐ ┌──▼───────────────┐
                │ Accounts svc   │ │ Transactions│ │ Customers svc    │
                │ (.NET 8)       │ │ svc (.NET 8)│ │ (.NET 8)         │
                └───────┬────────┘ └──────┬──────┘ └────────┬─────────┘
                        │                 │                 │
                        │        events (RabbitMQ /         │
                        │          MassTransit)             │
                        │   ┌─────────────┴──────────────┐  │
                        │   │  Dashboard BFF (SignalR hub)│  │
                        │   └─────────────────────────────┘  │
                        │                                     │
                ┌───────▼─────────────────────────────────────▼───────┐
                │        CobraBridge — Anti-Corruption Layer           │
                │  translates fixed-width COBOL records  <->  domain   │
                │  invokes batch programs · parses ACCOUNTS.DAT        │
                └───────────────────────┬─────────────────────────────┘
                                        │  files / process invocation
                          ┌─────────────▼─────────────────┐
                          │   Legacy Core — "mainframe"    │
                          │   GnuCOBOL batch programs       │
                          │   fixed-width flat files        │
                          └─────────────────────────────────┘

   Cross-cutting:  Docker / docker-compose · GitHub Actions CI/CD ·
                   OpenTelemetry → (Prometheus/Grafana or Seq)
```

## The bridge is the point

Everything above the legacy core exists to *strangle* it. The
**anti-corruption layer (ACL)** is the load-bearing component: it is the only
thing that understands both worlds — the positional, fixed-width,
batch-oriented COBOL world below, and the clean, event-driven, JSON world
above. Subtle data-translation bugs live here, so it gets the most testing.

## Migration as a visible metric

Each capability is tagged `LEGACY`, `BRIDGED`, or `MODERN`. The dashboard
renders the mix as a live "modernization progress" gauge. This turns the
Strangler Fig story into something a recruiter can *see* in ten seconds.

## Build phases

| Phase | Deliverable | Status |
|-------|-------------|--------|
| 0 | Foundations: repo, docs, ADRs, docker-compose skeleton, CI | **done** |
| 1 | Legacy core: GnuCOBOL batch + fixed-width data (containerized) | **done** |
| 2 | The bridge (ACL): C# service wrapping COBOL, first modern endpoint | **done** |
| 3 | Microservices behind the YARP gateway | **in progress (3a: gateway)** |
| 4 | Event-driven + real-time React/SignalR dashboard | planned |
| 5 | CI/CD hardening, observability, tests, polish | planned |

Phase 3a delivered the gateway itself: CobraBridge.Gateway is now the
system's single public entry point, proxying `/api/accounts*` to the bridge
via config-driven YARP routes/clusters. The bridge lost its public port —
it's reachable only through the gateway now. Phases 3b–d (accounts,
transactions, customers microservices behind the gateway) are not built yet.

## Repository layout

```
cobrabridge/
├── README.md
├── docker-compose.yml          # orchestrates the whole system locally
├── docs/
│   ├── architecture.md         # this file
│   └── adr/                    # architecture decision records
├── legacy-core/                # the COBOL "mainframe" (Phase 1, runs today)
├── src/                         # .NET solution (gateway, services, bridge)
│   ├── CobraBridge.Bridge/        # anti-corruption layer (Phase 2, done)
│   ├── CobraBridge.Bridge.Tests/  # unit tests for the legacy parser
│   ├── CobraBridge.Gateway/       # YARP API gateway (Phase 3a, done)
│   └── CobraBridge.Gateway.Tests/ # health + routing tests for the gateway
└── .github/workflows/ci.yml    # build + test pipeline
```
