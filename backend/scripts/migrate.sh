#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

INFRASTRUCTURE_PROJECT="src/ZealEducation.Infrastructure"
STARTUP_PROJECT="src/ZealEducation.API"

cd "$BACKEND_DIR"

usage() {
    cat <<EOF
Usage: ./scripts/migrate.sh <command> [args]

Commands:
  update [target]     Apply migrations up to <target> (default: latest)
  add <Name>          Create a new migration
  remove              Remove the last migration (only if not applied)
  list                List all migrations and their status
  script [from] [to]  Generate idempotent SQL script (default: full schema)
  drop                Drop the database (asks for confirmation)
  reset               Drop database then re-apply all migrations

Examples:
  ./scripts/migrate.sh update
  ./scripts/migrate.sh add AddStudentTable
  ./scripts/migrate.sh update 20260421120952_AddUserAccount
  ./scripts/migrate.sh script 0 > migration.sql
EOF
}

ef() {
    dotnet ef "$@" \
        --project "$INFRASTRUCTURE_PROJECT" \
        --startup-project "$STARTUP_PROJECT"
}

cmd="${1:-}"
shift || true

case "$cmd" in
    update)
        target="${1:-}"
        if [[ -n "$target" ]]; then
            ef database update "$target"
        else
            ef database update
        fi
        ;;
    add)
        name="${1:-}"
        if [[ -z "$name" ]]; then
            echo "Error: migration name required" >&2
            usage
            exit 1
        fi
        ef migrations add "$name"
        ;;
    remove)
        ef migrations remove
        ;;
    list)
        ef migrations list
        ;;
    script)
        from="${1:-0}"
        to="${2:-}"
        if [[ -n "$to" ]]; then
            ef migrations script "$from" "$to" --idempotent
        else
            ef migrations script "$from" --idempotent
        fi
        ;;
    drop)
        ef database drop --force
        ;;
    reset)
        ef database drop --force
        ef database update
        ;;
    -h|--help|help|"")
        usage
        ;;
    *)
        echo "Unknown command: $cmd" >&2
        usage
        exit 1
        ;;
esac
