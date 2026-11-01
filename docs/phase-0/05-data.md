# Data models and synchronization contract

This is a design, not a deployed database. Executable SQLite DDL is in `specs/data/local.sql`; PostgreSQL constraints below require a real server integration test before adoption.

```mermaid
erDiagram
  TENANT ||--o{ CAMPUS : owns
  CAMPUS ||--o{ LAB : contains
  TENANT ||--o{ MEMBERSHIP : authorizes
  USER ||--o{ MEMBERSHIP : joins
  MEMBERSHIP ||--o{ ROLE_ASSIGNMENT : receives
  TENANT ||--o{ CLASS : owns
  CLASS ||--o{ ENROLLMENT : includes
  MEMBERSHIP ||--o{ ENROLLMENT : attends
  EXPERIMENT ||--o{ EXPERIMENT_VERSION : versions
  EXPERIMENT_VERSION ||--o{ SCIENTIFIC_VALIDATION : validates
  EXPERIMENT_VERSION ||--o{ ASSET_PACKAGE : packages
  EXPERIMENT_VERSION ||--o{ RUBRIC_VERSION : scores
  CURRICULUM_STANDARD }o--o{ EXPERIMENT_VERSION : maps
  CLASS ||--o{ ASSIGNMENT : receives
  EXPERIMENT_VERSION ||--o{ ASSIGNMENT : freezes
  ASSIGNMENT ||--o{ ATTEMPT : generates
  ATTEMPT ||--o{ ATTEMPT_EVENT : journals
  ATTEMPT ||--o{ MEASUREMENT : records
  ATTEMPT ||--o{ ASSESSMENT_RESULT : assesses
  RUBRIC_VERSION ||--o{ ASSESSMENT_RESULT : governs
  SYNC_BATCH ||--o{ SYNC_BATCH_EVENT : acknowledges
  ATTEMPT_EVENT ||--o{ SYNC_BATCH_EVENT : retries
  TENANT ||--o{ SUBSCRIPTION : holds
  LICENSE_PLAN ||--o{ SUBSCRIPTION : configures
  SUBSCRIPTION ||--o{ ENTITLEMENT : grants
  SUBSCRIPTION ||--o{ SEAT_LEASE : allocates
  DEVICE ||--o{ SEAT_LEASE : uses
  TENANT ||--o{ DEVICE : registers
  TENANT ||--o{ INVOICE : billed
  INVOICE ||--o{ PAYMENT : settles
  TENANT ||--o{ LMS_CONNECTION : integrates
  TENANT ||--o{ AUDIT_LOG : records
```

## PostgreSQL entity dictionary

All tenant-owned records use UUID `tenant_id`, UUID `id`, UTC `created_at/updated_at`, and integer `revision`. Every parent exposes `UNIQUE(tenant_id,id)` and every child references both fields. Cross-tenant input must fail before insertion and at the database constraint. Global catalog records are immutable published data; tenant-private drafts require separate tenant-scoped access policies. Use `timestamptz`, decimal commercial amounts and ISO currency, never floating-point money.

| Entity | Essential attributes and constraints |
|---|---|
| Tenant, Campus, Lab | Name, status; campus.tenant; lab campus composite FK |
| User, Membership, RoleAssignment | Global OIDC issuer+subject unique; tenant membership unique(user,tenant); scoped role and campus/class access |
| Class, Enrollment | Campus, teacher membership FK; unique(class,student membership); reject inactive/nonstudent enrollment |
| Experiment, ExperimentVersion | Stable catalog ID; immutable version, schema/model versions, hash, release/review state; unique(experiment,version) |
| AssetPackage, ScientificValidation | Version FK, signing key/hash/compatibility; reviewer, reference suite hash, outcome, signed_at |
| CurriculumStandard, VersionStandard | Jurisdiction, grade, unit/code; many-to-many version mapping |
| RubricVersion | Versioned scoring rules and reviewed tolerances; immutable when published |
| Assignment | Class, frozen experiment version/rubric, teacher, due_at, state |
| Attempt | Assignment, actor membership, device, client/model/schema versions, progress, revision, provisional status |
| AttemptEvent | Attempt FK, unique(tenant,event_id), unique(tenant,attempt,sequence), canonical payload hash, server receipt time |
| Measurement | Attempt/event, quantity, canonical value/unit, model revision, uncertainty |
| AssessmentResult | Attempt, rubric, computed score/status, validation evidence; overrides append audit and replacement result |
| SyncBatch, SyncBatchEvent | Request ID/body hash and per-event acknowledgement; join entity permits many retries of one event |
| LicensePlan, Subscription | Versioned plan terms; tenant, term_start/end, price/currency, status; end > start |
| Entitlement, SeatLease | Versioned content/seat limits; device allocation, issued/expiry/release, unique active allocation ID; serialized allocation transaction prevents oversubscription |
| Device | Tenant, installation public identity, status, last_seen; no shared private server key |
| Invoice, Payment | Contract reference, amounts, currency, dates, paid/refunded/recognized records separately audited |
| LMSConnection | Tenant endpoint, scoped provider IDs; secret reference in vault, not plaintext DB export |
| AuditLog | Actor, scope, action, timestamp, object and correlation ID; append-only restricted writer; redact PII |

Example intended constraint: `FOREIGN KEY (tenant_id, class_id) REFERENCES class(tenant_id,id)`. Enable/force row policies for tenant tables using transaction-bound tenant context; migration role separate from runtime role. Role mapping, policies, migration scripts and backup restoration are Phase 2 implementation, with two-tenant negative tests mandatory. Entity names alone are not isolation evidence.

## SQLite model

One local profile database per tenant/learner session; no plaintext refresh token or grade-authority secret. `profile`, `installed_content`, `snapshot`, `journal`, `outbox`, `measurement`, `license_cache`, `schema_migration`. DDL enforces content identity/version and snapshot/profile ownership. Events belong to snapshots; outbox refers to events and may be acknowledged only explicitly. Updating snapshot plus journal/outbox is one transaction. SQLite foreign keys enabled on every connection; busy timeout and WAL/provider behavior must be validated on Windows/IL2CPP. Do not block the render loop with disk I/O.

Save includes equipment transforms, stable IDs, full vessel/component/terminal/wire state, measurements, step progress, model/schema/content versions, revision and relevant action history. Snapshot stores canonical JSON, not Unity scene object pointers. Atomic migrations record checksums, preserve a backup and refuse unsupported newer schemas without truncation. Export an actionable recovery bundle on corruption, schema mismatch or sync rejection. Encryption provider and recovery-key policy are unresolved; this DDL does not encrypt records.

## Sync rules

API contract lives in `specs/api/openapi.json`. Client allocates stable attempt/event UUIDs, monotonic sequence, expected revision, payload hash and versions. Authenticate actor/device/tenant independently of submitted IDs. Server validates frozen content, permitted commands, event order and assessment rules; records acceptance and outbox effect atomically. Response per event: accepted, duplicate, missing_predecessor, conflict, unsupported_version or rejected. Keep rejected items locally with visible action needed; never silently discard.

Network timeout after acceptance is retried with unchanged IDs/body; matching duplicate returns original result. Different payload under same ID produces HTTP 409. Batch envelope errors reject the batch; valid envelopes receive explicit per-event results. Server timestamps govern deadlines where policy requires; client time is evidence only. Offline deadline allowance, maximum batch size and retention are configuration/owner decisions (proposed cap 100 events, 1 MiB per request). Immutable finalized attempts cannot be overwritten by a stale device. Tests must cover partial acknowledgement, restart, duplicates, reordered events, revoked access and simultaneous devices.
