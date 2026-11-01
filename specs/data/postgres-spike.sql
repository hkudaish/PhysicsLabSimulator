-- Phase 0 architecture spike, not a production migration.
-- Run in a disposable database. The caller provisions the restricted test role.
CREATE SCHEMA stem_spike;
SET search_path = stem_spike, public;

CREATE TABLE tenant (
    id uuid PRIMARY KEY,
    name text NOT NULL
);
CREATE TABLE membership (
    tenant_id uuid NOT NULL REFERENCES tenant(id),
    id uuid NOT NULL,
    identity_id uuid NOT NULL,
    role text NOT NULL CHECK (role IN ('student','teacher','institutionAdmin')),
    PRIMARY KEY (tenant_id,id),
    UNIQUE (tenant_id,identity_id)
);
CREATE TABLE campus (
    tenant_id uuid NOT NULL REFERENCES tenant(id),
    id uuid NOT NULL,
    name text NOT NULL,
    PRIMARY KEY (tenant_id,id)
);
CREATE TABLE classroom (
    tenant_id uuid NOT NULL,
    id uuid NOT NULL,
    campus_id uuid NOT NULL,
    teacher_id uuid NOT NULL,
    name text NOT NULL,
    revision integer NOT NULL DEFAULT 0 CHECK (revision >= 0),
    PRIMARY KEY (tenant_id,id),
    FOREIGN KEY (tenant_id,campus_id) REFERENCES campus(tenant_id,id),
    FOREIGN KEY (tenant_id,teacher_id) REFERENCES membership(tenant_id,id)
);
CREATE TABLE enrollment (
    tenant_id uuid NOT NULL,
    id uuid NOT NULL,
    class_id uuid NOT NULL,
    student_id uuid NOT NULL,
    PRIMARY KEY (tenant_id,id),
    UNIQUE (tenant_id,class_id,student_id),
    FOREIGN KEY (tenant_id,class_id) REFERENCES classroom(tenant_id,id),
    FOREIGN KEY (tenant_id,student_id) REFERENCES membership(tenant_id,id)
);
CREATE TABLE attempt (
    tenant_id uuid NOT NULL,
    id uuid NOT NULL,
    class_id uuid NOT NULL,
    actor_id uuid NOT NULL,
    experiment_version text NOT NULL,
    model_version text NOT NULL,
    rubric_version text NOT NULL,
    revision integer NOT NULL CHECK (revision >= 0),
    PRIMARY KEY (tenant_id,id),
    FOREIGN KEY (tenant_id,class_id) REFERENCES classroom(tenant_id,id),
    FOREIGN KEY (tenant_id,actor_id) REFERENCES membership(tenant_id,id)
);
CREATE TABLE attempt_event (
    tenant_id uuid NOT NULL,
    id uuid NOT NULL,
    attempt_id uuid NOT NULL,
    sequence integer NOT NULL CHECK (sequence > 0),
    payload jsonb NOT NULL,
    payload_hash text NOT NULL CHECK (payload_hash ~ '^[a-f0-9]{64}$'),
    PRIMARY KEY (tenant_id,id),
    UNIQUE (tenant_id,attempt_id,sequence),
    FOREIGN KEY (tenant_id,attempt_id) REFERENCES attempt(tenant_id,id)
);

-- Transaction-local SET is mandatory at the trusted API boundary.
-- This policy assumes clients cannot execute SQL or choose the server's tenant context.
DO $$
DECLARE table_name text;
BEGIN
    FOREACH table_name IN ARRAY ARRAY['membership','campus','classroom','enrollment','attempt','attempt_event']
    LOOP
        EXECUTE format('ALTER TABLE stem_spike.%I ENABLE ROW LEVEL SECURITY', table_name);
        EXECUTE format('ALTER TABLE stem_spike.%I FORCE ROW LEVEL SECURITY', table_name);
        EXECUTE format(
          'CREATE POLICY tenant_isolation ON stem_spike.%I USING (tenant_id = nullif(current_setting(''stem.tenant_id'', true), '''')::uuid) WITH CHECK (tenant_id = nullif(current_setting(''stem.tenant_id'', true), '''')::uuid)', table_name);
    END LOOP;
END $$;

ALTER TABLE tenant ENABLE ROW LEVEL SECURITY;
ALTER TABLE tenant FORCE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON tenant
    USING (id = nullif(current_setting('stem.tenant_id', true), '')::uuid)
    WITH CHECK (id = nullif(current_setting('stem.tenant_id', true), '')::uuid);
