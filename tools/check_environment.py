"""Report actual tools; return nonzero when Phase 0 build prerequisites are missing."""
import json
from pathlib import Path
import shutil
import subprocess
import sys

from dev import environment

ROOT = Path(__file__).resolve().parents[1]


def main():
    env = environment()
    results = {}
    for name, args in {"dotnet": ["--version"], "node": ["--version"],
                       "npm": ["--version"], "git": ["--version"],
                       "Unity": ["-version"]}.items():
        location = env.get('STEM_UNITY_EDITOR') if name == 'Unity' else shutil.which(name, path=env['PATH'])
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
    if provenance.exists():
        unity_pin = json.loads(provenance.read_text())['editorVersion']
        results['Unity']['expected_version'] = unity_pin
        results['Unity']['matches_pin'] = unity_pin in results['Unity'].get('version', '')
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
                 and all(results[k]["matches_pin"] for k in (*expected, 'Unity'))) else 1


if __name__ == "__main__":
    sys.exit(main())
