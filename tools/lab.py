"""Build, exercise and package the Phase 1 application in isolated test profiles."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import zipfile

from run_godot_spike import context, ROOT

PROJECT = ROOT / 'src/GodotClient'
OUTPUT = ROOT / 'artifacts/visual-upgrade'
ARCHIVE = 'VirtualSTEM-Lab-Visual-Windows-x64.zip'


def digest(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['build', 'import', 'verify', 'export', 'package', 'close-check', 'package-check', 'legacy-check'])
    parser.add_argument('--windows', action='store_true', help='Exercise the standalone Windows export.')
    parser.add_argument('--compatibility', action='store_true', help='Use the OpenGL fallback renderer.')
    parser.add_argument('--capture', action='store_true')
    parser.add_argument('--resume', action='store_true')
    parser.add_argument('--benchmark-seconds', type=int, help='Real-renderer seconds per experiment and preset (5–3600).')
    args = parser.parse_args()
    lighting=PROJECT/'assets/lighting'
    provenance=json.loads((lighting/'provenance.json').read_text())
    if digest(lighting/'studio_small_09_1k.hdr') != provenance['sha256']:
        raise RuntimeError('Bundled lighting asset does not match its recorded checksum.')
    if args.action in ['package-check', 'legacy-check']:
        args.windows = args.resume = True
    binary, env = context(False)
    OUTPUT.mkdir(parents=True, exist_ok=True)
    results = OUTPUT / (('windows' if args.windows else 'linux') + ('-compat-results' if args.compatibility else '-results'))
    results.mkdir(parents=True, exist_ok=True)
    env['XDG_DATA_HOME'] = str(OUTPUT / 'linux-profile/data')
    env['XDG_CONFIG_HOME'] = str(OUTPUT / 'linux-profile/config')
    env['STEM_LAB_RESULTS'] = str(results)
    env['STEM_LAB_TEST_STORAGE'] = str(OUTPUT / (('windows' if args.windows else 'linux')+('-compat-test-storage' if args.compatibility else '-test-storage')))
    if args.windows:
        env['APPDATA'] = str(OUTPUT/'windows-profile')
        translated = ['STEM_LAB_RESULTS', 'STEM_LAB_TEST_STORAGE', 'APPDATA']
        env['WSLENV'] = ':'.join([x for x in env.get('WSLENV', '').split(':') if x and x.split('/')[0] not in translated] + [x+'/p' for x in translated])
    command = [str(binary), '--headless', '--path', str(PROJECT)]
    cwd = PROJECT
    log = OUTPUT / (args.action + ('-windows' if args.windows else '-linux') + ('-compat' if args.compatibility else '') + ('-resume' if args.resume else '') + '.log')
    exported = OUTPUT / 'windows'
    if args.action == 'build':
        command = ['dotnet', 'build', 'GodotClient.csproj', '-m:1', '--nologo']
    elif args.action == 'import':
        command += ['--editor', '--import', '--quit']
    elif args.action == 'close-check':
        if not args.windows:
            raise ValueError('Window-close evidence requires --windows.')
        env['STEM_LAB_EXE'] = str(exported/'VirtualStemLab.exe')
        env['WSLENV'] += ':STEM_LAB_EXE/p'
        script = (ROOT/'tools/check_lab_window_close.ps1').read_text().split('\n', 1)[1]
        # Execute the reviewed commands directly; leave Windows script policy unchanged.
        command = ['/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe', '-NoProfile', '-Command',
                   '$Executable=$env:STEM_LAB_EXE\n'+script]
    elif args.action == 'export':
        exported.mkdir(parents=True, exist_ok=True)
        subprocess.run(command + ['--script', 'res://WriteLicenses.gd'], env=env, cwd=cwd, check=True, timeout=30)
        command += ['--export-release', 'Windows Desktop', str(exported/'VirtualStemLab.exe')]
    elif args.action in ['verify', 'package-check', 'legacy-check']:
        if args.action == 'legacy-check':
            old=ROOT/'artifacts/phase1'
            source=old/'windows-test-storage/laboratory.db'
            if not source.exists():
                raise RuntimeError('The preserved Phase 1 draft is needed for a legacy restore check.')
            results=OUTPUT/'legacy-results';results.mkdir(exist_ok=True)
            storage=OUTPUT/'legacy-test-storage';storage.mkdir(exist_ok=True)
            with sqlite3.connect(source) as before, sqlite3.connect(storage/'laboratory.db') as after:
                before.backup(after)
            shutil.copy2(old/'windows-results/saved-workspace.sha256', results/'saved-workspace.sha256')
            env['STEM_LAB_TEST_STORAGE']=str(storage);env['STEM_LAB_RESULTS']=str(results)
        if args.action == 'package-check':
            archive_path = OUTPUT/ARCHIVE
            extraction = OUTPUT/('package-check-'+digest(archive_path)[:12])
            with zipfile.ZipFile(archive_path) as archive:
                if archive.testzip() is not None:
                    raise RuntimeError('Package CRC verification failed.')
                archive.extractall(extraction)
            exported = extraction/'VirtualSTEM-Lab'
            source_results = results
            results = OUTPUT/'package-smoke-results'
            results.mkdir(exist_ok=True)
            shutil.copy2(source_results/'saved-workspace.sha256', results/'saved-workspace.sha256')
            env['STEM_LAB_RESULTS'] = str(results)
        if args.windows:
            binary = exported / 'VirtualStemLab.exe'
            command = [str(binary)]
            cwd = exported
            if not args.capture:
                command += ['--headless']
        elif args.capture:
            command.remove('--headless')
        if args.compatibility:
            command += ['--rendering-method', 'gl_compatibility']
        command += ['--', '--resume-check' if args.resume else '--verify']
        if args.capture:
            command += ['--capture']
        if args.windows:
            command += ['--expect-export']
        if args.benchmark_seconds:
            command += ['--benchmark-seconds', str(args.benchmark_seconds)]
        report_path = results / ('resume-result.json' if args.resume else 'integration-result.json')
        failure = results/'failure.json'
        if failure.exists():
            failure.replace(results/'previous-failure.json')
        if report_path.exists():
            report_path.replace(report_path.with_suffix('.previous.json'))
    elif args.action == 'package':
        fallback_file=OUTPUT/'windows-compat-results/integration-result.json'
        fallback=json.loads(fallback_file.read_text())
        if not fallback['success'] or not fallback['exported'] or fallback['renderer']!='gl_compatibility':
            raise RuntimeError('The standalone OpenGL fallback must pass before packaging its launcher.')
        if fallback_file.stat().st_mtime < (exported/'VirtualStemLab.exe').stat().st_mtime:
            raise RuntimeError('The fallback evidence predates this export.')
        close = OUTPUT/'windows-results/window-close-result.json'
        if not json.loads(close.read_text(encoding='utf-8-sig'))['success']:
            raise RuntimeError('The real window-close check must pass.')
        for name in ['integration-result.json', 'resume-result.json']:
            report_file = OUTPUT/'windows-results'/name
            report = json.loads(report_file.read_text())
            if not report['success'] or not report['exported']:
                raise RuntimeError('Standalone Windows integration and fresh-process resume must pass.')
            if report_file.stat().st_mtime < (exported/'VirtualStemLab.exe').stat().st_mtime:
                raise RuntimeError('The runtime evidence predates this export.')
        if report_file.stat().st_mtime < close.stat().st_mtime:
            raise RuntimeError('A fresh-process resume is required after the window-close test.')
        files = sorted(p for p in exported.rglob('*') if p.is_file() and p.name != 'SHA256SUMS.txt')
        sums = {p.relative_to(exported).as_posix(): digest(p) for p in files}
        checksums = exported / 'SHA256SUMS.txt'
        checksums.write_text(''.join(f'{value}  {name}\n' for name, value in sums.items()))
        target = OUTPUT / ARCHIVE
        with zipfile.ZipFile(target, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
            for path in [*files, checksums]:
                archive.write(path, 'VirtualSTEM-Lab/' + path.relative_to(exported).as_posix())
        manifest = dict(archive=target.name, bytes=target.stat().st_size, sha256=digest(target), files=sums,
                        scope='Phase 1 visual upgrade; independent science review and target-school qualification pending.')
        (OUTPUT / 'package-manifest.json').write_text(json.dumps(manifest, indent=2)+'\n')
        print(json.dumps({k: manifest[k] for k in ['archive', 'bytes', 'sha256']}, indent=2))
        return 0
    with log.open('w') as stream:
        result = subprocess.run(command, cwd=cwd, env=env, stdout=stream, stderr=subprocess.STDOUT,
                                timeout=max(300, (args.benchmark_seconds or 0)*4+180))
    content = log.read_text()
    print(f'{args.action}: exit {result.returncode}; log: {log}')
    if result.returncode or 'ERROR:' in content or 'Failed to build project' in content or 'were leaked' in content or 'ObjectDB instances leaked' in content:
        print(content[-10000:])
        return result.returncode or 1
    if args.action == 'export':
        for name in ['VirtualStemLab.exe', 'VirtualStemLab.pck', 'data_GodotClient_windows_x86_64/GodotClient.dll']:
            if not (exported/name).is_file():
                raise RuntimeError('Missing export file: '+name)
        shutil.copytree(PROJECT/'licenses', exported/'licenses', dirs_exist_ok=True)
        for name in ['Tajawal-OFL.txt', 'DejaVu-COPYRIGHT.txt']:
            shutil.copy2(PROJECT/'assets'/name, exported/'licenses'/name)
        runtime_license = ROOT/'artifacts/tool-cache/nuget/microsoft.netcore.app.runtime.win-x64/10.0.13'
        for name in ['LICENSE.TXT', 'THIRD-PARTY-NOTICES.TXT']:
            shutil.copy2(runtime_license/name, exported/'licenses'/('dotnet-'+name))
        (exported/'LowGraphics.bat').write_bytes(b'@echo off\r\ncd /d "%~dp0"\r\nstart "" "VirtualStemLab.exe" --rendering-method gl_compatibility -- --low-graphics\r\n')
        (exported/'README.txt').write_text(
            'Virtual STEM Laboratory — Visual upgrade\n\n'
            'Extract the complete ZIP and run VirtualStemLab.exe on Windows x64.\n'
            'Keep the executable, PCK and managed data folder together. No Godot editor is required.\n'
            'For older graphics cards, launch LowGraphics.bat.\n'
            'Help inside the app explains dilution, wiring, camera and undo controls.\n'
            'Both experiment drafts save automatically when closing, or use Save locally.\n'
            'Local drafts: %APPDATA%/Godot/app_userdata/Virtual STEM Laboratory/drafts/laboratory.db\n'
            'Anonymous local drafts only: no accounts, cloud sync or grade records.\n'
            'Independent scientific review, school-hardware qualification and the 60-minute acceptance benchmark remain pending.\n', encoding='utf-8')
    if args.action in ['verify', 'package-check', 'legacy-check']:
        report = json.loads(report_path.read_text())
        if not report['success'] or (args.windows and not report['exported']):
            raise RuntimeError('Missing successful runtime evidence.')
        if args.capture:
            for name in ['chemistry', 'electricity']:
                if not (results/(name+'-lab.png')).is_file():
                    raise RuntimeError('Missing rendered screenshot.')
        print('Passed checks:', len(report['passed']))
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
