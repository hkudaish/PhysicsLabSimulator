\set ON_ERROR_STOP on
SET search_path = stem_spike, public;
CREATE ROLE stem_runtime NOLOGIN NOSUPERUSER NOBYPASSRLS;
GRANT USAGE ON SCHEMA stem_spike TO stem_runtime;
GRANT SELECT, INSERT, UPDATE ON ALL TABLES IN SCHEMA stem_spike TO stem_runtime;

-- Synthetic test data only. Seed through the disposable database owner.
INSERT INTO tenant VALUES
 ('10000000-0000-4000-8000-000000000001','School A'),
 ('20000000-0000-4000-8000-000000000001','School B');
INSERT INTO campus VALUES
 ('10000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000002','Campus A'),
 ('20000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000002','Campus B');
INSERT INTO membership VALUES
 ('10000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000003','10000000-0000-4000-8000-000000000004','teacher'),
 ('20000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000003','20000000-0000-4000-8000-000000000004','teacher');
INSERT INTO classroom VALUES
 ('10000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000005','10000000-0000-4000-8000-000000000002','10000000-0000-4000-8000-000000000003','Class A',0),
 ('20000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000005','20000000-0000-4000-8000-000000000002','20000000-0000-4000-8000-000000000003','Class B',0);

SET ROLE stem_runtime;
DO $$ BEGIN
    IF EXISTS(SELECT 1 FROM campus) THEN RAISE EXCEPTION 'No-context query leaked rows'; END IF;
    IF EXISTS(SELECT 1 FROM pg_roles WHERE rolname = current_user AND (rolsuper OR rolbypassrls)) THEN
        RAISE EXCEPTION 'Runtime role bypasses RLS';
    END IF;
END $$;
\echo PASS no tenant context is default deny; runtime has no bypass

BEGIN;
SELECT set_config('stem.tenant_id','10000000-0000-4000-8000-000000000001',true);
DO $$ BEGIN
    IF (SELECT count(*) FROM campus) <> 1 THEN RAISE EXCEPTION 'Wrong tenant-visible campus count'; END IF;
    IF EXISTS(SELECT 1 FROM classroom WHERE name = 'Class B') THEN RAISE EXCEPTION 'Cross-tenant read'; END IF;
END $$;
\echo PASS tenant A can read only its own rows

DO $$ BEGIN
    BEGIN
        INSERT INTO campus VALUES ('20000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000009','Attack');
        RAISE EXCEPTION 'Cross-tenant insert unexpectedly accepted';
    EXCEPTION WHEN insufficient_privilege THEN NULL;
    END;
END $$;
\echo PASS cross-tenant insert is rejected by RLS

DO $$ BEGIN
    BEGIN
        INSERT INTO classroom VALUES ('10000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000009',
            '20000000-0000-4000-8000-000000000002','10000000-0000-4000-8000-000000000003','Wrong campus',0);
        RAISE EXCEPTION 'Cross-tenant parent unexpectedly accepted';
    EXCEPTION WHEN foreign_key_violation THEN NULL;
    END;
END $$;
\echo PASS composite foreign key rejects cross-tenant parent

DO $$ BEGIN
    BEGIN
        UPDATE campus SET tenant_id = '20000000-0000-4000-8000-000000000001';
        RAISE EXCEPTION 'Tenant reassignment unexpectedly accepted';
    EXCEPTION WHEN insufficient_privilege THEN NULL;
    END;
END $$;
\echo PASS update cannot move a row into another tenant

INSERT INTO attempt VALUES ('10000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000010',
 '10000000-0000-4000-8000-000000000005','10000000-0000-4000-8000-000000000003','1','1','1',0);
INSERT INTO attempt_event VALUES ('10000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000011',
 '10000000-0000-4000-8000-000000000010',1,'{}',repeat('a',64));
DO $$ BEGIN
    BEGIN
        INSERT INTO attempt_event VALUES ('10000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000011',
         '10000000-0000-4000-8000-000000000010',2,'{}',repeat('b',64));
        RAISE EXCEPTION 'Duplicate event ID unexpectedly accepted';
    EXCEPTION WHEN unique_violation THEN NULL;
    END;
    BEGIN
        INSERT INTO attempt_event VALUES ('10000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000012',
         '10000000-0000-4000-8000-000000000010',1,'{}',repeat('b',64));
        RAISE EXCEPTION 'Duplicate sequence unexpectedly accepted';
    EXCEPTION WHEN unique_violation THEN NULL;
    END;
END $$;
\echo PASS event ID and attempt sequence uniqueness
COMMIT;

DO $$ BEGIN
    IF EXISTS(SELECT 1 FROM campus) THEN RAISE EXCEPTION 'Transaction-local tenant leaked after commit'; END IF;
END $$;
\echo PASS tenant context does not survive transaction commit

BEGIN;
SELECT set_config('stem.tenant_id','20000000-0000-4000-8000-000000000001',true);
DO $$ BEGIN
    IF (SELECT count(*) FROM campus WHERE name='Campus B') <> 1 THEN RAISE EXCEPTION 'Tenant B unavailable'; END IF;
    IF EXISTS(SELECT 1 FROM attempt_event) THEN RAISE EXCEPTION 'Tenant A event leaked to tenant B'; END IF;
END $$;
ROLLBACK;
\echo PASS second tenant can read own rows without seeing first tenant events
