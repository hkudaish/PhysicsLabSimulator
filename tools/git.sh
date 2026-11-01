#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
exec git --git-dir="$project_root/artifacts/source-control.git" --work-tree="$project_root" "$@"
