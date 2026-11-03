#!/usr/bin/env bash
set -Eeuo pipefail

usage() {
  cat <<'EOF'
Usage: mudmcp-{install,start,stop,uninstall}.sh [--install-root PATH] [--mudblazor-version VERSION] [--force]
Defaults: MUDMCP_INSTALL_ROOT or ${XDG_DATA_HOME:-$HOME/.local/share}/HorizonFuture/MudMCP
          MUDMCP_MUDBLAZOR_VERSION or 9.0.0
--force skips the uninstall confirmation.
EOF
}
fail() { printf 'Error: %s\n' "$*" >&2; exit 1; }

action="${1:-}"
case "$action" in install|start|stop|uninstall) shift;; -h|--help|"") usage; exit 0;; *) fail "Unknown action: $action";; esac

install_root="${MUDMCP_INSTALL_ROOT:-${XDG_DATA_HOME:-$HOME/.local/share}/HorizonFuture/MudMCP}"
mudblazor_version="${MUDMCP_MUDBLAZOR_VERSION:-9.0.0}"
force=false
while (($#)); do
  case "$1" in
    --install-root) (($# >= 2)) || fail '--install-root needs a path'; install_root="$2"; shift 2;;
    --mudblazor-version) (($# >= 2)) || fail '--mudblazor-version needs a version'; mudblazor_version="$2"; shift 2;;
    --force) force=true; shift;;
    -h|--help) usage; exit 0;;
    *) fail "Unknown option: $1";;
  esac
done
install_root="$(mkdir -p "$(dirname "$install_root")" && cd "$(dirname "$install_root")" && printf '%s/%s' "$PWD" "$(basename "$install_root")")"

upstream='https://github.com/mcbodge/MudMCP.git'
tag='v1.1.0'
compose_file="$install_root/docker-compose.horizon.yml"

check_docker() {
  command -v docker >/dev/null 2>&1 || fail 'Docker CLI is required.'
  docker info >/dev/null
  docker compose version >/dev/null
}
check_checkout() {
  [[ -d "$install_root/.git" ]] || fail "No MudMCP install at $install_root; run scripts/mudmcp-install.sh."
  [[ "$(git -C "$install_root" remote get-url origin)" == "$upstream" ]] || fail 'Install root is not the expected MudMCP checkout.'
  [[ "$(git -C "$install_root" describe --tags --exact-match 2>/dev/null || true)" == "$tag" ]] || fail "Checkout must be pinned to $tag."
  [[ -f "$install_root/Dockerfile" ]] || fail 'Pinned checkout has no Dockerfile.'
}
write_compose() {
  cat > "$compose_file" <<'YAML'
services:
  mudblazor-mcp:
    build:
      context: .
      dockerfile: Dockerfile
    image: horizonfuture-mudmcp:v1.1.0
    container_name: horizonfuture-mudmcp
    ports:
      - "127.0.0.1:8000:8080"
    volumes:
      - mudmcp-data:/app/data
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      MUDBLAZOR_VERSION: "${MUDBLAZOR_VERSION:-9.0.0}"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/live"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 600s
    restart: unless-stopped
volumes:
  mudmcp-data:
    name: horizonfuture-mudmcp-data
YAML
}
ensure_install() {
  if [[ ! -e "$install_root" ]]; then
    command -v git >/dev/null 2>&1 || fail 'Git is required for first install.'
    git clone --branch "$tag" --depth 1 "$upstream" "$install_root"
  fi
  check_checkout
  write_compose
}
compose() {
  env MUDBLAZOR_VERSION="$mudblazor_version" docker compose --project-name horizonfuture-mudmcp --project-directory "$install_root" -f "$compose_file" "$@"
}

case "$action" in
 install)
   check_docker; ensure_install; compose build
   printf 'MudMCP %s image built for MudBlazor %s.\n' "$tag" "$mudblazor_version";;
 start)
   check_docker; ensure_install; compose up --detach --build
   printf 'MCP endpoint: http://localhost:8000/mcp\nFirst indexing may take several minutes.\n';;
 stop)
   check_docker; check_checkout; write_compose; compose down
   printf 'Stopped; cache volume preserved.\n';;
 uninstall)
   check_docker; check_checkout; write_compose
   if [[ "$force" != true ]]; then
     printf 'This deletes the container, image, checkout, and cache. Type REMOVE to continue: '
     IFS= read -r answer
     [[ "$answer" == REMOVE ]] || { printf 'Cancelled.\n'; exit 0; }
   fi
   compose down --volumes --remove-orphans --rmi local
   rm -rf -- "$install_root"
   printf 'MudMCP and cached data removed.\n';;
esac
