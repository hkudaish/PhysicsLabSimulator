"""Run a disposable local-only PostgreSQL architecture spike; always stop the server."""
import argparse
import os
from pathlib import Path
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--distribution', type=Path, default=Path('/tmp/stem-postgres-root'))
    args = parser.parse_args()
    bins = args.distribution / 'usr/lib/postgresql/16/bin'
    share = args.distribution / 'usr/share/postgresql/16'
    env = os.environ.copy()
    env['LD_LIBRARY_PATH'] = str(args.distribution / 'usr/lib/x86_64-linux-gnu')
    env['LC_ALL'] = 'C'
    (ROOT / 'artifacts').mkdir(exist_ok=True)
    log_path = ROOT / 'artifacts/postgres-spike.log'
    with tempfile.TemporaryDirectory(prefix='stem-pg-spike-') as temporary, log_path.open('w') as log:
        temporary = Path(temporary)
        data, sockets = temporary / 'data', temporary / 'sockets'
        sockets.mkdir(mode=0o700)

        def run(*command, capture=False):
            return subprocess.run([str(part) for part in command], env=env, check=True, text=True,
                                  stdout=subprocess.PIPE if capture else log, stderr=log, timeout=60)

        run(bins/'initdb', '-D', data, '-L', share, '--username=stem_owner', '--auth-local=trust',
            '--auth-host=reject', '--no-locale', '--encoding=UTF8')
        started = False
        try:
            run(bins/'pg_ctl', '-D', data, '-l', temporary/'server.log', '-o', f"-F -k {sockets} -h ''", '-w', 'start')
            started = True
            output = run(bins/'psql', '-X', '-h', sockets, '-U', 'stem_owner', '-d', 'postgres', '-v', 'ON_ERROR_STOP=1',
                         '-f', ROOT/'specs/data/postgres-spike.sql', '-f', ROOT/'tests/postgres/tenant_isolation.sql', capture=True)
            log.write(output.stdout)
            passes = [line for line in output.stdout.splitlines() if line.startswith('PASS ')]
            if len(passes) != 8:
                raise RuntimeError(f'Expected 8 assertion groups, got {len(passes)}; inspect {log_path}')
            print('\n'.join(passes))
        finally:
            if started:
                run(bins/'pg_ctl', '-D', data, '-m', 'fast', '-w', 'stop')
            if (temporary/'server.log').exists():
                log.write((temporary/'server.log').read_text())
    print(f'Disposable PostgreSQL stopped and removed; log: {log_path}')


if __name__ == '__main__':
    main()
