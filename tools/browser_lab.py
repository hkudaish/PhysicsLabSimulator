"""Build or serve the browser lab at http://localhost:5080 using the pinned SDK."""
import argparse
import hashlib
import json
import subprocess
from pathlib import Path
from dev import environment, ROOT


def verify_assets():
    root = ROOT/'src/BrowserLab/wwwroot'
    provenance = json.loads((root/'vendor/provenance.json').read_text())
    for name, expected in provenance['files'].items():
        if hashlib.sha256((root/'vendor'/name).read_bytes()).hexdigest() != expected:
            raise RuntimeError('Renderer asset checksum mismatch: '+name)
    catalog = ROOT/'src/GodotClient/assets/catalog'
    expected = json.loads((catalog/'elements-provenance.json').read_text())['normalizedSha256']
    if hashlib.sha256((catalog/'elements.json').read_bytes()).hexdigest() != expected:
        raise RuntimeError('Element reference checksum mismatch.')
    lighting = ROOT/'src/GodotClient/assets/lighting'
    expected = json.loads((lighting/'provenance.json').read_text())['sha256']
    if hashlib.sha256((root/'assets/studio_small_09_1k.hdr').read_bytes()).hexdigest() != expected:
        raise RuntimeError('Lighting asset checksum mismatch.')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['serve', 'build'], nargs='?', default='serve')
    parser.add_argument('--no-build', action='store_true', help='Serve an already-built checkout.')
    args = parser.parse_args()
    verify_assets()
    env = environment()
    project = ROOT/'src/BrowserLab/BrowserLab.csproj'
    import shutil
    dotnet = shutil.which('dotnet', path=env['PATH'])
    if not dotnet:
        raise SystemExit('Install .NET SDK 10.0.402 or run tools/bootstrap_toolchain.py first.')
    if args.action == 'build':
        return subprocess.call([dotnet, 'build', str(project), '-m:1', '--nologo'], cwd=ROOT, env=env)
    print('Virtual STEM Browser Lab: http://localhost:5080 (local access only). Ctrl+C stops the server.', flush=True)
    command = [dotnet, 'run', '--project', str(project), '--no-launch-profile']
    if args.no_build:
        command.append('--no-build')
    try:
        return subprocess.call(command, cwd=ROOT, env=env)
    except KeyboardInterrupt:
        return 0


if __name__ == '__main__':
    raise SystemExit(main())
