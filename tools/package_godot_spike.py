"""Package only a successfully exercised standalone Windows diagnostic export."""
import hashlib
import json
from pathlib import Path
import zipfile

ROOT = Path(__file__).resolve().parents[1]


def sha256(path):
    digest = hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(block)
    return digest.hexdigest()


def main():
    base = ROOT/'artifacts/godot-spike'
    report = json.loads((base/'windows-exported-results/godot-windows-result.json').read_text())
    if not report['success'] or not report['exportTemplate']:
        raise RuntimeError('Run and verify the standalone export before packaging.')
    folder = base/'windows-export'
    files = sorted(p for p in folder.rglob('*') if p.is_file() and p.name != 'SHA256SUMS.txt')
    checksums = {p.relative_to(folder).as_posix(): sha256(p) for p in files}
    (folder/'SHA256SUMS.txt').write_text(''.join(f'{digest}  {name}\n' for name, digest in checksums.items()))
    target = base/'VirtualSTEM-Godot-Compatibility-Windows-x64.zip'
    with zipfile.ZipFile(target, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
        for path in [*files, folder/'SHA256SUMS.txt']:
            archive.write(path, 'VirtualSTEM-Godot-Compatibility/' + path.relative_to(folder).as_posix())
    manifest = {'archive': target.name, 'bytes': target.stat().st_size, 'sha256': sha256(target),
                'files': checksums, 'scope': 'Phase 0 diagnostic preview; not a commercial release.'}
    (base/'package-manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
    print(json.dumps({key: manifest[key] for key in ('archive', 'bytes', 'sha256')}, indent=2))


if __name__ == '__main__':
    main()
