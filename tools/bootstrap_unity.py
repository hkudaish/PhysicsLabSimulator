"""Materialize Unity's checksum-pinned URP template; do not install/activate Unity."""
import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import tarfile
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
TEMPLATE_URL = 'https://download.packages.unity.com/com.unity.template.urp-blank/-/com.unity.template.urp-blank-17.1.0.tgz'
TEMPLATE_SHA256 = '7fa34efa44a7cf7009d1a45277fb45b7cb8d9c731d57532bb03fd1452ab62a7c'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--archive', type=Path, default=Path('/tmp/stem-unity-urp-blank-17.1.0.tgz'))
    args = parser.parse_args()
    destination = ROOT / 'src/UnityClient'
    if any((destination / name).exists() for name in ('Assets', 'Packages', 'ProjectSettings')):
        raise RuntimeError('Project source already exists; refusing to replace it')
    if not args.archive.exists():
        with urllib.request.urlopen(TEMPLATE_URL, timeout=60) as response:
            args.archive.write_bytes(response.read())
    if hashlib.sha256(args.archive.read_bytes()).hexdigest() != TEMPLATE_SHA256:
        raise ValueError('Unity template checksum mismatch')
    count = 0
    with tarfile.open(args.archive) as bundle:
        for member in bundle.getmembers():
            parts = PurePosixPath(member.name).parts
            if parts[:2] != ('package', 'ProjectData~') or len(parts) < 4:
                continue
            if parts[2] not in ('Assets', 'Packages', 'ProjectSettings'):
                continue
            if '..' in parts or not member.isfile():
                raise ValueError(f'Unexpected template entry: {member.name}')
            target = destination.joinpath(*parts[2:])
            if target.exists():
                raise RuntimeError(f'Refusing to overwrite: {target}')
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(bundle.extractfile(member).read())
            count += 1
        license_file = destination / 'ThirdParty/Unity-URP-Template-LICENSE.md'
        license_file.parent.mkdir(exist_ok=True)
        license_file.write_bytes(bundle.extractfile('package/LICENSE.md').read())
    project_version = destination / 'ProjectSettings/ProjectVersion.txt'
    project_version.write_text('m_EditorVersion: 6000.3.27f2\nm_EditorVersionWithRevision: 6000.3.27f2 (22188f149405)\n')
    provenance = {
        'editorVersion': '6000.3.27f2', 'editorRevision': '22188f149405',
        'editorMetadata': 'https://services.api.unity.com/unity/editor/release/v1/releases?version=6000.3&limit=1',
        'template': 'com.unity.template.urp-blank', 'templateVersion': '17.1.0',
        'templateUrl': TEMPLATE_URL, 'templateSha256': TEMPLATE_SHA256,
        'templateMetadata': 'https://packages.unity.com/com.unity.template.urp-blank',
        'supplierLockfile': True, 'editorImported': False, 'windowsBuildVerified': False,
        'note': 'Official template source only; caches excluded. Supplier lockfile is not proof of import with the selected editor.'}
    (destination / 'bootstrap-provenance.json').write_text(json.dumps(provenance, indent=2) + '\n')
    print(f'Materialized {count} supplier project files, license and editor pin; editor import/build still required.')


if __name__ == '__main__':
    main()
