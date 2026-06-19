# Legacy Core — the "mainframe"

This is the deliberately old-fashioned half of CobraBridge: a small batch
banking system written in **COBOL** (GnuCOBOL), operating on **fixed-width
flat files** the way a 1980s core-banking platform would. Everything modern
in this repository exists to *strangle* this layer — to put a contemporary
face on it without rewriting it in one big risky leap.

## What's here

| Path | Role |
|------|------|
| `cobol/ACCTLIST.cbl` | Daily account batch: accrues interest, writes a report, updates balances |
| `cobol/copybooks/ACCOUNT.cpy` | The 80-byte fixed-width account record layout |
| `data/ACCOUNTS.DAT` | Sample account master (byte-exact fixed width) |
| `scripts/run-batch.sh` | Compile + run locally without Docker |
| `Dockerfile` | Builds the `cobc` toolchain and compiles the programs |

## The record layout

The account master is positional. Field offsets are a contract — changing
them requires migrating `ACCOUNTS.DAT`.

```
Pos   Len  Field         Picture        Notes
1     10   ACCT-ID       X(10)
11    30   ACCT-NAME     X(30)
41     2   ACCT-TYPE     X(02)          CH=checking, SV=savings
43    11   ACCT-BALANCE  9(09)V99       implied 2-decimal, in cents
54     1   ACCT-STATUS   X(01)          A=active, C=closed, F=frozen
55    26   FILLER        X(26)
```

## Run it

Locally (needs `gnucobol`):

```bash
./scripts/run-batch.sh
```

Or in Docker:

```bash
docker build -t cobrabridge-legacy .
docker run --rm cobrabridge-legacy
```

Expected: the batch reports five active accounts, accrues one day of
interest on the three active savings accounts, and prints total assets
under management.

## Why real COBOL?

See [ADR-0002](../docs/adr/0002-gnucobol-legacy-core.md). Short version:
the whole point of the project is the *bridge*. A bridge over a simulated
river is a bridge over nothing.
