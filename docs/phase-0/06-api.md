# API design

`specs/api/openapi.json` is the initial OpenAPI 3.1 contract for attempt creation, event sync and assignment listing. It is a draft, not a working backend. `/health` in the scaffold is development-only readiness plumbing and does not prove database or authentication readiness.

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

Only three core routes are formalized in the initial machine-readable draft. Remaining routes need detailed schemas, errors and authorization contracts before their Phase 2 implementation; they are backlog items, not completed contracts.

Use `Idempotency-Key` on retryable POST operations with persisted scope, request hash and original response; retrying the same key with a different body is 409. Retention must exceed supported offline/retry windows. ETag/If-Match protects editable teacher/admin records. Errors use problem-style JSON with status, stable error code and correlation ID; never leak learner records, SQL or credentials. 400 malformed input; 401 missing/invalid identity; 403 insufficient permission; 404 inaccessible/absent resource per consistent non-disclosure policy; 409 revision/idempotency conflict; 413 oversized batch; 422 unsupported scientific/schema data; 429 rate limit with retry guidance.

Bulk ingest returns 200 with individual acknowledgements for a valid envelope, including per-event rejection. Erroring one event cannot silently acknowledge later events that depend on it. Final grades are computed server-side from verified events; the API has no `setStudentFinalScore` route. API and JSON schema compatibility are versioned separately from experiment/model versions.

Contract tests needed: OpenAPI validator, generated client typecheck, unauthorized tenant/class access, actor spoofing, duplicate and changed-body retries, sequence gaps, pagination and body limits. Phase 0's stdlib checks resolve local schema references but are not a complete OpenAPI conformance validator.
