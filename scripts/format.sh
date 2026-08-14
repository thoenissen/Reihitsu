#!/usr/bin/env bash
# Formats the given paths with the repository's own CLI formatter.
#
#   scripts/format.sh <path> [<path>...] [--check|--dry-run]
#
# Paths are resolved against the caller's working directory, not the repository
# root, so a relative path means what it looks like it means.
#
# Run this over every changed C# path before running tests, as CLAUDE.md and
# AGENTS.md require.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

if [[ "$#" -eq 0 ]]; then
    echo "format.sh: expected at least one path." >&2
    exit 2
fi

install_arguments=()
formatter_arguments=()

for argument in "$@"; do
    case "$argument" in
        --no-install)
            install_arguments+=("$argument")
            ;;
        -*)
            formatter_arguments+=("$argument")
            ;;
        *)
            # Absolute paths survive the repository-root invocation below
            if [[ -e "$argument" ]]; then
                formatter_arguments+=("$(cd "$(dirname "$argument")" && pwd)/$(basename "$argument")")
            else
                formatter_arguments+=("$argument")
            fi
            ;;
    esac
done

if [[ "${#formatter_arguments[@]}" -eq 0 ]]; then
    echo "format.sh: expected at least one path." >&2
    exit 2
fi

reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet

dotnet run --project "$(reihitsu_repo_root)/Reihitsu.Cli" -- "${formatter_arguments[@]}"
