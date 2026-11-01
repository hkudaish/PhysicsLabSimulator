"""Run the isolated Phase 0 Godot C# project with writable test-only profiles."""
import argparse
import json
from pathlib import Path
import subprocess

from dev import environment
from run_native_sqlite_spike import windows_path

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / 'spikes/GodotCompatibility'


def context(windows=False):
    env = environment()
    output = ROOT / 'artifacts/godot-spike'
    output.mkdir(parents=True, exist_ok=True)
    if windows:
        binary = ROOT / 'artifacts/godot-toolchain/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe'
        env['DOTNET_ROOT'] = '/mnt/c/Program Files/dotnet'
        env['DOTNET_CLI_HOME'] = str(ROOT / 'artifacts/tool-cache/windows-dotnet')
        env['NUGET_PACKAGES'] = str(ROOT / 'artifacts/tool-cache/windows-nuget')
        env['APPDATA'] = str(output / 'windows-profile')
        env['STEM_GODOT_RESULTS'] = str(output / 'windows')
        translated = ['DOTNET_ROOT', 'DOTNET_CLI_HOME', 'NUGET_PACKAGES', 'APPDATA', 'STEM_GODOT_RESULTS']
        shared = ['DOTNET_CLI_TELEMETRY_OPTOUT', 'DOTNET_SKIP_FIRST_TIME_EXPERIENCE', 'DOTNET_GENERATE_ASPNET_CERTIFICATE']
        entries = [entry for entry in env.get('WSLENV', '').split(':') if entry and entry.split('/')[0] not in translated + shared]
        env['WSLENV'] = ':'.join(entries + [key + '/p' for key in translated] + shared)
    else:
        binary = Path('/tmp/stem-godot-tools/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64')
        env['XDG_DATA_HOME'] = str(output / 'linux-data')
        env['XDG_CONFIG_HOME'] = str(output / 'linux-config')
        env['STEM_GODOT_RESULTS'] = str(output / 'linux')
    return binary, env


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--windows', action='store_true')
    parser.add_argument('--import-only', action='store_true')
    parser.add_argument('--capture', action='store_true')
    parser.add_argument('--exported', action='store_true', help='Run the exported Windows binary with no source-project path.')
    args = parser.parse_args()
    if args.exported:
        args.windows = True
    binary, env = context(args.windows)
    if args.exported:
        binary = ROOT / 'artifacts/godot-spike/windows-export/VirtualStemCompatibility.exe'
        exported_output = ROOT / 'artifacts/godot-spike/windows-exported-results'
        exported_output.mkdir(parents=True, exist_ok=True)
        env['STEM_GODOT_RESULTS'] = str(exported_output)
    if args.capture and not args.exported:
        capture_output = ROOT / 'artifacts/godot-spike' / ('windows-rendered' if args.windows else 'linux-rendered')
        capture_output.mkdir(parents=True, exist_ok=True)
        env['STEM_GODOT_RESULTS'] = str(capture_output)
    convert = windows_path if args.windows else str
    command = [str(binary)] if args.exported else [str(binary), '--path', convert(PROJECT)]
    if args.import_only:
        command += ['--headless', '--editor', '--import', '--quit']
    else:
        if not args.capture:
            command += ['--headless']
        command += ['--', '--verify']
        if args.exported:
            command += ['--expect-export']
        if args.capture:
            command += ['--capture']
    log = ROOT / 'artifacts/godot-spike' / ('windows-exported-results' if args.exported else ('windows' if args.windows else 'linux'))
    log.mkdir(parents=True, exist_ok=True)
    report_path = Path(env['STEM_GODOT_RESULTS']) / ('godot-windows-result.json' if args.windows else 'godot-linux-result.json')
    if not args.import_only and report_path.exists():
        report_path.replace(report_path.with_name('previous-result.json'))
    with (log / ('import.log' if args.import_only else 'run.log')).open('w') as stream:
        result = subprocess.run(command, cwd=binary.parent if args.exported else PROJECT, env=env,
                                stdout=stream, stderr=subprocess.STDOUT, timeout=120)
    print('Godot exit:', result.returncode, 'Log:', log)
    if not args.import_only and result.returncode == 0:
        if not report_path.is_file():
            raise RuntimeError('Godot returned success without producing a fresh diagnostic report.')
        report = json.loads(report_path.read_text())
        if not report['success'] or (args.exported and not report['exportTemplate']):
            raise RuntimeError('Diagnostic report did not satisfy the requested execution mode.')
        if args.capture and not (report_path.parent/'godot-windows-ui.png').is_file():
            raise RuntimeError('Requested rendered viewport was not captured.')
    return result.returncode


if __name__ == '__main__':
    raise SystemExit(main())
