# Verification record

Date: 2026-11-01. Environment: current Linux/WSL shell in `/mnt/d/PhysicsLabSimulator`. Windows hardware/runtime not validated.

| Command / inspection | Observed result | Meaning |
|---|---|---|
| `python3 -m unittest discover -s tests/phase0 -v` | 12 tests passed | Design/fixture/SQLite checks only |
| Original-source manifest check | Six hashes and byte sizes matched | Supplied documents preserved |
| Requirements integrity | Unique IDs, all BR/FR IDs, phases 0–5, local design links | Traceability structure valid; product tests planned |
| OpenAPI local reference/auth checks | Passed | Local refs resolve; auth and retry contracts declared; not full OpenAPI validation |
| Analytical fixture arithmetic | Two chemistry and two electrical cases passed | Expected numbers self-consistent; no C# solver executed |
| Workbook numeric check | Year 1 net revenue/contribution/result/cash and funding totals match caches | Narrow independent arithmetic verification; not full workbook recalculation |
| SQLite in-memory design tests | FK/profile isolation, uniqueness, ack receipt, atomic rollback and missing content checks passed | DDL behavior verified under Python SQLite; no Unity/provider/encryption test |
| `python3 tools/check_environment.py` | Exit 1; missing/inoperative required tools | Environment gate correctly remains open |
| `git status --short` | Fatal: not a Git repository | No checkpoint/commit exists |
| dotnet / node / Unity | Not available on PATH | C# compilation, frontend build, editor import and Windows build unrun |
| npm | Executable reports WSL 1 unsupported / cannot determine Node directory | Present wrapper is unusable; not a functional npm environment |
| Git | Version 2.43.0 available | Tool exists; workspace metadata is not a usable repository |

CI workflow was authored but not executed remotely. No performance, pilot, customer, grade-integrity, deployment or production claims are supported by these checks.

## Windows tooling follow-up

After the PATH check, read-only inspection found Windows .NET/Node installations outside PATH. Normal sandbox calls failed with WSL `UtilBindVsockAnyPort` errors; approved execution outside the sandbox succeeded. No toolchain installation was performed.

| Follow-up | Observed result | Limits |
|---|---|---|
| Windows dotnet `--list-sdks` / `--info` | SDK 8.0.427, runtime 8.0.33, win-x64 | Selected SDK 10.0.402 not installed; backend net10.0 uncompiled |
| Windows Node `--version` | v22.17.0, npm 10.9.2 | Selected Node 24.22.2 not installed |
| Disposable Scientific.Core copy with SDK 8 pin | Release build passed, 0 warnings/errors | Interface only; original SDK-10-pinned build and Unity import unverified |
| Registry version queries and local install | Exact pins and package-lock generated; lifecycle scripts disabled | No dependency security scan claimed |
| Portal `npm run build` | TypeScript check and Vite 8.3.5 production build passed | Empty shell only; no runtime/BiDi/accessibility check |
| Portal `npm ci --ignore-scripts --no-audit --no-fund` | Clean lockfile install passed; 23 packages installed | Windows Node 22.17.0; pinned Node 24 still unverified |
| Portal lockfile root dependency comparison | Matches exact package.json pins | Dependency integrity structure only; not a vulnerability scan |

The earlier PATH observations remain valid, but must not be interpreted as absence of Windows installations. See [development setup](10-development.md) for commands and remaining environment work.
