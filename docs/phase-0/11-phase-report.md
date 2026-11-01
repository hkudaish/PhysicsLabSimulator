# Phase completion report

**Phase:** 0 — Discovery, requirements and architecture. Date: 2026-11-01.

**Current status:** Passed for discovery/architecture/scaffolding; Godot selected and Phase 1 prototype work authorized by the owner's continuation after preview review. See [the current gate decision](14-gate-decision.md). Scientific certification, representative school hardware and production acceptance remain unverified. The assessment below preserves the earlier pre-decision review record; its pending engine-selection items are superseded by the gate decision.

**Scope completed:** Six-file audit/preservation; gap/conflict analysis; 58-row RTM; architecture/scientific boundaries; cloud ERD/local SQLite and PostgreSQL isolation spike; 43-path/52-operation API design with 78 schemas; RTL text wireframes; risks/backlog/roadmap; actual selected-toolchain builds; official Unity template source/editor pin; local Git history. Owner confirmed Saudi secondary-school chemistry and introductory DC electricity as the proposed planning baseline. Precise curricular units remain unmapped.

**Deliverables produced:** Documents 01–12 and verification record; RTM/manifest; OpenAPI generator/validation; SQLite/PostgreSQL design SQL; analytical fixtures; scientific-interface/backend/portal scaffolds; Unity source/build entry point with supplier provenance/license; checksum-pinned tool bootstrap, development wrapper and smoke tools; draft CI.

**Files created or modified:** docs/phase-0, specs, src, tests, tools, .github/workflows and root setup files. The six originals are unchanged. Initial separate-metadata checkpoint **4cb233d** preserves original sources and earlier work; `bash tools/git.sh log --oneline` shows current history. No remote publication occurred.

**Scientific tests:** Four analytical cases checked for arithmetic consistency. No scientific solver is implemented or certified. Conservation/topology/fault/replay tests are specified for Phase 1; independent scientific signoff is absent.

**Integration tests:** **17 artifact tests and 8 API-contract tests passed.** Full OpenAPI validation passed. PostgreSQL 16.15 passed **8 assertion groups** for RLS, tenant foreign keys, update isolation, event uniqueness and transaction-local context. Native SQLite probes passed **12 assertions per platform** on Linux SQLite 3.45.1/.NET 10.0.13 and Windows winsqlite3 3.53.4/.NET 8.0.33, including Unicode paths, bound Arabic JSON, rollback and abrupt child-process-exit recovery. Shared interop code compiles against .NET Standard 2.1/C# 9. Existing selected-SDK backend/interface and Node builds passed. These results do not establish actual authentication, server grading, sync or Unity/IL2CPP integration.

**Performance results:** No laboratory or target-Windows measurements. School hardware is pending; FPS, latency and hour-long stability targets remain provisional.

**Security results:** PostgreSQL tests use synthetic tenants and a non-superuser/non-BYPASSRLS role. API schemas reject learner-supplied grade/actor/tenant fields. Download checksums and original hashes verified. Actual server authorization, signing/encryption, key protection, dependency and penetration tests remain unimplemented/unrun.

**Known defects / limits:** No Unity editor was found in standard folders or registered Windows installations; portable locations remain possible. Supplier lock and diagnostic UI/editor scripts remain unverified in Unity. Backend/portal remain empty scaffolds. Native SQLite interop is a diagnostic, not encrypted production persistence. Font glyph coverage passes with a bundled fallback for Tajawal's missing subscript ₂; actual shaping, BiDi, editing and font import remain unverified. PostgreSQL is a design spike, not production migrations. API edit/admin/LMS refinements remain before Phase 2.

**Unfinished requirements:** Licensed Unity import/Windows build; native Arabic shaping/BiDi/input spike; Unity SQLite/IL2CPP/provider/recovery/protection decision; school hardware baseline; scientific reviewer/fixture tolerances; exact curriculum mapping; owner architecture/MVP review. No external approvals or market evidence are fabricated.

**Alternative-engine update:** After authorization to evaluate Godot, the candidate passed 14 Linux headless checks, 16 Windows editor-runtime checks and 17 exported Windows checks, with 10 native SQLite assertions inside each engine runtime. It loads Scientific.Core, uses bundled fonts without system fallback, preserves Arabic/LTR cluster order, edits logical text, renders simple lit 3D geometry and runs from its own exported folder without source/editor mode. The source, captured viewport, licenses and Windows review ZIP are concrete deliverables. This is a compatibility experiment, not either scientific lesson. See [13-godot-evaluation.md](13-godot-evaluation.md).

If the owner selects Godot, replace the Unity/IL2CPP-only requirements above with the evaluated Godot C# Windows path and its remaining human/hardware/data-protection reviews. Do not require a Unity installation to accept a Godot baseline. No engine change or scientific acceptance has been silently approved.

## Phase 0 acceptance assessment

| Criterion | Evidence | Decision |
|---|---|---|
| Requirements reviewed/categorized | Source/conflict audit, 58 rows, proposed Saudi secondary baseline confirmed | Detailed review pending |
| Architecture documented | Boundaries, trust model, validated Godot alternative and scaffold builds | Owner engine-revision review pending |
| Science models defined | Assumptions/units/tolerances/reference cases | Independent scientific review pending |
| Database architecture | ERD, SQLite/PG isolation and in-engine Godot native probe | Local-data protection/provider policy review pending |
| Risks/dependencies identified | R01–R15 and concrete Unity handoff | Documented |
| MVP scoped | Two Phase 1 experiments; Phase 2 institution workflows | Owner review pending |
| Reproducible environment/builds | SDK/Node, locks, checkpoint and actual Godot Windows export verified | Candidate ready; owner engine-baseline selection pending |
| Measurable Phase 1 tasks | P1-01–11, reference cases, benchmark protocol | School hardware/reviewer pending |

**Recommended next steps:** Review the runnable Godot preview and accept/reject the proposed engine revision; select school PC specifications; assign scientific review and map curriculum units. If Unity is retained instead, its [import/build/RTL/provider checks](12-unity-validation.md) remain required. Reassess Phase 0 before Phase 1.

**Phase acceptance decision required:** Prompt.txt and V2 gates require evidence before progression. The owner's “go ahead” authorized continued Phase 0 work; it does not replace missing evidence. Phase 1 approval follows prerequisites and the gate report.

See [verification.md](verification.md) for commands and observed results.
