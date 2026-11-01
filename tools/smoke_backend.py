"""Verify only the scaffold's development health route and production default deny."""
import json
from pathlib import Path
import socket
import subprocess
import time
import urllib.error
import urllib.request

from dev import environment

ROOT = Path(__file__).resolve().parents[1]


def status(url):
    try:
        with urllib.request.urlopen(url, timeout=1) as response:
            return response.status, response.read()
    except urllib.error.HTTPError as error:
        return error.code, error.read()


def smoke(mode):
    with socket.socket() as sock:
        sock.bind(('127.0.0.1', 0))
        port = sock.getsockname()[1]
    env = environment()
    env['ASPNETCORE_ENVIRONMENT'] = mode
    env['ASPNETCORE_URLS'] = f'http://127.0.0.1:{port}'
    executable = str(Path(env['DOTNET_ROOT']) / 'dotnet')
    log_path = ROOT / 'artifacts' / f'backend-{mode.lower()}-smoke.log'
    with log_path.open('w') as log:
        process = subprocess.Popen([executable, str(ROOT / 'src/Backend/bin/Release/net10.0/Backend.dll')],
                                   cwd=ROOT, env=env, stdout=log, stderr=log)
        try:
            deadline = time.monotonic() + 15
            while True:
                if process.poll() is not None:
                    raise RuntimeError(f'Backend exited; inspect {log_path}')
                try:
                    code, body = status(f'http://127.0.0.1:{port}/health')
                    break
                except (OSError, TimeoutError):
                    if time.monotonic() >= deadline:
                        raise RuntimeError(f'Backend startup timeout; inspect {log_path}')
                    time.sleep(0.1)
            expected = 200 if mode == 'Development' else 404
            if code != expected:
                raise AssertionError(f'{mode}: expected {expected}, got {code}')
            if mode == 'Development' and json.loads(body) != {'status': 'scaffold', 'phase': 0}:
                raise AssertionError('Unexpected scaffold status')
            if status(f'http://127.0.0.1:{port}/api/v1/assignments')[0] != 404:
                raise AssertionError('Phase 2 application route unexpectedly exposed')
            print(f'{mode}: health {code}; unimplemented application route 404')
        finally:
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait(timeout=5)


if __name__ == '__main__':
    for mode in ('Development', 'Production'):
        smoke(mode)
