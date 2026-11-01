# API design

`specs/api/openapi.json` is the validated initial OpenAPI 3.1 design: 43 paths, 52 operations and 78 schemas spanning classroom, attempts, licensing, content, commercial records and administration. It is a contract, not a working backend. The scaffold's development `/health` does not prove database/authentication readiness.

All application routes use `/api/v1`, bearer access tokens, explicit membership authorization, tenant/class filtering, cursor pagination and UTC ISO-8601 timestamps. Tenant is selected from verified membership; never trust a tenant header or arbitrary body field. UUID resource IDs are not authorization. Assignment listing requires teacher authorization for a selected class, or returns only the authenticated student's assignments. Attempt creation is allowed only for the assigned learner/device/content version. Reject unknown JSON command types and unsupported versions.

| Surface planned for Phase 2 | Representative operations | Required permission |
|---|---|---|
| Membership/classroom | members, classes, enrollments; assignments CRUD | Teacher own classes; institution admin tenant scope |
| Content | catalog/version manifest GET; drafts/reviews/releases POST | Learner entitled catalog; author/reviewer/publisher separate |
| Attempts | create, read own, event batches; teacher review | Learner own attempts; authorized class teacher |
| Assessment | validate/recompute and reasoned override | Server evaluator; audited authorized teacher override |
| Licensing | device registration, activate, lease/release, renewal | IT/institution admin; learners limited to current session |
| Commercial | plans, subscriptions, invoices, payments, refunds | Authorized commercial operator |
| Reporting/LMS | tenant/class reports, exports, launch/grade adapters | Authorized teacher/admin; contracted connector |
| Privacy/audit | data export/deletion request, audit query | Restricted institutional/privacy operator |

The deterministic generator is `tools/build_api_contract.py`; baseline shared schemas live in `specs/api/core-schemas.json`. All operations declare permission metadata, typed bodies/results and applicable retry/revision headers. Full OpenAPI validation and eight contract tests pass. Fine-grained edits, full rubric/plan administration and actual LTI/xAPI protocol contracts still need refinement before affected Phase 2 implementation. Declared permission metadata is not enforcement.

Use `Idempotency-Key` on retryable POST operations with persisted scope, request hash and original response; retrying the same key with a different body is 409. Retention must exceed supported offline/retry windows. ETag/If-Match protects editable teacher/admin records. Errors use problem-style JSON with status, stable error code and correlation ID; never leak learner records, SQL or credentials. 400 malformed input; 401 missing/invalid identity; 403 insufficient permission; 404 inaccessible/absent resource per consistent non-disclosure policy; 409 revision/idempotency conflict; 413 oversized batch; 422 unsupported scientific/schema data; 429 rate limit with retry guidance.

Bulk ingest returns 200 with individual acknowledgements for a valid envelope, including per-event rejection. Erroring one event cannot silently acknowledge later events that depend on it. Final grades are computed server-side from verified events; the API has no `setStudentFinalScore` route. API and JSON schema compatibility are versioned separately from experiment/model versions.

Completed design checks: full OpenAPI validation, generator drift, parameter coverage, required permissions, client grade/actor/tenant rejection, typed command boundaries, event-batch limits and teacher-override reasons. Still required with implementation: generated-client typecheck, real tenant/class authorization, replay/idempotency conflicts, sequence recovery, pagination and transport/body limits. Schema rejection does not establish end-to-end authorization.

Standards references: [OpenAPI 3.1.1](https://spec.openapis.org/oas/v3.1.1.html). Runtime behavior remains specified for future implementation.
