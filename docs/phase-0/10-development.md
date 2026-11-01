# Development setup and verification

## What exists

Pure C# scientific-interface project (`netstandard2.1`, C# 9), ASP.NET Core empty development health host (`net10.0`), empty buildable RTL React/TypeScript portal, reserved Unity module, OpenAPI/SQLite design artifacts, reference fixtures, stdlib Python artifact checks and draft CI. No simulator, portal features, authentication or solver implementations exist. The scientific interface compiled in a disposable copy using installed Windows SDK 8.0.427; the pinned SDK 10 and backend target remain unverified.

Proposed folder conventions: `src/Scientific.Core` has no engine dependencies; `src/UnityClient` owns scene/render/input; `src/Backend` owns future server modules; `src/Portal` owns future web UI. `specs` holds versioned contracts; `tests` holds repeatable evidence; `docs/phase-0` holds review artifacts; `tools` holds setup/verification. Never place build outputs, tokens, student records or machine-specific editor files in source control.

## Run now

```sh
python3 -m unittest discover -s tests/phase0 -v
python3 tools/check_environment.py
```

Artifact tests cover original hashes, requirement IDs/links, JSON schema references, analytical fixture arithmetic, financial Year 1 arithmetic and SQLite ownership/uniqueness/acknowledgement/transaction constraints. They do not test an implemented scientific engine, renderer, encrypted local storage or sync service.

## Finish environment baseline

Install .NET SDK 10.0.402 (pinned by `global.json`), Node 24.22.2, npm, Unity Hub and Unity 6.3 LTS with Windows build support in a suitable development environment. Exact Unity patch and URP/Input System/Test Framework/package locks must be recorded after clean import. Validate compatibility before accepting pins; no SDK has been installed during this task. Sources are linked in the architecture document.

Then run:

```sh
dotnet build src/Scientific.Core/Scientific.Core.csproj --configuration Release
dotnet build src/Backend/Backend.csproj --configuration Release
dotnet run --project src/Backend/Backend.csproj --environment Development
```

The development `/health` endpoint reports scaffold status only; it must not be used as production readiness. No database, user identity or tenant access is wired up. Bind local-only during initial work.

Create/import the Universal 3D project using `src/UnityClient/README.md`, then produce an empty Windows build and run the RTL/provider spikes. The empty RTL React+TypeScript shell now has exact pins/lockfile; typecheck/build and a subsequent clean `npm ci --ignore-scripts` passed on Windows Node 22.17.0. Validate using the selected Node baseline before closing that environment criterion. Unity licensing and a target Windows test machine are external dependencies.

Windows tooling discovered at `C:\Program Files\dotnet\dotnet.exe` (SDK 8.0.427) and `C:\Program Files\nodejs\node.exe` (22.17.0) works through WSL only outside the sandbox in this session. The shell PATH tools fail independently. Do not describe these installations as missing from the machine; they do not match the selected pins.

## Source preservation and Git

The supplied six documents remain byte-for-byte unchanged (manifest hashes checked). Current `.git` is an empty protected environment directory and Git reports no repository. Do not delete it or claim a commit/checkpoint. Initialize Git in an owner-configured writable checkout, then capture these files as an initial checkpoint; do not silently relocate the project or change the protected directory. Current changes are reversible additions to a document-only workspace.

Proposed branching: protected `main`, short task branches, reviewed pull requests, phase tags only after acceptance. Build and review before merging. Commit atomic changes with requirement IDs; never commit institutional data or secrets. No remote has been configured or contacted.

## Coding and pipeline standards

Scientific code uses canonical units, finite input validation, immutable state, deterministic command sequences, explicit error types and versioned assumptions. Unity adapters may not change solver values. Async I/O and rendering communicate through data contracts, cancellation and revision checks. No undocumented sample-code copy or native plugin dependency.

The draft GitHub workflow runs Python checks, two .NET scaffold builds and a locked portal build. It has not run remotely because no repository/remote exists. Unity jobs are still missing pending real project setup. Release CI must add scientific tests, Unity edit/play-mode tests, Windows build/smoke, asset/dependency scans and signing when those components exist. Pipeline YAML alone is not successful-build evidence.
