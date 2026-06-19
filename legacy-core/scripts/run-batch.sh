#!/usr/bin/env bash
# Build & run the legacy batch locally (requires gnucobol installed).
# Usage:  ./scripts/run-batch.sh
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BIN="$HERE/bin"
mkdir -p "$BIN"

echo ">> compiling ACCTLIST.cbl"
cobc -x -free -I "$HERE/cobol/copybooks" -o "$BIN/acctlist" "$HERE/cobol/ACCTLIST.cbl"

echo ">> running batch in legacy-core/data"
cd "$HERE/data"
"$BIN/acctlist"

echo ">> ----- ACCTRPT.TXT -----"
cat "$HERE/data/ACCTRPT.TXT"
