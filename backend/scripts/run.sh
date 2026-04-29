#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

API_PROJECT="src/ZealEducation.API"

cd "$BACKEND_DIR"

usage() {
    cat <<EOF
Usage: ./scripts/run.sh [command] [-- <extra dotnet args>]

Commands:
  dev              Run the API in Development (default)
  prod             Run the API in Production
  watch            Run with hot reload (dotnet watch)
  build            Build the solution (Debug)
  publish [dir]    Publish Release build (default dir: ./publish)
  restore          Restore NuGet packages
  clean            Clean build artifacts

Examples:
  ./scripts/run.sh
  ./scripts/run.sh dev
  ./scripts/run.sh watch
  ./scripts/run.sh dev -- --launch-profile https
  ./scripts/run.sh publish ./out
EOF
}

cmd="${1:-dev}"
shift || true

extra_args=()
if [[ "${1:-}" == "--" ]]; then
    shift
    extra_args=("$@")
fi

case "$cmd" in
    dev)
        ASPNETCORE_ENVIRONMENT=Development dotnet run build --project "$API_PROJECT" "${extra_args[@]}"
        ;;
    prod)
        ASPNETCORE_ENVIRONMENT=Production dotnet run --project "$API_PROJECT" -c Release "${extra_args[@]}"
        ;;
    watch)
        ASPNETCORE_ENVIRONMENT=Development dotnet watch --project "$API_PROJECT" run "${extra_args[@]}"
        ;;
    build)
        dotnet build "${extra_args[@]}"
        ;;
    publish)
        out="${1:-./publish}"
        dotnet publish "$API_PROJECT" -c Release -o "$out" "${extra_args[@]}"
        ;;
    restore)
        dotnet restore "${extra_args[@]}"
        ;;
    clean)
        dotnet clean "${extra_args[@]}"
        ;;
    -h|--help|help)
        usage
        ;;
    *)
        echo "Unknown command: $cmd" >&2
        usage
        exit 1
        ;;
esac
