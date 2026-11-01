"""Install checksum-pinned Linux build tools in /tmp; never change system tools."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import tarfile
import tempfile
import urllib.request

TOOLS = {
    'dotnet': {
        'version': '10.0.402',
        'archive': 'stem-dotnet-sdk-10.0.402.tar.gz',
        'url': 'https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.402/dotnet-sdk-10.0.402-linux-x64.tar.gz',
        'algorithm': 'sha512',
        'digest': '1904a98dda2db34ba78ba3c4fb1b224455329af91c341d83547843c1839e112b04ae699ee92fa2449a8f62249196a938723078c0c60b9f65c88539d5687729c2',
        'source': 'https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json',
    },
    'node': {
        'version': '24.22.2',
        'archive': 'stem-node-v24.22.2-linux-x64.tar.xz',
        'url': 'https://nodejs.org/dist/v24.22.2/node-v24.22.2-linux-x64.tar.xz',
        'algorithm': 'sha256',
        'digest': '48cc0e21e048790e54c0cf5749c2725005313d652651c40b7a21feb9697aaf75',
        'source': 'https://nodejs.org/dist/v24.22.2/SHASUMS256.txt',
    },
}


def verify(archive, tool):
    digest = hashlib.new(tool['algorithm'])
    with archive.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(block)
    if digest.hexdigest() != tool['digest']:
        raise ValueError(f'Checksum mismatch: {archive}')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=Path('/tmp/virtual-stem-toolchain'))
    parser.add_argument('--archives', type=Path, default=Path('/tmp'))
    parser.add_argument('--offline', action='store_true')
    args = parser.parse_args()
    args.root.mkdir(parents=True, exist_ok=True)
    args.archives.mkdir(parents=True, exist_ok=True)
    for name, tool in TOOLS.items():
        target = args.root / name
        marker = target / '.stem-toolchain.json'
        if marker.is_file() and json.loads(marker.read_text()) == tool:
            print(f'{name} {tool["version"]}: already installed')
            continue
        if target.exists():
            raise RuntimeError(f'Refusing to replace unrecognized directory: {target}')
        archive = args.archives / tool['archive']
        if not archive.exists():
            if args.offline:
                raise FileNotFoundError(archive)
            temporary = archive.with_suffix(archive.suffix + '.download')
            with urllib.request.urlopen(tool['url'], timeout=60) as response, temporary.open('wb') as out:
                shutil.copyfileobj(response, out)
            temporary.replace(archive)
        verify(archive, tool)
        with tempfile.TemporaryDirectory(dir=args.root) as staging:
            extracted = Path(staging) / 'extract'
            extracted.mkdir()
            with tarfile.open(archive) as bundle:
                bundle.extractall(extracted, filter='data')
            source = extracted if name == 'dotnet' else extracted / f'node-v{tool["version"]}-linux-x64'
            (source / '.stem-toolchain.json').write_text(json.dumps(tool, indent=2) + '\n')
            source.rename(target)
        print(f'{name} {tool["version"]}: checksum verified and installed in {target}')
    print(f'export PATH="{args.root}/dotnet:{args.root}/node/bin:$PATH"')
    print(f'export DOTNET_ROOT="{args.root}/dotnet"')


if __name__ == '__main__':
    main()
