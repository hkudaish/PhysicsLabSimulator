"""Build the alternative-engine spike using its explicitly scoped SDK policy."""
import argparse
from pathlib import Path
import subprocess

from run_godot_spike import context, PROJECT, ROOT


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--windows', action='store_true')
    args = parser.parse_args()
    _, env = context(args.windows)
    dotnet = '/mnt/c/Program Files/dotnet/dotnet.exe' if args.windows else '/tmp/virtual-stem-toolchain/dotnet/dotnet'
    platform = 'windows' if args.windows else 'linuxbsd'
    output = ROOT / 'artifacts/godot-spike' / ('windows' if args.windows else 'linux')
    output.mkdir(parents=True, exist_ok=True)
    command = [dotnet, 'build', 'GodotCompatibility.csproj', '--configuration', 'Debug', '--nologo',
               '-p:GodotTargetPlatform=' + platform, '-p:RestoreLockedMode=true']
    with (output/'build.log').open('w') as stream:
        result = subprocess.run(command, cwd=PROJECT, env=env, stdout=stream, stderr=subprocess.STDOUT, timeout=120)
    print('Build exit:', result.returncode, 'Log:', output/'build.log')
    return result.returncode


if __name__ == '__main__':
    raise SystemExit(main())
