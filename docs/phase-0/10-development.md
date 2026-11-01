# Development setup and verification

## Verified baseline

.NET SDK **10.0.402**, Node **24.22.2** and npm **11.19.1** are isolated under `/tmp/virtual-stem-toolchain`. Official archive checksums are pinned and verified by `tools/bootstrap_toolchain.py`. System installations and the user's home directory are unchanged. Rebootstrap after `/tmp` is cleared.

Both scientific-interface and backend projects build with zero warnings/errors using the selected SDK. The empty Arabic RTL React/TypeScript portal passes clean lockfile installation and production build using selected Node 24. Backend smoke checks verify Development `/health` = 200, Production `/health` = 404, and unimplemented application routes = 404.

Unity project source now comes from the verified official URP template, with editor **6000.3.27f2** pinned. Editor import, Windows build, Arabic rendering and SQLite/IL2CPP spikes remain unverified. See [the concrete Unity handoff](12-unity-validation.md). No solver or playable laboratory is implemented.

## Reproduce builds

Python 3.12+, network access and Linux x64 are required:

```sh
python3 tools/bootstrap_toolchain.py
python3 tools/dev.py dotnet build src/Scientific.Core/Scientific.Core.csproj --configuration Release --nologo
python3 tools/dev.py dotnet build src/Backend/Backend.csproj --configuration Release --nologo
python3 tools/dev.py npm --prefix src/Portal ci --ignore-scripts --no-audit --no-fund
python3 tools/dev.py npm --prefix src/Portal run build
python3 tools/check_environment.py
python3 tools/smoke_backend.py
```

Use bootstrap `--offline` when verified archives remain in `/tmp`. `tools/dev.py` sets PATH and writable .NET runtime/cache and npm cache locations. NuGet/npm restore and loopback checks may need authorized execution outside the sandbox. The environment checker intentionally returns 1 until the selected Unity editor is available. Editor availability alone does not satisfy import/build acceptance.

The health endpoint reports scaffold status only. No authentication, tenant routes, database or student data is wired into the backend.

## Artifact and API checks

```sh
python3 -m unittest discover -s tests/phase0 -v
python3 -m venv --without-pip /tmp/stem-contract-venv
python3 -m pip --python /tmp/stem-contract-venv/bin/python install -r tests/contracts/requirements.txt --cache-dir /tmp/stem-pip-cache
/tmp/stem-contract-venv/bin/python -m unittest discover -s tests/contracts -v
```

The first suite has 17 checks: original hashes, RTM links, local constraints, analytical/workbook arithmetic, Unity supplier-source consistency and bundled probe assets/licenses/schema. The second has 8 checks, including full OpenAPI validation and rejection of learner grade/tenant/actor fields. These validate contracts, not implemented authentication or business logic. `python3 tools/build_api_contract.py` deterministically regenerates the API design.

Standalone native SQLite probes run with `python3 tools/run_native_sqlite_spike.py` and `python3 tools/run_native_sqlite_spike.py --windows`. Each passed 12 assertions, including abrupt-process-exit recovery. The Windows diagnostic deliberately uses installed SDK 8.0.427 in its own scoped project; this does not change the backend SDK selection. The shared source also builds against .NET Standard 2.1. See [NativeSqlite](../../spikes/NativeSqlite/README.md) and [the Unity diagnostic](12-unity-validation.md).

## PostgreSQL architecture spike

PostgreSQL 16.15 Ubuntu packages were downloaded/extracted under `/tmp`; no service was installed. On Ubuntu 24.04 x64, download `postgresql-16`, `postgresql-client-16` and `libpq5`, each version `16.15-0ubuntu0.24.04.1`, using `apt-get download` in `/tmp/stem-postgres-packages`. Extract each `.deb` with `dpkg-deb -x <archive> /tmp/stem-postgres-root`.

From the project root run `python3 tools/run_postgres_spike.py`. It creates a private temporary database and Unix socket, runs synthetic two-tenant checks, then stops/removes the database. Eight assertion groups passed; log: `artifacts/postgres-spike.log`. SQL is a limited architecture spike, not production migrations. Draft CI repeats its assertions; remote CI has not run.

## Source control and checkpoints

The six supplied documents remain unchanged. Protected empty `.git` remains untouched. A usable local repository has separate metadata at `artifacts/source-control.git`; checkpoint **4cb233d** preserves earlier work and original sources. Use:

```sh
bash tools/git.sh status --short
bash tools/git.sh log --oneline
bash tools/git.sh diff
```

Ordinary Git cannot discover this metadata because protected `.git` prevents a normal pointer file. No remote is configured. Preserve the metadata directory or export a Git bundle before clearing `artifacts`; it contains local history. A future unrestricted checkout can use standard Git metadata.

## Conventions and pipeline

Scientific.Core has no Unity/rendering/network dependency. UnityClient owns assets/presentation; Backend and Portal reserve server/admin modules. Specs/tests are versioned separately. Reject invalid/nonfinite quantities and unsupported models; preserve scientific state on rejection. Never infer measurements from scene color or position.

Draft CI checks artifacts, full API contracts, .NET builds, the locked portal and PostgreSQL isolation. Unity CI awaits licensed editor setup. Later phases add implemented solver, authorization, synchronization, installation and performance tests, signing and dependency/license scans. No remote workflow execution is claimed.
