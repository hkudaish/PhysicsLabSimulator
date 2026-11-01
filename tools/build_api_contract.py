"""Generate the Phase 0 API design; no server behavior is implemented here."""
import copy
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
UUID = {'type': 'string', 'format': 'uuid'}
TEXT = {'type': 'string', 'minLength': 1, 'maxLength': 500}
VERSION = {'type': 'string', 'minLength': 1, 'maxLength': 100}
TIME = {'type': 'string', 'format': 'date-time'}
COUNT = {'type': 'integer', 'minimum': 0}
REVISION = {'type': 'integer', 'minimum': 0}


def ref(name):
    return {'$ref': '#/components/schemas/' + name}


def obj(fields, required=None):
    return {'type': 'object', 'additionalProperties': False,
            'properties': fields, 'required': list(fields) if required is None else required}


def enum(*values):
    return {'type': 'string', 'enum': list(values)}


def array(schema, maximum=100):
    return {'type': 'array', 'maxItems': maximum, 'items': schema}


def build():
    schemas = json.loads((ROOT / 'specs/api/core-schemas.json').read_text())
    schemas.update({
        'Session': obj({'actorId': UUID, 'tenantId': UUID, 'permissions': array(VERSION)}),
        'Tenant': obj({'id': UUID, 'name': TEXT, 'revision': REVISION}),
        'CampusInput': obj({'name': TEXT}),
        'Campus': obj({'id': UUID, 'name': TEXT, 'revision': REVISION}),
        'LabInput': obj({'campusId': UUID, 'name': TEXT}),
        'Lab': obj({'id': UUID, 'campusId': UUID, 'name': TEXT, 'revision': REVISION}),
        'MembershipInput': obj({'identityId': UUID, 'role': enum('student', 'teacher', 'institutionAdmin', 'itAdmin')}),
        'Membership': obj({'id': UUID, 'identityId': UUID, 'role': VERSION, 'status': enum('active', 'revoked'), 'revision': REVISION}),
        'ClassInput': obj({'campusId': UUID, 'teacherMembershipId': UUID, 'name': TEXT}),
        'Class': obj({'id': UUID, 'campusId': UUID, 'teacherMembershipId': UUID, 'name': TEXT, 'revision': REVISION}),
        'EnrollmentInput': obj({'studentMembershipId': UUID}),
        'Enrollment': obj({'id': UUID, 'classId': UUID, 'studentMembershipId': UUID, 'status': enum('active', 'revoked'), 'revision': REVISION}),
        'AssignmentInput': obj({'classId': UUID, 'experimentId': UUID, 'experimentVersion': VERSION,
                                'rubricVersion': VERSION, 'dueAt': {'type': ['string', 'null'], 'format': 'date-time'}}),
        'DeviceInput': obj({'installationId': UUID, 'name': TEXT, 'clientVersion': VERSION}),
        'Device': obj({'id': UUID, 'name': TEXT, 'status': enum('active', 'revoked'), 'revision': REVISION}),
        'SeatRequest': obj({'deviceId': UUID, 'subscriptionId': UUID, 'seatCount': {'type': 'integer', 'minimum': 1},
                            'mode': enum('online', 'offlineAllocation')}),
        'SeatLease': obj({'id': UUID, 'deviceId': UUID, 'seatCount': COUNT, 'expiresAt': TIME,
                          'signedToken': {'type': 'string', 'minLength': 1, 'maxLength': 16384}}),
        'Plan': obj({'id': UUID, 'version': VERSION, 'name': TEXT, 'currency': {'const': 'SAR'},
                     'annualPriceMinor': COUNT, 'concurrentSeats': COUNT}),
        'Subscription': obj({'id': UUID, 'planId': UUID, 'termStart': TIME, 'termEnd': TIME,
                             'status': enum('draft', 'active', 'suspended', 'expired', 'cancelled'), 'revision': REVISION}),
        'RenewalInput': obj({'planId': UUID, 'newTermEnd': TIME, 'contractReference': TEXT}),
        'Entitlement': obj({'id': UUID, 'subscriptionId': UUID, 'contentPackId': UUID, 'expiresAt': TIME}),
        'InvoiceInput': obj({'subscriptionId': UUID, 'contractReference': TEXT, 'amountMinor': COUNT,
                             'currency': {'const': 'SAR'}, 'dueAt': TIME}),
        'Invoice': obj({'id': UUID, 'amountMinor': COUNT, 'currency': {'const': 'SAR'},
                        'status': enum('issued', 'paid', 'partiallyPaid', 'cancelled'), 'revision': REVISION}),
        'PaymentInput': obj({'externalReference': TEXT, 'amountMinor': {'type': 'integer', 'minimum': 1},
                             'receivedAt': TIME}),
        'Payment': obj({'id': UUID, 'invoiceId': UUID, 'amountMinor': COUNT, 'receivedAt': TIME}),
        'RefundInput': obj({'amountMinor': {'type': 'integer', 'minimum': 1}, 'reason': TEXT, 'externalReference': TEXT}),
        'Refund': obj({'id': UUID, 'paymentId': UUID, 'amountMinor': COUNT, 'status': enum('recorded')}),
        'Experiment': obj({'id': UUID, 'titleAr': TEXT, 'titleEn': TEXT, 'domain': enum('chemistry', 'electricity')}),
        'DraftInput': obj({'experimentId': UUID, 'titleAr': TEXT, 'titleEn': TEXT, 'modelVersion': VERSION,
                          'objective': TEXT, 'curriculumCodes': array(VERSION), 'rubricDraftId': UUID}),
        'Draft': obj({'id': UUID, 'experimentId': UUID, 'status': enum('draft', 'scientificReview', 'instructionalReview', 'qa', 'approved', 'published'), 'revision': REVISION}),
        'ReviewInput': obj({'stage': enum('scientific', 'instructional', 'qa'), 'decision': enum('approve', 'changesRequired'),
                            'evidenceHash': {'type': 'string', 'pattern': '^[a-f0-9]{64}$'}, 'notes': TEXT}),
        'Review': obj({'id': UUID, 'reviewerId': UUID, 'stage': VERSION, 'decision': VERSION, 'reviewedAt': TIME}),
        'PublishInput': obj({'version': VERSION, 'packageHash': {'type': 'string', 'pattern': '^[a-f0-9]{64}$'},
                             'signingKeyId': VERSION, 'rubricVersion': VERSION}),
        'Manifest': obj({'experimentId': UUID, 'experimentVersion': VERSION, 'modelVersion': VERSION,
                         'schemaVersion': {'type': 'integer', 'minimum': 1}, 'packageHash': {'type': 'string', 'pattern': '^[a-f0-9]{64}$'},
                         'packageUrl': {'type': 'string', 'format': 'uri'}, 'signature': TEXT, 'keyId': VERSION, 'expiresAt': TIME}),
        'RollbackInput': obj({'experimentVersion': VERSION, 'reason': TEXT}),
        'Result': obj({'attemptId': UUID, 'rubricVersion': VERSION, 'score': {'type': ['number', 'null'], 'minimum': 0, 'maximum': 100},
                       'status': enum('provisional', 'verified', 'rejected', 'overridden'), 'revision': REVISION}),
        'OverrideInput': obj({'score': {'type': 'number', 'minimum': 0, 'maximum': 100}, 'reason': TEXT}),
        'Report': obj({'classId': UUID, 'assignedCount': COUNT, 'completedCount': COUNT,
                       'verifiedCount': COUNT, 'generatedAt': TIME}),
        'ExportJob': obj({'id': UUID, 'status': enum('queued', 'ready', 'failed'),
                          'downloadUrl': {'type': ['string', 'null'], 'format': 'uri'}}),
        'LmsInput': obj({'issuer': {'type': 'string', 'format': 'uri'}, 'clientId': VERSION,
                         'deploymentId': VERSION, 'provider': enum('lti13', 'xapi')}),
        'LmsConnection': obj({'id': UUID, 'provider': VERSION, 'status': enum('pendingValidation', 'active', 'disabled'), 'revision': REVISION}),
        'PrivacyRequestInput': obj({'membershipId': UUID, 'action': enum('export', 'delete'), 'reason': TEXT}),
        'PrivacyRequest': obj({'id': UUID, 'status': enum('requested', 'underReview', 'completed', 'rejected')}),
        'AuditEntry': obj({'id': UUID, 'actorId': UUID, 'action': VERSION, 'objectId': UUID, 'occurredAt': TIME}),
        'MutationAck': obj({'id': UUID, 'revision': REVISION, 'status': VERSION}),
    })
    for name, fields in {
        'AddWaterCommand': {'type': {'const': 'addWater'}, 'targetVesselId': UUID, 'volumeL': {'type': 'number', 'minimum': 0}},
        'SetVoltageCommand': {'type': {'const': 'setVoltage'}, 'componentId': UUID, 'voltageV': {'type': 'number', 'minimum': 0, 'maximum': 12}},
        'DisconnectCommand': {'type': {'const': 'disconnect'}, 'connectionId': UUID},
        'ResetCommand': {'type': {'const': 'reset'}},
        'UndoCommand': {'type': {'const': 'undo'}},
        'PlaceEquipmentCommand': {'type': {'const': 'placeEquipment'}, 'equipmentId': UUID, 'catalogItemId': VERSION,
                                  'positionM': obj({'x': {'type': 'number'}, 'y': {'type': 'number'}, 'z': {'type': 'number'}})},
        'ReadMeterCommand': {'type': {'const': 'readMeter'}, 'meterId': UUID, 'mode': enum('voltage', 'current'),
                             'positiveTerminalId': UUID, 'negativeTerminalId': UUID},
    }.items():
        schemas[name] = obj(fields)
        schemas['Command']['oneOf'].append(ref(name))

    def response(name, description):
        return {'description': description, 'content': {'application/json': {'schema': ref(name)}}}

    errors = {str(status): {'description': description, 'content': {'application/problem+json': {'schema': ref('Problem')}}}
              for status, description in [(400, 'Invalid envelope'), (401, 'Invalid identity'), (403, 'Insufficient permission'),
                                           (404, 'Resource unavailable'), (409, 'Revision/idempotency conflict'), (412, 'If-Match failed'),
                                           (413, 'Body limit exceeded'), (422, 'Unsupported or invalid domain data'), (429, 'Rate limited')]}
    paths = {}

    def route(path, method, operation, output, permissions, request=None, collection=False, revision=False, notes=''):
        params = [{'name': name, 'in': 'path', 'required': True, 'schema': UUID} for name in re.findall(r'{(.*?)}', path)]
        if method == 'get' and collection:
            params += [{'name': 'cursor', 'in': 'query', 'schema': VERSION},
                       {'name': 'limit', 'in': 'query', 'schema': {'type': 'integer', 'minimum': 1, 'maximum': 100, 'default': 25}}]
            page = output + 'Page'
            schemas[page] = obj({'items': array(ref(output)), 'nextCursor': {'type': ['string', 'null']}})
            output = page
        if method == 'post':
            params.append({'name': 'Idempotency-Key', 'in': 'header', 'required': True,
                           'schema': {'type': 'string', 'minLength': 1, 'maxLength': 128}})
        if revision:
            params.append({'name': 'If-Match', 'in': 'header', 'required': True,
                           'schema': {'type': 'string', 'pattern': '^"[0-9]+"$'}})
        status = '201' if method == 'post' and operation.startswith(('create', 'register', 'record', 'request', 'publish')) else '200'
        headers = {'ETag': {'description': 'Quoted integer revision where response contains revision.', 'schema': {'type': 'string'}}}
        operation_doc = {'operationId': operation, 'tags': [path.split('/')[1]],
                         'description': 'Tenant and actor derive from verified session membership. ' + notes,
                         'x-permissions': permissions, 'parameters': params,
                         'responses': {status: {**response(output, 'Authorized response; original result on identical retry'), 'headers': headers}, **copy.deepcopy(errors)}}
        if request:
            operation_doc['requestBody'] = {'required': True, 'content': {'application/json': {'schema': ref(request)}}}
        paths.setdefault('/api/v1' + path, {})[method] = operation_doc

    route('/session', 'get', 'getSession', 'Session', ['authenticated'])
    route('/tenant', 'get', 'getCurrentTenant', 'Tenant', ['institution.read'])
    for resource, name, permission in [('campuses', 'Campus', 'institution.manage'), ('labs', 'Lab', 'institution.manage'),
                                        ('memberships', 'Membership', 'members.manage'), ('classes', 'Class', 'classes.manage')]:
        route('/'+resource, 'get', 'list'+name+'s', name, [permission], collection=True)
        route('/'+resource, 'post', 'create'+name, name, [permission], request=name+'Input')
        route('/'+resource+'/{resourceId}', 'get', 'get'+name, name, [permission])
    route('/classes/{classId}/enrollments', 'get', 'listEnrollments', 'Enrollment', ['class.enroll'], collection=True)
    route('/classes/{classId}/enrollments', 'post', 'createEnrollment', 'Enrollment', ['class.enroll'], 'EnrollmentInput')
    route('/enrollments/{enrollmentId}/revoke', 'post', 'revokeEnrollment', 'MutationAck', ['class.enroll'], revision=True)
    route('/assignments', 'get', 'listAssignments', 'Assignment', ['assignment.readOwnOrClass'], collection=True)
    route('/assignments', 'post', 'createAssignment', 'Assignment', ['assignment.createOwnClass'], 'AssignmentInput')
    route('/assignments/{assignmentId}', 'get', 'getAssignment', 'Assignment', ['assignment.readOwnOrClass'])
    route('/assignments/{assignmentId}/revoke', 'post', 'revokeAssignment', 'MutationAck', ['assignment.manageOwnClass'], revision=True)
    route('/attempts', 'post', 'createAttempt', 'Attempt', ['attempt.createOwn'], 'CreateAttempt')
    route('/attempts/{attemptId}', 'get', 'getAttempt', 'Attempt', ['attempt.readOwnOrClass'])
    route('/attempts/{attemptId}/events', 'post', 'syncAttemptEvents', 'BatchAck', ['attempt.submitOwn'], 'EventBatch',
          notes='Cap 100 events / 1 MiB. Per-event acknowledgement; retain rejected events. Sequence gaps reject dependent successors. Same ID with changed payload conflicts.')
    route('/attempts/{attemptId}/result', 'get', 'getAssessmentResult', 'Result', ['attempt.readOwnOrClass'])
    route('/attempts/{attemptId}/override', 'post', 'overrideAssessment', 'Result', ['assessment.overrideOwnClass'], 'OverrideInput', revision=True,
          notes='Teacher override requires reason and append-only audit. This is not a learner score-submission endpoint.')
    route('/devices', 'post', 'registerDevice', 'Device', ['device.register'], 'DeviceInput')
    route('/devices', 'get', 'listDevices', 'Device', ['devices.manage'], collection=True)
    route('/devices/{deviceId}/revoke', 'post', 'revokeDevice', 'MutationAck', ['devices.manage'], revision=True)
    route('/seat-leases', 'post', 'requestSeatLease', 'SeatLease', ['seats.allocate'], 'SeatRequest',
          notes='Offline allocation reduces available online pool until expiry. Enforce aggregate limit transactionally.')
    route('/seat-leases/{leaseId}/release', 'post', 'releaseSeatLease', 'MutationAck', ['seats.release'])
    route('/plans', 'get', 'listPlans', 'Plan', ['plans.read'], collection=True)
    route('/subscriptions', 'get', 'listSubscriptions', 'Subscription', ['subscriptions.read'], collection=True)
    route('/subscriptions/{subscriptionId}/renewals', 'post', 'recordRenewal', 'Subscription', ['commercial.manage'], 'RenewalInput', revision=True)
    route('/entitlements', 'get', 'listEntitlements', 'Entitlement', ['entitlements.readOwn'], collection=True)
    route('/invoices', 'get', 'listInvoices', 'Invoice', ['commercial.read'], collection=True)
    route('/invoices', 'post', 'createInvoice', 'Invoice', ['commercial.manage'], 'InvoiceInput')
    route('/invoices/{invoiceId}/payments', 'post', 'recordPayment', 'Payment', ['commercial.manage'], 'PaymentInput', revision=True,
          notes='Records reconciled payment; does not charge a card or imply recognized revenue.')
    route('/payments/{paymentId}/refunds', 'post', 'recordRefund', 'Refund', ['commercial.manage'], 'RefundInput',
          notes='Audited record only; no external funds movement.')
    route('/experiments', 'get', 'listExperiments', 'Experiment', ['content.readEntitled'], collection=True)
    route('/experiments/{experimentId}/versions/{versionId}/manifest', 'get', 'getManifest', 'Manifest', ['content.readEntitled'])
    route('/content-drafts', 'post', 'createContentDraft', 'Draft', ['content.author'], 'DraftInput')
    route('/content-drafts/{draftId}', 'get', 'getContentDraft', 'Draft', ['content.reviewOrAuthor'])
    route('/content-drafts/{draftId}/reviews', 'post', 'recordContentReview', 'Review', ['content.reviewAssignedStage'], 'ReviewInput', revision=True,
          notes='Enforce scientific → instructional → QA order; reviewer cannot approve own draft.')
    route('/content-drafts/{draftId}/publications', 'post', 'publishContent', 'Manifest', ['content.publish'], 'PublishInput', revision=True,
          notes='Requires all approvals; published versions immutable; independent package signing.')
    route('/experiments/{experimentId}/rollbacks', 'post', 'recordContentRollback', 'Manifest', ['content.publish'], 'RollbackInput',
          notes='Changes active-version pointer only; preserves previous versions and attempts.')
    route('/classes/{classId}/report', 'get', 'getClassReport', 'Report', ['report.readOwnClass'])
    route('/classes/{classId}/exports', 'post', 'requestClassExport', 'ExportJob', ['report.exportOwnClass'])
    route('/lms-connections', 'get', 'listLmsConnections', 'LmsConnection', ['integrations.manage'], collection=True)
    route('/lms-connections', 'post', 'createLmsConnection', 'LmsConnection', ['integrations.manage'], 'LmsInput',
          notes='No credentials in payload. Store credential reference through a separately secured provisioning process. LTI launch/grade protocols require dedicated conformance contracts before integration.')
    route('/privacy-requests', 'post', 'requestPrivacyAction', 'PrivacyRequest', ['privacy.manage'], 'PrivacyRequestInput',
          notes='Creates a reviewed request, does not immediately delete student data or legal records.')
    route('/audit-log', 'get', 'listAuditLog', 'AuditEntry', ['audit.readRestricted'], collection=True)
    return {'openapi': '3.1.0', 'info': {'title': 'Virtual STEM Laboratory API design', 'version': '0.2.0',
            'description': 'Phase 0 contracts only. No application routes are implemented. Fine-grained institution/plan edits and LMS protocol adapters remain future contract revisions.'},
            'security': [{'bearerAuth': []}], 'paths': paths,
            'components': {'securitySchemes': {'bearerAuth': {'type': 'http', 'scheme': 'bearer', 'bearerFormat': 'JWT'}}, 'schemas': schemas}}


if __name__ == '__main__':
    contract = build()
    destination = ROOT / 'specs/api/openapi.json'
    destination.write_text(json.dumps(contract, indent=2, ensure_ascii=False) + '\n')
    print(f'{len(contract["paths"])} paths; {sum(len(v) for v in contract["paths"].values())} operations; {len(contract["components"]["schemas"])} schemas')
