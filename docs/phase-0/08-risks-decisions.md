# Risk register and owner decisions

All owners below are proposed roles, not assigned people. Review at each phase gate.

| ID | Risk / unresolved decision | Impact / likelihood | Mitigation and exit evidence | Owner / needed by |
|---|---|---|---|---|
| R01 | Unity editor missing; SDK/Node builds and separate-metadata Git now verified | High / actual | Licensed Unity import and empty Windows build; preserve separate Git metadata before artifact cleanup | Engineering / Phase 0 |
| R02 | Arabic native text rendering and mixed BiDi unproven | High / high | Bundle reviewed font; test equations, signed values, focus, shaping and input; decide browser with license/cost evidence | UX + engineering / Phase 0 |
| R03 | Native SQLite works in standalone hosts; Unity IL2CPP and encryption remain unknown | High / medium | Windows/Linux interop and abrupt-exit checks passed; execute prepared Unity diagnostic and choose protected-key/provider policy | Engineering / Phase 0 |
| R04 | No school hardware baseline | High / actual | Owner supplies or selects actual CPU/GPU/RAM/Windows model; measure empty project before Phase 1 budget | Product + school IT / Phase 0 |
| R05 | No scientific reviewer, dataset or certified tolerances | High / actual | Named reviewer, equation references, licensed source data and signed fixture review | Scientific lead / Phase 0 |
| R06 | Broad content demands exceed team capacity | High / high | Two initial experiments only; qualified authors; per-pack validation cost | Product / Phase 0 |
| R07 | Offline seats oversubscribe or cannot be instantly revoked | High / high | Bounded preallocated tokens, explicit grace/clock rollback/revocation terms | Product + security / Phase 2 design approval |
| R08 | Client grades can be altered | High / high | Server event validation, provisional offline scores, controlled high-stakes policy, audited overrides | Security + teaching / Phase 2 |
| R09 | Cross-tenant leak / shared-machine PII exposure | Critical / medium | Composite FK, RLS and object authorization negative tests; protected per-profile storage and logout | Security / Phase 2 |
| R10 | Residency, retention and student-data terms undefined | High / actual | Privacy/legal review with institution, minimal collection, retention/export/deletion policy | Product/privacy / before real data |
| R11 | Pricing, demand and costs unvalidated | High / actual | Institution interviews, pilot evidence, quotations, funding signoff; remove deferred home assumption from separate MVP scenario | Commercial/finance / before spend |
| R12 | Native plugins/assets lack licenses or performance budget | High / medium | Avoid copied dialogue dependencies; track licenses, target hardware tests | Technical art/engineering / Phase 1 |
| R13 | LMS scope assumes Moodle includes LRS | Medium / high | Contract integration scope; test against actual LMS/LRS separately | Integrations / Phase 2 |
| R14 | Sync partial failure loses or duplicates attempts | High / medium | Durable transaction/outbox, ID deduplication, changed-body conflict, restart/partial-ack tests | Backend / Phase 2 |
| R15 | Unverified supply chain/release/backup | High / medium | Locked dependencies, vulnerability review, content/install signing, key rotation and restore drill | Operations / Phase 4 |

Decisions requiring owner input before Phase 0 acceptance: initial grades/curriculum and scientific lead; target Windows hardware; native UI vs browser based on spike; precise toolchain pins and reproducible Unity import; offline license assumptions; initial institution-only scope and budget; hosting/privacy decision owners. Safe proposals are documented; none is represented as owner-approved.

Exact offline grace days, contract wording, hosting vendor and payment provider are deliberately unresolved. They do not block writing architecture, but affected implementation must wait for a decision. No provider installation, purchase, message, publishing or production access is performed by this package.
