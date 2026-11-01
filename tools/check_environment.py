"""Report actual tools; return nonzero when Phase 0 build prerequisites are missing."""
import json
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]


def main():
    results = {}
    for name, args in {"dotnet": ["--version"], "node": ["--version"],
                       "npm": ["--version"], "git": ["--version"],
                       "Unity": ["-version"]}.items():
        location = shutil.which(name)
        if not location:
            results[name] = {"available": False}
            continue
        try:
            run = subprocess.run([location, *args], cwd=ROOT, capture_output=True,
                                 text=True, timeout=20)
            results[name] = {"available": run.returncode == 0,
                             "version": run.stdout.strip() or run.stderr.strip()}
        except (OSError, subprocess.TimeoutExpired) as error:
            results[name] = {"available": False, "error": str(error)}
    expected = {"dotnet": "10.0.402", "node": "v24.22.2"}
    for name, version in expected.items():
        results[name]["matches_pin"] = results[name].get("version") == version
    repository = subprocess.run(["git", "rev-parse", "--is-inside-work-tree"],
                                cwd=ROOT, capture_output=True, text=True) if shutil.which("git") else None
    results["repository"] = {"available": repository is not None and repository.returncode == 0}
    print(json.dumps(results, indent=2))
    return 0 if (all(v["available"] for v in results.values())
                 and all(results[k]["matches_pin"] for k in expected)) else 1


if __name__ == "__main__":
    sys.exit(main())
