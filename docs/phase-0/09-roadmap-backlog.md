# Milestone roadmap and implementation backlog

Durations are source planning ranges, not delivery promises. Staffing, scientific review and school access are unconfirmed. Phase transitions require completed evidence and explicit owner signoff. Current stage: Phase 0 in progress; no Phase 1 implementation.

| Phase | Scope | Exit evidence | Source duration / funding hypothesis |
|---|---|---|---|
| 0 | Audit, architecture, contracts, RTL/provider spikes, reproducible setup | Reviewed specs, pinned builds, named dependencies, precise Phase 1 acceptance | 3–4 weeks; SAR 30k–75k |
| 1 | Windows 3D proof: dilution + Ohm's law; Arabic UI; save/resume | Executable and install/run evidence, scientific tests, hardware benchmark, no blockers | 4–6 weeks; SAR 90k–175k |
| 2 | Expanded lessons, accounts, teacher/admin, entitlements, sync, content/assessment | Licensed school assigns → runs offline → syncs → reviews/exports | 10–16 weeks; SAR 280k–650k |
| 3 | Actual school pilot; scientific/usability/privacy/performance remediation | Measured metrics with protocol, defects resolved, release candidate | 6–8 weeks; SAR 45k–120k |
| 4 | Signed production release, hosting/billing records/support/restore | Institution purchases/activates, deploys, operates and renews with support | Duration TBD; SAR 75k–200k |
| 5 | Optional science, curricula, platforms, VR/AI | Separately approved business case and validation | Deferred; no budget commitment |

## Remaining Phase 0 tasks

| ID | Task | Dependencies | Acceptance |
|---|---|---|---|
| P0-01 | Review supplied-file audit and conflicts | Owner | Each proposed resolution approved or assigned |
| P0-02 | Finish Unity environment baseline | Licensed Unity and Windows machine | SDK/Node builds and separate-metadata Git checkpoint complete; Unity source pinned; real import/build pending |
| P0-03 | Native Arabic UI spike | Unity/font/license | Arabic shaping + mixed equation + keyboard focus verified in Windows build |
| P0-04 | SQLite/IL2CPP spike | Unity/SQLite provider | Transactional save/restart/rollback on Windows; encryption/provider decision |
| P0-05 | Scientific review and school baseline | Named reviewer, school IT | Signed model/fixture tolerances; actual hardware selected |
| P0-06 | Review expanded API baseline | Owner/security review | 52 operations/78 schemas pass full validation; selected Node 24 clean install/build passed; detailed Phase 2 refinements tracked |
| P0-07 | Owner Phase 0 gate review | P0-01–06 | Accepted architecture/MVP boundaries; reproducible environment; explicit Phase 1 authorization |

## Precise Phase 1 backlog (not started)

| ID | Task | Dependency | Measurable acceptance/test |
|---|---|---|---|
| P1-01 | Core command/state/units/version framework | Phase 0 pass + approval | Rejected nonfinite/unit/revision commands leave state unchanged; no Unity dependency |
| P1-02 | Dilution transfer model | P1-01, scientific review | SCI-C01–06 pass including conservation and empty/invalid state |
| P1-03 | Resistive graph + MNA | P1-01 | SCI-E01–06 pass; KCL residual and reference tolerances met |
| P1-04 | URP laboratory scene and camera | Approved editor/hardware/assets | Select/place/rotate/zoom with mouse and keyboard; cancel invalid placement; no lost objects in 20 scripted cycles |
| P1-05 | Chemistry interaction and readings | P1-02/04 | Student transfers solution/adds water and gets calculated result; reset and undo restore valid state |
| P1-06 | Wiring and multimeter | P1-03/04 | Student closes/opens loop, changes V/R; probes show calculated signs/units and meaningful fault states |
| P1-07 | Arabic UI/English strings | P0-03, P1-05/06 | Mixed RTL/LTR human review; full keyboard navigation; no essential color-only cues |
| P1-08 | SQLite snapshot/journal | P0-04, versioned state | Close/reopen preserves tools, wires, chemistry and measurements; injected failure rolls back atomically |
| P1-09 | Scientific-to-visual integration | P1-05–08 | Same command stream at low/high visual quality yields identical state within declared tolerance |
| P1-10 | Windows build/package and validation | All above | Clean-machine start; 60-min session; >=30 FPS target on approved baseline; save/reload succeeds |
| P1-11 | Evidence and Phase 1 report | P1-10 | Build hash, hardware, logs, reviewer signoff, limitations; owner gate before Phase 2 |

Proposed benchmark protocol: record CPU/GPU/RAM, Windows/driver, editor/build/content hashes, resolution and preset. Warm up for 2 minutes; record frame-time distribution during both experiments, load time, peak/steady memory, solver latency, save/reload and one 60-minute session. Target >=30 FPS on approved minimum configuration, with proposed p95 frame <=33.3 ms, load <=10 s and command-to-reading <=100 ms. Only FPS/hour targets come from the baseline; additional budgets are proposals requiring approval. Do not claim numbers before measurement.

## Later-phase backlog

| ID | Phase | Work | Dependency / acceptance |
|---|---|---|---|
| P2-01 | 2 | pH, neutralization, reaction rate, heat, indicators; series/parallel/LED/fault lessons | Independently reviewed models and per-lesson reference tests |
| P2-02 | 2 | OIDC, tenancy, campuses/labs, roles, memberships | Cross-tenant/class negative tests and shared-machine logout |
| P2-03 | 2 | Teacher classes/assignments/progress/reports | Teacher assigns; learner completes; authorized teacher reviews/export |
| P2-04 | 2 | Plans, subscription/entitlement/device/seat allocation | Online/offline allocations respect agreed limits/expiry; no lost work on expiry |
| P2-05 | 2 | Event ingestion/reconciliation/server assessment | Retry/partial ack/restart/concurrency tests; untrusted client grade rejected |
| P2-06 | 2 | Content authoring/review/signing/versioning | Only approved signed immutable packages execute; rollback works |
| P2-07 | 2 | Guided lessons/rubrics/badges/history | Versioned scores and traceable observations; no client grade authority |
| P2-08 | 2 | Contracted LTI/xAPI/export adapters | Actual target-system conformance; no assumed bundled LRS |
| P3-01 | 3 | School recruitment/deployment and pilot protocol | Explicit owner outreach authorization; selected schools and consent/data arrangements |
| P3-02 | 3 | Learning/usability/performance/security measurements | Raw evidence, sample limits, defects and before/after measures |
| P3-03 | 3 | Pilot remediation/release candidate | No critical defects; owner commercial-readiness decision |
| P4-01 | 4 | Hosting, signed installer, monitoring, secure updates | Reproducible signed package; operational alerts and rollback |
| P4-02 | 4 | Backup/recovery/support/onboarding/manuals | Measured restore, support rehearsal and institutional deployment |
| P4-03 | 4 | Commercial contracts/invoices/renewals/payment integration | Approved terms/provider; audited operational records |
| P5-01 | 5 | Advanced science, SPICE, browser/mobile/VR, curricula, AI | Separate prioritization, scientific/governance review and approval |

Every feature completion requires relevant code, tests, documentation and explicit limitations. No PR, commit, deployment or outreach is claimed here.
