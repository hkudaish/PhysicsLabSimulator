"""Execute the shared native SQLite probe in separate processes, including abrupt exit."""
import argparse
import json
from pathlib import Path
import subprocess
import tempfile

from dev import environment

ROOT = Path(__file__).resolve().parents[1]


def windows_path(path):
    value = Path(path).resolve().as_posix()
    if not value.startswith('/mnt/') or len(value) < 8 or value[6] != '/':
        raise ValueError('Windows probe files must be on a mounted Windows drive.')
    return value[5].upper() + ':\\' + value[7:].replace('/', '\\')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--windows', action='store_true')
    args = parser.parse_args()
    platform = 'windows' if args.windows else 'linux'
    host = ROOT / 'spikes/NativeSqlite' / ('WindowsHost' if args.windows else 'LinuxHost')
    executable = '/mnt/c/Program Files/dotnet/dotnet.exe' if args.windows else environment()['DOTNET_ROOT'] + '/dotnet'
    env = environment()
    if args.windows:
        env['DOTNET_ROOT'] = r'C:\Program Files\dotnet'
        env['DOTNET_CLI_HOME'] = windows_path(ROOT / 'artifacts/tool-cache/windows-dotnet')
        env['NUGET_PACKAGES'] = windows_path(ROOT / 'artifacts/tool-cache/windows-nuget')
    project = host / (host.name + '.csproj')
    build = subprocess.run([executable, 'build', project.name, '--configuration', 'Release', '--nologo'],
                           cwd=host, env=env, capture_output=True, text=True, timeout=90)
    if build.returncode:
        raise RuntimeError(build.stdout + build.stderr)
    framework = 'net8.0' if args.windows else 'net10.0'
    assembly = host / 'bin/Release' / framework / (host.name + '.dll')
    output = ROOT / 'artifacts/native-sqlite'
    output.mkdir(exist_ok=True, parents=True)
    convert = windows_path if args.windows else str
    schema = ROOT / 'specs/data/local.sql'
    with tempfile.TemporaryDirectory(prefix=platform + '-probe-', dir=output) as working:
        unicode_dir = Path(working) / 'مختبر'
        unicode_dir.mkdir()
        database = unicode_dir / 'state.db'

        def run(mode, expected=0):
            command = [executable, convert(assembly), mode, convert(database)]
            if mode == 'seed':
                command.append(convert(schema))
            process = subprocess.run(command, env=env, cwd=ROOT, capture_output=True, text=True, timeout=30)
            if process.returncode != expected:
                raise RuntimeError(f'{mode}: exit {process.returncode}, expected {expected}\n{process.stdout}\n{process.stderr}')
            return process.stdout

        result = json.loads(run('seed'))
        run('interrupt', expected=86)
        run('verify')
        result['passed'].append('abrupt-process-exit recovery')
        result['passed'].append('Unicode database directory')
        result['platform'] = platform
        result['scope'] = 'Standalone native interop only; Unity/IL2CPP/encryption unverified.'
        (output / (platform + '-result.json')).write_text(json.dumps(result, indent=2, ensure_ascii=False) + '\n')
        print(json.dumps(result, indent=2, ensure_ascii=False))
    return 0


if __name__ == '__main__':
    main()
