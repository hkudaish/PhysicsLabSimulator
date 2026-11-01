# Phase completion report

**Phase:** 0 — Discovery, requirements and architecture. Date: 2026-11-01.

**Status:** Incomplete / **Blocked at the phase gate**. Tooling, checkpoints and architecture checks have advanced; Unity runtime evidence, scientific review and school hardware selection still prevent acceptance. Phase 1 has not started.

**Scope completed:** Six-file audit/preservation; gap/conflict analysis; 57-row RTM; architecture/scientific boundaries; cloud ERD/local SQLite and PostgreSQL isolation spike; 43-path/52-operation API design with 78 schemas; RTL text wireframes; risks/backlog/roadmap; actual selected-toolchain builds; official Unity template source/editor pin; local Git history. Owner confirmed Saudi secondary-school chemistry and introductory DC electricity as the proposed planning baseline. Precise curricular units remain unmapped.

**Deliverables produced:** Documents 01–12 and verification record; RTM/manifest; OpenAPI generator/validation; SQLite/PostgreSQL design SQL; analytical fixtures; scientific-interface/backend/portal scaffolds; Unity source/build entry point with supplier provenance/license; checksum-pinned tool bootstrap, development wrapper and smoke tools; draft CI.

**Files created or modified:** docs/phase-0, specs, src, tests, tools, .github/workflows and root setup files. The six originals are unchanged. Initial separate-metadata checkpoint **4cb233d** preserves original sources and earlier work; `bash tools/git.sh log --oneline` shows current history. No remote publication occurred.

**Scientific tests:** Four analytical cases checked for arithmetic consistency. No scientific solver is implemented or certified. Conservation/topology/fault/replay tests are specified for Phase 1; independent scientific signoff is absent.

**Integration tests:** **17 artifact tests and 8 API-contract tests passed.** Full OpenAPI validation passed. PostgreSQL 16.15 passed **8 assertion groups** for RLS, tenant foreign keys, update isolation, event uniqueness and transaction-local context. Native SQLite probes passed **12 assertions per platform** on Linux SQLite 3.45.1/.NET 10.0.13 and Windows winsqlite3 3.53.4/.NET 8.0.33, including Unicode paths, bound Arabic JSON, rollback and abrupt child-process-exit recovery. Shared interop code compiles against .NET Standard 2.1/C# 9. Existing selected-SDK backend/interface and Node builds passed. These results do not establish actual authentication, server grading, sync or Unity/IL2CPP integration.

**Performance results:** No laboratory or target-Windows measurements. School hardware is pending; FPS, latency and hour-long stability targets remain provisional.

**Security results:** PostgreSQL tests use synthetic tenants and a non-superuser/non-BYPASSRLS role. API schemas reject learner-supplied grade/actor/tenant fields. Download checksums and original hashes verified. Actual server authorization, signing/encryption, key protection, dependency and penetration tests remain unimplemented/unrun.

**Known defects / limits:** No Unity editor was found in standard folders or registered Windows installations; portable locations remain possible. Supplier lock and diagnostic UI/editor scripts remain unverified in Unity. Backend/portal remain empty scaffolds. Native SQLite interop is a diagnostic, not encrypted production persistence. Font glyph coverage passes with a bundled fallback for Tajawal's missing subscript ₂; actual shaping, BiDi, editing and font import remain unverified. PostgreSQL is a design spike, not production migrations. API edit/admin/LMS refinements remain before Phase 2.

**Unfinished requirements:** Licensed Unity import/Windows build; native Arabic shaping/BiDi/input spike; Unity SQLite/IL2CPP/provider/recovery/protection decision; school hardware baseline; scientific reviewer/fixture tolerances; exact curriculum mapping; owner architecture/MVP review. No external approvals or market evidence are fabricated.

## Phase 0 acceptance assessment

| Criterion | Evidence | Decision |
|---|---|---|
| Requirements reviewed/categorized | Source/conflict audit, 57 rows, proposed Saudi secondary baseline confirmed | Detailed review pending |
| Architecture documented | Boundaries, trust model, decisions, actual scaffold builds | Unity compatibility spikes pending |
| Science models defined | Assumptions/units/tolerances/reference cases | Independent scientific review pending |
| Database architecture | ERD, SQLite tests, two-tenant PostgreSQL spike | Unity provider pending |
| Risks/dependencies identified | R01–R15 and concrete Unity handoff | Documented |
| MVP scoped | Two Phase 1 experiments; Phase 2 institution workflows | Owner review pending |
| Reproducible environment/builds | Verified SDK/Node, locks/builds, checkpoint, Unity source pin | Real Unity import/Windows build required |
| Measurable Phase 1 tasks | P1-01–11, reference cases, benchmark protocol | School hardware/reviewer pending |

**Recommended next steps:** Configure licensed Unity and run [import/build/RTL/provider checks](12-unity-validation.md); select representative school PC specifications; assign scientific review and map curriculum units. Then reassess Phase 0 before Phase 1.

**Phase acceptance decision required:** Prompt.txt and V2 gates require evidence before progression. The owner's “go ahead” authorized continued Phase 0 work; it does not replace missing evidence. Phase 1 approval follows prerequisites and the gate report.

See [verification.md](verification.md) for commands and observed results.
