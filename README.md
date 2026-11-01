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

Phase 1 is not authorized by this package. Phase 0 has outstanding environment and owner decisions; see the report for evidence.
