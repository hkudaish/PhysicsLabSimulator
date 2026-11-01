"""Phase 0 design checks. These do not execute product solvers or the application."""
import csv
import hashlib
import json
import math
import re
from pathlib import Path
import sqlite3
import unittest
import xml.etree.ElementTree as ET
import zipfile

ROOT = Path(__file__).resolve().parents[2]


class DesignArtifacts(unittest.TestCase):
    def test_unity_supplier_package_pins_match_lock(self):
        packages = ROOT / 'src/UnityClient/Packages'
        manifest = json.loads((packages / 'manifest.json').read_text())['dependencies']
        locked = json.loads((packages / 'packages-lock.json').read_text())['dependencies']
        for name, version in manifest.items():
            with self.subTest(package=name):
                self.assertEqual(version, locked[name]['version'])

    def test_unity_build_scene_and_pipeline_references_exist(self):
        project = ROOT / 'src/UnityClient'
        settings = (project / 'ProjectSettings/EditorBuildSettings.asset').read_text()
        scenes = re.findall(r'path: (Assets/.*\.unity)', settings)
        self.assertTrue(scenes)
        for scene in scenes:
            self.assertTrue((project / scene).is_file())
            meta = (project / (scene + '.meta')).read_text()
            self.assertIn(re.search(r'guid: ([a-f0-9]+)', meta).group(1), settings)
        graphics = (project / 'ProjectSettings/GraphicsSettings.asset').read_text()
        quality = (project / 'ProjectSettings/QualitySettings.asset').read_text()
        pipelines = re.findall(r'[Cc]ustomRenderPipeline:.*guid: ([a-f0-9]+)', graphics + quality)
        self.assertTrue(pipelines)
        metadata = [p.read_text() for p in (project / 'Assets').rglob('*.asset.meta')]
        for pipeline in pipelines:
            self.assertTrue(any(pipeline in content for content in metadata))

    def test_unity_editor_pin_and_vendor_license_preserved(self):
        project = ROOT / 'src/UnityClient'
        source = json.loads((project / 'bootstrap-provenance.json').read_text())
        version = (project / 'ProjectSettings/ProjectVersion.txt').read_text()
        self.assertIn(source['editorVersion'], version)
        self.assertIn(source['editorRevision'], version)
        self.assertFalse(source['editorImported'])
        self.assertTrue((project / 'ThirdParty/Unity-URP-Template-LICENSE.md').is_file())

    def test_original_source_hashes(self):
        manifest = json.loads((ROOT / 'docs/phase-0/source-manifest.json').read_text())
        self.assertEqual(len(manifest['files']), 6)
        for source in manifest['files']:
            with self.subTest(source=source['path']):
                data = (ROOT / source['path']).read_bytes()
                self.assertEqual(len(data), source['bytes'])
                self.assertEqual(hashlib.sha256(data).hexdigest(), source['sha256'])

    def test_requirements_have_unique_ids_and_reviewable_designs(self):
        with (ROOT / 'docs/phase-0/requirements.csv').open() as source:
            rows = list(csv.DictReader(source))
        ids = [r['id'] for r in rows]
        self.assertEqual(len(ids), len(set(ids)))
        self.assertTrue({f'BR-{n:02}' for n in range(1, 11)} <= set(ids))
        self.assertTrue({f'FR-{n:02}' for n in range(1, 13)} <= set(ids))
        self.assertEqual({r['phase'] for r in rows}, set('012345'))
        for row in rows:
            with self.subTest(requirement=row['id']):
                self.assertTrue(all(row.values()))
                self.assertTrue((ROOT / row['design_artifact']).is_file())
                self.assertIn('no product implementation', row['test_status'])

    def test_openapi_local_references_and_auth(self):
        doc = json.loads((ROOT / 'specs/api/openapi.json').read_text())
        self.assertEqual(doc['openapi'], '3.1.0')
        self.assertTrue(doc['security'])
        operations = []

        def walk(value):
            if isinstance(value, dict):
                if '$ref' in value:
                    self.assertTrue(value['$ref'].startswith('#/'))
                    target = doc
                    for segment in value['$ref'][2:].split('/'):
                        target = target[segment.replace('~1', '/').replace('~0', '~')]
                for item in value.values():
                    walk(item)
            elif isinstance(value, list):
                for item in value:
                    walk(item)

        walk(doc)
        for path, methods in doc['paths'].items():
            self.assertTrue(path.startswith('/api/v1/'))
            for method, operation in methods.items():
                operations.append(operation['operationId'])
                self.assertNotEqual(operation.get('security'), [])
                self.assertTrue({'401', '403', '409'} <= operation['responses'].keys())
                if method == 'post':
                    self.assertIn('Idempotency-Key', [p['name'] for p in operation['parameters']])
        self.assertEqual(len(operations), len(set(operations)))

    def test_analytical_chemistry_fixture_arithmetic(self):
        doc = json.loads((ROOT / 'specs/science/reference-cases.json').read_text())
        for case in doc['chemistry']:
            with self.subTest(case=case['id']):
                n = case['concentration_mol_L'] * case['aliquot_L']
                v = case['aliquot_L'] + case['water_L']
                self.assertAlmostEqual(n, case['expected_moles'], delta=1e-12)
                self.assertAlmostEqual(v, case['expected_volume_L'], delta=1e-12)
                self.assertAlmostEqual(n / v, case['expected_concentration_mol_L'], delta=1e-10)

    def test_analytical_electrical_fixture_arithmetic(self):
        doc = json.loads((ROOT / 'specs/science/reference-cases.json').read_text())
        for case in doc['electricity']:
            with self.subTest(case=case['id']):
                current = case['voltage_V'] / case['resistance_ohm']
                self.assertTrue(math.isfinite(current))
                self.assertAlmostEqual(current, case['expected_current_A'], delta=1e-9)
                self.assertAlmostEqual(current**2 * case['resistance_ohm'],
                                       case['expected_power_W'], delta=1e-9)

    def test_workbook_year1_scenario_arithmetic(self):
        # Read original numeric inputs and caches, not an edited workbook or formula engine.
        ns = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
        with zipfile.ZipFile(ROOT / 'Virtual_STEM_Lab_Financial_Model_V2.xlsx') as archive:
            def sheet(number):
                tree = ET.fromstring(archive.read(f'xl/worksheets/sheet{number}.xml'))
                return {cell.get('r'): float(cell.find('s:v', ns).text)
                        for cell in tree.findall('.//s:c', ns)
                        if cell.find('s:v', ns) is not None and cell.get('t') not in ('s', 'str')}
            inputs, revenue, operating, funding = [sheet(n) for n in (1, 2, 3, 7)]
        school = sum(inputs[f'B{p}'] * inputs[f'B{n}'] for p, n in ((4, 8), (5, 9), (6, 10)))
        home = inputs['B7'] * inputs['B11']
        net = school * (1-inputs['B16']) + home * (1-inputs['B17'])
        contribution = school * (1-inputs['B16']) * (1-inputs['B18']) + home * (1-inputs['B17']) * (1-inputs['B19'])
        acquisition = sum(inputs[f'B{n}'] for n in (8, 9, 10)) * inputs['B23'] + inputs['B11'] * inputs['B24']
        result = contribution - inputs['B20'] - acquisition
        self.assertAlmostEqual(net, revenue['B17'], places=6)
        self.assertAlmostEqual(contribution, revenue['B21'], places=6)
        self.assertAlmostEqual(result, operating['B9'], places=6)
        self.assertAlmostEqual(result-inputs['B22'], operating['B14'], places=6)
        for col, total in (('C', 'B10'), ('D', 'B11')):
            self.assertAlmostEqual(sum(funding[f'{col}{r}'] for r in range(2, 7)), funding[total])


class LocalSchema(unittest.TestCase):
    def setUp(self):
        self.db = sqlite3.connect(':memory:')
        self.addCleanup(self.db.close)
        self.db.executescript((ROOT / 'specs/data/local.sql').read_text())
        self.db.execute("INSERT INTO profile VALUES ('p1','t1','u1')")
        self.db.execute("INSERT INTO profile VALUES ('p2','t2','u2')")
        self.db.execute("INSERT INTO installed_content VALUES ('dilution','1','model-1',1,'hash','key')")
        self.db.execute("INSERT INTO snapshot VALUES ('s1','p1','dilution','1','model-1',1,0,'{}','2026-11-01T00:00:00Z')")
        self.db.commit()

    def event(self, event_id='e1', sequence=1, profile='p1'):
        self.db.execute('INSERT INTO journal VALUES (?,?,?,?,?,?,?)',
                        (event_id, profile, 's1', sequence, '{}', 'hash', '2026-11-01T00:00:00Z'))

    def test_schema_and_foreign_keys_enabled(self):
        self.assertEqual(self.db.execute('PRAGMA foreign_keys').fetchone()[0], 1)
        self.assertEqual(self.db.execute('PRAGMA integrity_check').fetchone()[0], 'ok')
        self.assertEqual(self.db.execute('PRAGMA foreign_key_check').fetchall(), [])

    def test_cross_profile_event_rejected(self):
        with self.assertRaises(sqlite3.IntegrityError):
            self.event(profile='p2')

    def test_duplicate_event_id_and_sequence_rejected(self):
        self.event()
        with self.assertRaises(sqlite3.IntegrityError):
            self.event(sequence=2)
        with self.assertRaises(sqlite3.IntegrityError):
            self.event(event_id='e2')

    def test_outbox_requires_existing_journal_and_receipt(self):
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute("INSERT INTO outbox(event_id) VALUES ('missing')")
        self.event()
        self.db.execute("INSERT INTO outbox(event_id) VALUES ('e1')")
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute("UPDATE outbox SET status='acknowledged' WHERE event_id='e1'")
        self.db.execute("UPDATE outbox SET status='acknowledged',server_receipt='receipt' WHERE event_id='e1'")

    def test_atomic_rollback_preserves_snapshot_and_journal(self):
        with self.assertRaises(sqlite3.IntegrityError):
            with self.db:
                self.db.execute("UPDATE snapshot SET revision=1,state_json='{\"changed\":true}' WHERE id='s1'")
                self.event()
                self.db.execute("INSERT INTO outbox(event_id) VALUES ('missing')")
        self.assertEqual(self.db.execute("SELECT revision,state_json FROM snapshot WHERE id='s1'").fetchone(), (0, '{}'))
        self.assertEqual(self.db.execute('SELECT count(*) FROM journal').fetchone()[0], 0)

    def test_unknown_content_version_rejected(self):
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute("UPDATE snapshot SET experiment_version='unknown' WHERE id='s1'")


if __name__ == '__main__':
    unittest.main()
