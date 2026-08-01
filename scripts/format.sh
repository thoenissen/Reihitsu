#!/usr/bin/env bash
# Formats the given paths with the repository's own CLI formatter.
#
#   scripts/format.sh <path> [<path>...] [--check|--dry-run]
#
# Run this over every changed C# path before running tests, as CLAUDE.md and
# AGENTS.md require.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

if [ "$#" -eq 0 ]; then
    echo "format.sh: expected at least one path." >&2
    exit 2
fi

install_arguments=()
formatter_arguments=()

for argument in "$@"; do
    case "$argument" in
        --no-install) install_arguments+=("$argument") ;;
        *) formatter_arguments+=("$argument") ;;
    esac
done

reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet

cd "$(reihitsu_repo_root)"

dotnet run --project Reihitsu.Cli -- "${formatter_arguments[@]}"
