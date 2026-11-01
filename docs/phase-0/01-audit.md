# Project audit and gap analysis

Date: 2026-11-01. Scope: the six supplied root files and local project inventory. No remote repository, school machines, or connected production systems were supplied.

## Evidence and authority

`Prompt.txt` supplies the requested phase sequence. BRD/PRD V2 addenda supersede conflicting earlier phase descriptions within those documents. Technical Reference V2 consolidates the intended stack. The original Arabic dialogue is historical evidence, including illustrative code, not a runnable project. Resolve unresolved product choices with the owner before implementing affected features.

Files referenced by older names in Prompt.txt are mapped to the supplied `_V2` documents and `Simulation_Technical_Project_Original.txt`; no missing older version is silently substituted. Root originals are unmodified; SHA-256 hashes are recorded in `source-manifest.json`.

Initial inventory: six documents only; no `.cs`, `.csproj`, Unity Assets/Packages/ProjectSettings, frontend package, database, executable, CI, or tests. `.git`, `.agents`, `.codex`, `.aws` are empty protected directories. Git reports no repository. Do not remove/reinitialize the protected `.git` directory. Python is available; Unity, dotnet, Node and npm are absent on PATH. Host availability does not establish Windows development readiness.

Review method: full extraction of DOCX paragraph/table text and all seven XLSX sheets including formulas and cached values; original dialogue indexed across all 6,199 lines, with targeted review of scientific, persistence, integration and security examples. This is not a line-by-line certification of every historical snippet. No sample is imported into production code.

## Gap analysis

| Requirement | Current initial status | Gap | Recommended solution | Phase |
|---|---|---|---|---|
| Repository, repeatable build | No usable repository/toolchain | No checkpoint or build evidence | Preserve hashes; scaffold; install pinned tools and initialize Git in permitted environment | 0 |
| 3D laboratory interaction | Dialogue snippets | No scenes, prefabs, assets or input integration | Unity URP scene; keyboard/mouse selection, placement, camera, undo | 1 |
| Dilution/concentration | Requirements and reaction examples | No validated mole/volume accounting | Pure C# model, reference fixtures, conservation tests | 1 |
| DC electricity | MNA described; illustrative wiring | No tested solver | Resistive DC graph + MNA, invalid topology diagnosis | 1 |
| Arabic scientific UI | Web overlay proposal | No font, BiDi or keyboard proof | Native-UI RTL spike before selecting renderer | 0/1 |
| Save/resume | Dialogue SaveLoadManager | Example saves transforms but not full solver/wire state | Transactional SQLite snapshots with model/schema versions | 1 |
| Accounts/classes/assignments | BRD/PRD only | All services missing | Tenant-aware modular monolith and React portals | 2 |
| Licensing and seats | Proposed prices and seat counts | No leases, token or policy | Separate entitlements, allocated offline seats, signed tokens | 2 |
| Offline sync and grading | Requirements and LMS examples | No durable journal, trust boundary or replay handling | Transactional outbox and server validation; provisional offline grades | 2 |
| Content governance | Proposed workflow | No review/signing pipeline | Immutable versions and independent signing keys | 2 |
| LMS | Historical WebGL/SCORM snippets | No conformance or school contract | LTI first when contracted, separately configured xAPI LRS | 2/5 |
| Scientific/privacy/security/pilot | Requirements only | No certified results, policies or real measurements | Scientific review; tenant negative tests; measured pilot | 1–4 |
| Production/commercial operation | Financial scenarios | No customers, deployments or support evidence | Pilot then signed releases and operational gates | 3/4 |

## Concrete historical sample findings

* Original lines 1210–1338: `ReactionSolver` finds matching reagent IDs and replaces the vessel identity with the first product while animating color. It does not account for limiting reagent, remaining amounts, all products or energy. This cannot establish scientific validity.
* Original lines 2400–2493: `CircuitManager` sums a single path's resistances, assumes series topology and substitutes 0.001 ohm for a short. This is not a general MNA solver and produces an arbitrary finite short-circuit current.
* Original lines 3636–3770 show an earlier, different save example with beaker amount/temperature and wire endpoint positions. Reconnecting by spatial overlap cannot reliably recover stable terminal identities; the existence of this example does not repair the later transform-only schema or prove an integrated persistence layer.
* Original lines 5841–5979: the shown `LabItemState`/`SaveLoadManager` stores object IDs, prefab names and transforms. Full chemistry quantities, circuit connectivity and model versions are not serialized by that example.
* Original lines 6142–6199: XOR variables, timing heuristics and code compression are proposed as anti-cheat techniques. They cannot make client-authored grades authoritative; do not inherit the unmeasured “99%” deterrence claim.
* Original includes repeated evolving class definitions and ellipsis placeholders. It has no dependency locks, integration harness or successful compilation evidence.

## Conflicts and proposed resolutions

| Conflict | Proposed resolution | Review status |
|---|---|---|
| Unreal/HDRP/high realism vs Windows low-spec Unity URP | Follow V2 Unity/URP baseline; benchmark modest visuals first | Proposed |
| Mandatory embedded browser vs native Arabic UI | Phase 0 font/BiDi/input spike; browser only if native fails and cost/license accepted | Unresolved spike |
| Broad initial chemistry/electronics catalog vs two-experiment proof | Dilution and resistive DC only in Phase 1; other experiments in Phase 2 | Follows V2 addenda |
| Client HMAC/XOR/obfuscation vs assessment integrity | Server-validated event replay and auditable teacher overrides; offline results provisional | Proposed |
| WebGL/SCORM-first dialogue vs Windows-first V2 | Defer browser deployment to Phase 5; contracted LMS adapter at Phase 2 earliest | Follows V2 |
| Free-market assumptions vs approved commercial terms | Prices and volumes remain hypotheses; configure only after approval | Unresolved commercial review |
| Workbook 200 home subscribers in Y1 vs deferred home product | Preserve source; create a separate institution-only scenario before funding decisions | Open |

No existing implementation quality, dependencies or runtime vulnerabilities can be measured because no application was supplied. This is distinct from a clean security or performance result.
