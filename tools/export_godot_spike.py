"""Export the compatibility experiment; a subsequent Windows runtime check is mandatory."""
import json
from pathlib import Path
import shutil
import subprocess

from run_godot_spike import context, PROJECT, ROOT


def main():
    binary, env = context(False)
    attribution = subprocess.run([str(binary), '--headless', '--path', str(PROJECT),
                                  '--script', 'res://WriteLicenses.gd'], cwd=PROJECT, env=env,
                                 capture_output=True, text=True, timeout=30)
    if attribution.returncode:
        raise RuntimeError(attribution.stdout + attribution.stderr)
    output = ROOT / 'artifacts/godot-spike/windows-export'
    output.mkdir(parents=True, exist_ok=True)
    log_path = ROOT / 'artifacts/godot-spike/export.log'
    with log_path.open('w') as log:
        result = subprocess.run([str(binary), '--headless', '--path', str(PROJECT), '--export-release',
                                 'Windows Desktop', str(output/'VirtualStemCompatibility.exe')], cwd=PROJECT,
                                env=env, stdout=log, stderr=subprocess.STDOUT, timeout=240)
    text = log_path.read_text()
    # Some editor export paths can return success after a managed compilation failure.
    if result.returncode or 'ERROR:' in text or 'Failed to build project' in text:
        raise RuntimeError(f'Export failed; inspect {log_path}')
    expected = ['VirtualStemCompatibility.exe', 'VirtualStemCompatibility.pck',
                'data_GodotCompatibility_windows_x86_64/GodotCompatibility.dll']
    for name in expected:
        if not (output/name).is_file():
            raise RuntimeError(f'Export missing required file: {name}')
    shutil.copytree(PROJECT/'licenses', output/'licenses', dirs_exist_ok=True)
    for name in ('Tajawal-OFL.txt', 'DejaVu-COPYRIGHT.txt'):
        shutil.copyfile(PROJECT/'assets'/name, output/'licenses'/name)
    (output/'README.txt').write_text(
        'Virtual STEM Laboratory — Phase 0 Godot compatibility experiment\n\n'
        'Keep this executable, PCK, data folder and licenses together.\n'
        'Run VirtualStemCompatibility.exe on Windows x64.\n'
        'Use Arabic/English, scale buttons and Run diagnostics.\n'
        'This is a diagnostic preview, not a playable scientific laboratory.\n'
        'No school hardware, instructional or scientific acceptance is implied.\n', encoding='utf-8')
    print(f'Export created at {output}; run tools/run_godot_spike.py --exported --capture next.')
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
