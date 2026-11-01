-- Design spike only. No encryption, Unity provider, or production migration claim.
PRAGMA foreign_keys = ON;
CREATE TABLE schema_migration (
    version INTEGER PRIMARY KEY CHECK (version > 0),
    checksum TEXT NOT NULL,
    applied_at TEXT NOT NULL
);
CREATE TABLE profile (
    id TEXT PRIMARY KEY,
    tenant_id TEXT NOT NULL,
    learner_id TEXT NOT NULL,
    UNIQUE (tenant_id, learner_id)
);
CREATE TABLE installed_content (
    experiment_id TEXT NOT NULL,
    version TEXT NOT NULL,
    model_version TEXT NOT NULL,
    schema_version INTEGER NOT NULL CHECK (schema_version > 0),
    package_hash TEXT NOT NULL,
    signature_key_id TEXT NOT NULL,
    PRIMARY KEY (experiment_id, version)
);
CREATE TABLE snapshot (
    id TEXT PRIMARY KEY,
    profile_id TEXT NOT NULL REFERENCES profile(id),
    experiment_id TEXT NOT NULL,
    experiment_version TEXT NOT NULL,
    model_version TEXT NOT NULL,
    schema_version INTEGER NOT NULL CHECK (schema_version > 0),
    revision INTEGER NOT NULL CHECK (revision >= 0),
    state_json TEXT NOT NULL,
    saved_at TEXT NOT NULL,
    UNIQUE (profile_id, id),
    FOREIGN KEY (experiment_id, experiment_version)
        REFERENCES installed_content(experiment_id, version)
);
CREATE TABLE journal (
    event_id TEXT PRIMARY KEY,
    profile_id TEXT NOT NULL,
    snapshot_id TEXT NOT NULL,
    sequence INTEGER NOT NULL CHECK (sequence > 0),
    payload_json TEXT NOT NULL,
    payload_hash TEXT NOT NULL,
    occurred_at TEXT NOT NULL,
    UNIQUE (snapshot_id, sequence),
    FOREIGN KEY (profile_id, snapshot_id) REFERENCES snapshot(profile_id, id)
);
CREATE TABLE outbox (
    event_id TEXT PRIMARY KEY REFERENCES journal(event_id),
    status TEXT NOT NULL DEFAULT 'pending'
        CHECK (status IN ('pending', 'acknowledged', 'conflict', 'rejected')),
    retry_count INTEGER NOT NULL DEFAULT 0 CHECK (retry_count >= 0),
    next_retry_at TEXT,
    server_receipt TEXT,
    CHECK (status <> 'acknowledged' OR server_receipt IS NOT NULL)
);
CREATE TABLE measurement (
    id TEXT PRIMARY KEY,
    event_id TEXT NOT NULL REFERENCES journal(event_id),
    quantity TEXT NOT NULL,
    value REAL NOT NULL,
    unit TEXT NOT NULL,
    model_version TEXT NOT NULL
);
CREATE TABLE license_cache (
    allocation_id TEXT PRIMARY KEY,
    profile_id TEXT NOT NULL REFERENCES profile(id),
    device_id TEXT NOT NULL,
    signed_token TEXT NOT NULL,
    expires_at TEXT NOT NULL
);
CREATE INDEX outbox_due ON outbox(status, next_retry_at);
