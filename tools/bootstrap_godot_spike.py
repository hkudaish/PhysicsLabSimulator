"""Fetch the exact portable Godot artifacts used by the Phase 0 experiment."""
import argparse
import hashlib
import json
from pathlib import Path
import urllib.request
import zipfile

ROOT = Path(__file__).resolve().parents[1]


def digest(path):
    value = hashlib.sha256()
    with path.open('rb') as source:
        for block in iter(lambda: source.read(1024 * 1024), b''):
            value.update(block)
    return value.hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--offline', action='store_true', help='Use the archives already in /tmp.')
    args = parser.parse_args()
    metadata = json.loads((ROOT/'spikes/GodotCompatibility/toolchain.json').read_text())
    for record in metadata['files']:
        archive = Path('/tmp') / record['archive']
        if not archive.is_file():
            if args.offline:
                raise FileNotFoundError(archive)
            temporary = archive.with_suffix(archive.suffix + '.download')
            with urllib.request.urlopen(record['url'], timeout=60) as response, temporary.open('wb') as stream:
                while block := response.read(1024 * 1024):
                    stream.write(block)
            temporary.replace(archive)
        if archive.stat().st_size != record['bytes'] or digest(archive) != record['sha256']:
            raise ValueError('Artifact does not match the recorded pin: ' + str(archive))
        with zipfile.ZipFile(archive) as bundle:
            if 'templates' in record['archive']:
                destination = ROOT/'artifacts/godot-toolchain/export-templates/4.7.2.stable.mono'
                destination.mkdir(parents=True, exist_ok=True)
                for name in bundle.namelist():
                    if name.startswith('templates/windows_') and 'x86_64' in name:
                        (destination/Path(name).name).write_bytes(bundle.read(name))
            else:
                windows = 'win64' in record['archive']
                destination = ROOT/'artifacts/godot-toolchain' if windows else Path('/tmp/stem-godot-tools')
                destination.mkdir(parents=True, exist_ok=True)
                for entry in bundle.infolist():
                    if not (destination/entry.filename).resolve().is_relative_to(destination.resolve()):
                        raise ValueError('Unsafe ZIP entry: ' + entry.filename)
                bundle.extractall(destination)
                folder = destination / ('Godot_v4.7.2-stable_mono_win64' if windows else 'Godot_v4.7.2-stable_mono_linux_x86_64')
                (folder/'_sc_').touch()
                if not windows:
                    (folder/'Godot_v4.7.2-stable_mono_linux.x86_64').chmod(0o755)
        print(record['artifact'] + ': recorded hash matched; portable tools prepared')
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
