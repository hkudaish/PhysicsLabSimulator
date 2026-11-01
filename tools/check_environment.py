"""Report actual tools; return nonzero when Phase 0 build prerequisites are missing."""
import argparse
import json
from pathlib import Path
import shutil
import subprocess
import sys

from dev import environment

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--engine', choices=('unity', 'godot'), default='unity')
    selected = parser.parse_args().engine
    env = environment()
    results = {}
    engine = 'Unity' if selected == 'unity' else 'Godot'
    for name, args in {"dotnet": ["--version"], "node": ["--version"],
                       "npm": ["--version"], "git": ["--version"],
                       engine: ['-version'] if selected == 'unity' else ['--version']}.items():
        if name == 'Unity':
            location = env.get('STEM_UNITY_EDITOR')
        elif name == 'Godot':
            location = env.get('STEM_GODOT_EDITOR', '/tmp/stem-godot-tools/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64')
        else:
            location = shutil.which(name, path=env['PATH'])
        if not location:
            results[name] = {"available": False}
            continue
        try:
            run = subprocess.run([location, *args], cwd=ROOT, capture_output=True,
                                 text=True, timeout=20, env=env)
            results[name] = {"available": run.returncode == 0,
                             "version": run.stdout.strip() or run.stderr.strip()}
        except (OSError, subprocess.TimeoutExpired) as error:
            results[name] = {"available": False, "error": str(error)}
    expected = {"dotnet": "10.0.402", "node": "v24.22.2"}
    for name, version in expected.items():
        results[name]["matches_pin"] = results[name].get("version") == version
    provenance = ROOT / 'src/UnityClient/bootstrap-provenance.json'
    if selected == 'unity' and provenance.exists():
        unity_pin = json.loads(provenance.read_text())['editorVersion']
        results['Unity']['expected_version'] = unity_pin
        results['Unity']['matches_pin'] = unity_pin in results['Unity'].get('version', '')
    elif selected == 'godot':
        results['Godot']['expected_version'] = '4.7.2'
        results['Godot']['matches_pin'] = results['Godot'].get('version', '').startswith('4.7.2.')
    metadata = ROOT / 'artifacts/source-control.git'
    git_args = ['git']
    if metadata.is_dir():
        git_args += [f'--git-dir={metadata}', f'--work-tree={ROOT}']
    repository = subprocess.run([*git_args, 'rev-parse', '--verify', 'HEAD'], cwd=ROOT,
                                capture_output=True, text=True) if shutil.which('git') else None
    results['repository'] = {'available': repository is not None and repository.returncode == 0,
                             'metadata': str(metadata) if metadata.is_dir() else 'default',
                             'checkpoint': repository.stdout.strip() if repository is not None and repository.returncode == 0 else None}
    print(json.dumps(results, indent=2))
    return 0 if (all(v["available"] for v in results.values())
                 and all(results[k]["matches_pin"] for k in (*expected, engine))) else 1


if __name__ == "__main__":
    sys.exit(main())
