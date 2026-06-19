# CobraBridge legacy core — the "mainframe".
# A deliberately small image whose only job is to compile and run the
# COBOL batch programs against the fixed-width account master.
FROM ubuntu:24.04

RUN apt-get update \
    && apt-get install -y --no-install-recommends gnucobol \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /legacy

# Copy sources and data
COPY cobol/ ./cobol/
COPY data/ ./data/
COPY scripts/ ./scripts/

# Compile every program at build time; binaries land in /legacy/bin
RUN mkdir -p bin \
    && cobc -x -free -I cobol/copybooks -o bin/acctlist cobol/ACCTLIST.cbl

# The batch reads/writes files in its working directory, so run from /data.
WORKDIR /legacy/data
ENV PATH="/legacy/bin:${PATH}"

# Default: run the daily account batch and print the report.
CMD ["sh", "-c", "acctlist && echo '----- ACCTRPT.TXT -----' && cat ACCTRPT.TXT"]
