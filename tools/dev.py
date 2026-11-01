"""Run pinned local tools without writing caches to the user's home directory."""
import os
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]


def environment():
    env = os.environ.copy()
    toolchain = Path(env.get('STEM_TOOLCHAIN_ROOT', '/tmp/virtual-stem-toolchain'))
    cache = ROOT / 'artifacts' / 'tool-cache'
    cache.mkdir(parents=True, exist_ok=True)
    env['PATH'] = os.pathsep.join([str(toolchain / 'dotnet'), str(toolchain / 'node/bin'), env.get('PATH', '')])
    env['DOTNET_ROOT'] = str(toolchain / 'dotnet')
    env['DOTNET_CLI_HOME'] = str(cache / 'dotnet-home')
    env['NUGET_PACKAGES'] = str(cache / 'nuget')
    env['DOTNET_CLI_TELEMETRY_OPTOUT'] = '1'
    env['DOTNET_SKIP_FIRST_TIME_EXPERIENCE'] = '1'
    env['DOTNET_GENERATE_ASPNET_CERTIFICATE'] = 'false'
    runtime = toolchain / 'runtime'
    runtime.mkdir(mode=0o700, parents=True, exist_ok=True)
    env['XDG_RUNTIME_DIR'] = str(runtime)
    env['npm_config_cache'] = str(cache / 'npm')
    return env


def main():
    if len(sys.argv) < 2:
        sys.exit('Usage: python3 tools/dev.py <tool> [arguments...]')
    env = environment()
    executable = shutil.which(sys.argv[1], path=env['PATH'])
    if executable is None:
        sys.exit(f'Tool unavailable: {sys.argv[1]}; run tools/bootstrap_toolchain.py first')
    return subprocess.run([executable, *sys.argv[2:]], env=env).returncode


if __name__ == '__main__':
    sys.exit(main())
