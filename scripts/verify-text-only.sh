#!/usr/bin/env bash
# Proves that a change carries no compiled behavior.
#
#   scripts/verify-text-only.sh [--base <rev>] [--head <rev>]
#
# Defaults to the merge base with origin/main and HEAD. Use `--head worktree`
# to include uncommitted changes.
#
# Exit codes: 0 proven text-only, 1 not proven, 2 tool or setup error.
#
# The single `TEXT-ONLY PROOF: …` line is the quotable evidence the gh-* skills
# record when they skip a preflight attempt or apply a non-blocking cleanup.
set -uo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

install_arguments=()
proof_arguments=()

for argument in "$@"; do
    case "$argument" in
        --no-install) install_arguments+=("$argument") ;;
        *) proof_arguments+=("$argument") ;;
    esac
done

if ! reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet; then
    exit 2
fi

cd "$(reihitsu_repo_root)"

dotnet run scripts/proof/verify-text-only.cs -- "${proof_arguments[@]+"${proof_arguments[@]}"}"
