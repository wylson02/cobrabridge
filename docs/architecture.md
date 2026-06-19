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
| 3 | Microservices behind the YARP gateway | **in progress (3a: gateway, 3b: accounts + strangler switch)** |
| 4 | Event-driven + real-time React/SignalR dashboard | planned |
| 5 | CI/CD hardening, observability, tests, polish | planned |

Phase 3a delivered the gateway itself: CobraBridge.Gateway is now the
system's single public entry point, proxying `/api/accounts*` by config-driven
YARP routes/clusters. The bridge lost its public port — it's reachable only
through the gateway now.

Phase 3b delivered the modern side of the swap and the Strangler switch
itself — see below. The shared `Account` model, `FixedWidthAccountParser`,
and the legacy-file path resolver moved out of the Bridge into
`CobraBridge.Domain` so AccountsService could reuse them instead of
duplicating them. Phases 3c–d (transactions, customers microservices) are
not built yet.

## The Strangler switch (Phase 3b)

Two services can now answer `GET /api/accounts*` with the identical JSON
contract — enums as text, balance as decimal:

```
AccountsSource = "legacy"  (default)
  client -> Gateway (YARP) -> Bridge -> parses ACCOUNTS.DAT live -> JSON

AccountsSource = "modern"
  client -> Gateway (YARP) -> AccountsService -> PostgreSQL
            (seeded once from ACCOUNTS.DAT, idempotently, on startup) -> JSON
```

The gateway holds a single YARP cluster named `accounts`. At startup it
resolves `AccountsSource` (`legacy` | `modern`) into that cluster's
destination address — either the Bridge's or AccountsService's internal
URL — via the `Services:Bridge` / `Services:AccountsService` config keys.
The route (`/api/accounts*` → cluster `accounts`, `/api` prefix stripped)
never changes; only where it points does. That's the entire mechanism —
no client code, no route, no contract change, just one setting flip.

Current state is queryable, for a future dashboard or just curl:

```bash
curl http://localhost:8090/api/_migration/status
# {"accountsSource":"legacy","activeBackend":"http://bridge:8080/","description":"..."}
```

To flip it: `ACCOUNTS_SOURCE=modern docker compose up -d gateway` (only the
gateway container needs to restart — Bridge, AccountsService, and Postgres
keep running). See [README.md](../README.md#run-the-modern-stack-and-flip-the-strangler-switch)
for the full walkthrough.

## Repository layout

```
cobrabridge/
├── README.md
├── docker-compose.yml          # orchestrates the whole system locally
├── docs/
│   ├── architecture.md         # this file
│   └── adr/                    # architecture decision records
├── legacy-core/                # the COBOL "mainframe" (Phase 1, runs today)
├── src/                              # .NET solution (gateway, services, bridge)
│   ├── CobraBridge.Domain/             # shared Account model, legacy parser, path resolver
│   ├── CobraBridge.Domain.Tests/       # parser + path resolver tests
│   ├── CobraBridge.Bridge/             # anti-corruption layer (Phase 2, done)
│   ├── CobraBridge.AccountsService/    # modern accounts API (Phase 3b, Postgres-backed)
│   ├── CobraBridge.AccountsService.Tests/ # mapper, seeder, endpoint, legacy/modern equivalence tests
│   ├── CobraBridge.Gateway/            # YARP API gateway + Strangler switch (Phase 3a/3b)
│   └── CobraBridge.Gateway.Tests/      # health, routing, and switch tests for the gateway
└── .github/workflows/ci.yml    # build + test pipeline
```
