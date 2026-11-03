#!/usr/bin/env bash
set -Eeuo pipefail
dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
exec "$dir/mudmcp.sh" install "$@"
