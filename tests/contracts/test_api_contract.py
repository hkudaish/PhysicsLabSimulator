"""Schema/contract validation only; no authentication or business logic is executed."""
import importlib.util
import json
from pathlib import Path
import re
import unittest

from jsonschema import Draft202012Validator, FormatChecker
from openapi_spec_validator import validate

ROOT = Path(__file__).resolve().parents[2]
DOC = json.loads((ROOT / 'specs/api/openapi.json').read_text())
ID = '00000000-0000-4000-8000-000000000001'


def validator(name):
    schema = {'$ref': '#/components/schemas/' + name, 'components': DOC['components']}
    return Draft202012Validator(schema, format_checker=FormatChecker())


class ApiContract(unittest.TestCase):
    def test_full_openapi_validation(self):
        validate(DOC)

    def test_generated_contract_is_current(self):
        spec = importlib.util.spec_from_file_location('contract_builder', ROOT / 'tools/build_api_contract.py')
        builder = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(builder)
        self.assertEqual(builder.build(), DOC)

    def test_all_operations_declare_permission_and_path_parameters(self):
        for path, methods in DOC['paths'].items():
            for method, operation in methods.items():
                with self.subTest(path=path, method=method):
                    self.assertTrue(operation['x-permissions'])
                    names = {p['name'] for p in operation['parameters'] if p['in'] == 'path'}
                    self.assertEqual(names, set(re.findall(r'{(.*?)}', path)))

    def test_learner_attempt_does_not_accept_grade_or_tenant(self):
        request = {'assignmentId': ID, 'deviceId': ID, 'experimentVersion': '1', 'rubricVersion': '1',
                   'modelVersion': 'dilution-v1', 'schemaVersion': 1, 'clientVersion': '0.1'}
        validator('CreateAttempt').validate(request)
        for field, value in [('score', 100), ('tenantId', ID), ('actorId', ID)]:
            with self.subTest(field=field):
                self.assertTrue(list(validator('CreateAttempt').iter_errors({**request, field: value})))

    def test_typed_commands_reject_unknown_or_invalid_data(self):
        good = {'type': 'transfer', 'sourceVesselId': ID, 'targetVesselId': ID, 'volumeL': 0.1}
        validator('Command').validate(good)
        for bad in [{**good, 'volumeL': -0.1}, {**good, 'sourceVesselId': 'not-a-uuid'},
                    {'type': 'setFinalGrade', 'score': 100}, {**good, 'type': 'unmodeledReaction'}]:
            self.assertTrue(list(validator('Command').iter_errors(bad)))

    def test_batch_limit_and_sequence_shape(self):
        event = {'eventId': ID, 'sequence': 1, 'expectedRevision': 0, 'occurredAt': '2026-11-01T12:00:00Z',
                 'modelVersion': '1', 'schemaVersion': 1, 'payloadHash': 'a' * 64,
                 'command': {'type': 'addWater', 'targetVesselId': ID, 'volumeL': 0.1}}
        validator('EventBatch').validate({'requestId': ID, 'events': [event]})
        self.assertTrue(list(validator('EventBatch').iter_errors({'requestId': ID, 'events': []})))
        self.assertTrue(list(validator('EventBatch').iter_errors({'requestId': ID, 'events': [event] * 101})))
        self.assertTrue(list(validator('Event').iter_errors({**event, 'sequence': 0})))

    def test_privileged_override_requires_reason_and_bounded_score(self):
        validator('OverrideInput').validate({'score': 90, 'reason': 'Teacher reviewed the recorded observations.'})
        for data in [{'score': 100}, {'score': 101, 'reason': 'Review'}, {'score': 90, 'reason': ''}]:
            self.assertTrue(list(validator('OverrideInput').iter_errors(data)))

    def test_default_auth_cannot_be_disabled_per_operation(self):
        self.assertEqual(DOC['security'], [{'bearerAuth': []}])
        for methods in DOC['paths'].values():
            for operation in methods.values():
                self.assertNotEqual(operation.get('security'), [])


if __name__ == '__main__':
    unittest.main()
