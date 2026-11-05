#!/usr/bin/env bash
set -Eeuo pipefail

# Ensure safe XDG_RUNTIME_DIR if invalid or unset
if [ -z "${XDG_RUNTIME_DIR:-}" ] || [ ! -d "${XDG_RUNTIME_DIR:-}" ]; then
  export XDG_RUNTIME_DIR="${TMPDIR:-/tmp}"
fi

# Locate repository root and relevant directories
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "$SCRIPT_DIR/.." && pwd)"

API_DIR="${REPO_ROOT}/src/HF.API"
CLIENT_DIR="${REPO_ROOT}/src/HF.Client"
OPENAPI_DIR="${CLIENT_DIR}/OpenApi"
SPEC_FILE="${OPENAPI_DIR}/openapi.json"
GENERATED_DIR="${CLIENT_DIR}/Generated"

CLIENT_NAME="${KIOTA_CLIENT_NAME:-ApiClient}"
CLIENT_NAMESPACE="${KIOTA_CLIENT_NAMESPACE:-HF.Client}"
API_PORT="${API_PORT:-5268}"
OPENAPI_URL="http://127.0.0.1:${API_PORT}/openapi/v1.json"

mkdir -p "$OPENAPI_DIR"
mkdir -p "$GENERATED_DIR"

echo "==> Restoring dotnet local tools (including Kiota)..."
dotnet tool restore --tool-manifest "${REPO_ROOT}/dotnet-tools.json"

echo "==> Starting HF.API on port ${API_PORT} to extract OpenAPI specification..."
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__PostgresConnection="${ConnectionStrings__PostgresConnection:-Host=localhost;Database=horizonfuture_dev;Username=postgres;Password=postgres}"
export Jwt__Key="${Jwt__Key:-HorizonFuture-Development-Secret-Key-Must-Be-At-Least-32-Bytes-Long-123456}"

# Start API in background and capture PID
dotnet run --project "$API_DIR" --no-launch-profile --urls "http://127.0.0.1:${API_PORT}" > /dev/null 2>&1 &
API_PID=$!

cleanup() {
  if [ -n "${API_PID:-}" ] && kill -0 "$API_PID" 2>/dev/null; then
    echo "==> Stopping HF.API (PID $API_PID)..."
    kill -15 "$API_PID" 2>/dev/null || true
    wait "$API_PID" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

echo "==> Waiting for OpenAPI endpoint at ${OPENAPI_URL}..."
SUCCESS=0
TIMEOUT=35
START_TIME=$(date +%s)

while true; do
  CURRENT_TIME=$(date +%s)
  ELAPSED=$((CURRENT_TIME - START_TIME))
  if [ $ELAPSED -ge $TIMEOUT ]; then
    break
  fi

  if curl -s -f -o "$SPEC_FILE" "$OPENAPI_URL" 2>/dev/null; then
    if [ -s "$SPEC_FILE" ]; then
      SUCCESS=1
      break
    fi
  fi

  # Check if process died early
  if ! kill -0 "$API_PID" 2>/dev/null; then
    echo "Error: HF.API process terminated unexpectedly before OpenAPI could be retrieved." >&2
    exit 1
  fi

  sleep 1
done

cleanup
trap - EXIT INT TERM

if [ $SUCCESS -ne 1 ]; then
  echo "Error: Failed to retrieve OpenAPI specification from ${OPENAPI_URL} within ${TIMEOUT} seconds." >&2
  exit 1
fi

echo "==> OpenAPI specification successfully saved to ${SPEC_FILE} ($(wc -c < "$SPEC_FILE") bytes)."

echo "==> Running Kiota to generate C# client..."
dotnet kiota generate \
  --openapi "$SPEC_FILE" \
  --language CSharp \
  --class-name "$CLIENT_NAME" \
  --namespace-name "$CLIENT_NAMESPACE" \
  --output "$GENERATED_DIR" \
  --clean-output \
  --log-level Warning

echo "==> Kiota client successfully generated in ${GENERATED_DIR}."
