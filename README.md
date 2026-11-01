# Virtual STEM Laboratory

Phase 0 discovery package, dated 2026-11-01. No simulator is implemented yet.

Start with [the audit](docs/phase-0/01-audit.md), [architecture](docs/phase-0/03-architecture.md), and [phase report](docs/phase-0/11-phase-report.md).

Original files in the project root are preserved. `docs/phase-0/source-manifest.json` records their hashes. Requirements in those files are project evidence; proposed decisions remain subject to owner review.

Run the checks available without third-party dependencies:

```sh
python3 -m unittest discover -s tests/phase0 -v
python3 tools/check_environment.py
```

The first command checks design artifacts and the SQLite design, not scientific implementations. The second exits unsuccessfully until required development tools are installed. See [development setup](docs/phase-0/10-development.md).

Current evidence: **17 artifact checks, 8 API-contract tests and 8 PostgreSQL assertion groups passed**. Native SQLite diagnostics passed 12 assertions each on Windows and Linux. Scientific-interface/backend builds pass with pinned .NET 10.0.402; the portal builds with pinned Node 24.22.2. Unity Arabic/SQLite diagnostic source is prepared, but editor import and player execution remain unverified. The owner selected Saudi secondary school as the proposed curriculum baseline.

A local recoverable repository uses separate metadata because the environment protects `.git`. Run `bash tools/git.sh status` or `bash tools/git.sh log --oneline`; preserve `artifacts/source-control.git` before clearing artifacts. No remote is configured.

Phase 1 is not authorized by this package. Phase 0 has outstanding environment and owner decisions; see the report for evidence.
