# Verification record

Date: 2026-11-01. Linux/WSL build shell; target-school Windows runtime remains unvalidated.

| Check | Observed result | Boundary |
|---|---|---|
| `python3 -m unittest discover -s tests/phase0 -v` | **15 passed** | Design/source/fixtures/SQLite/Unity-source consistency |
| `/tmp/stem-contract-venv/bin/python -m unittest discover -s tests/contracts -v` | **8 passed** | Full OpenAPI validation, deterministic generation, request shapes and declared permissions; no server authorization |
| API inventory | 43 paths, 52 operations, 78 schemas | Design only; application routes unimplemented |
| Original manifest | Six hashes/byte sizes matched | Originals unchanged |
| RTM integrity | 57 rows; BR/FR IDs, phases and links checked | Product tests remain planned |
| Analytical fixtures | Two chemistry/two electricity expectations consistent | No scientific implementation tested |
| Workbook arithmetic | Year 1 totals/cash and phase-funding totals match caches | Not full workbook recalculation or a forecast |
| SQLite design | Ownership/FK/uniqueness/ack/rollback checks pass | Python SQLite, not Unity provider/encryption |
| SDK/Node archives | Official SHA-512/SHA-256 verified | Isolated /tmp install; system tools unchanged |
| Scientific.Core build through tools/dev.py | **SDK 10.0.402: zero warnings/errors** | Interface only |
| Backend build through tools/dev.py | **SDK 10.0.402: zero warnings/errors** | Empty host |
| Portal clean npm ci and npm run build | **Node 24.22.2/npm 11.19.1: passed** | Empty shell; no runtime BiDi/accessibility proof |
| `python3 tools/smoke_backend.py` | Development health 200; Production health 404; application route 404 in both | Loopback processes stopped; logs under artifacts |
| `python3 tools/run_postgres_spike.py` | **8 assertion groups passed, PostgreSQL 16.15** | Private Unix socket; synthetic data; database stopped/removed |
| Unity template bootstrap | Registry SHA-1 and pinned SHA-256 matched; 65 supplier files plus license/editor metadata | Supplier caches excluded; editor import unrun |
| Unity source consistency | Direct pins match supplier lock; scenes/GUIDs/quality pipelines exist; license preserved | Not editor compilation, Windows build or provider test |
| Local Git checkpoint | **4cb233d**, separate metadata under artifacts/source-control.git | Protected .git untouched; use tools/git.sh; no remote |
| Environment checker | SDK/Node pins, npm, Git/checkpoint available; exit 1 for missing Unity | Expected remaining environment gate |

PostgreSQL assertions cover: default deny without context/restricted role; own-tenant visibility; foreign insert rejection; composite parent-FK rejection; tenant-changing update rejection; event ID/sequence uniqueness; context cleared after commit; second-tenant isolation.

Earlier Windows SDK 8/Node 22 fallback builds are superseded by selected-toolchain builds. Read-only home/runtime and network/socket restrictions were resolved through writable tool/cache paths and authorized network/loopback execution. No compiler warnings or tests were disabled.

No Unity installation/activation, school-PC performance benchmark, scientific certification, pilot/customer outcome, production deployment, penetration test, signed release or remote CI run is claimed.
