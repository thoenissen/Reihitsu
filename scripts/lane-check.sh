#!/usr/bin/env bash
# Confirms the squad lane a run claimed against the paths its diff changes.
#
#   scripts/lane-check.sh <docs|tests|bug-fix|behavior-change|tooling> [--base <rev>] [--head <rev|worktree>] [--no-install]
#
# Defaults to the merge base with origin/main and HEAD; `--head worktree` includes uncommitted
# changes. The diff's paths decide, not the claim: each changed path is classified, and the
# narrowest lane that admits all of them is compared with the claimed one. C# files count as
# text only when scripts/verify-text-only.sh proves the whole diff text-only, so this script
# runs that proof first and prints its line.
#
# Exit codes: 0 the claimed lane covers the diff, 1 it does not (the verdict line names the lane
# the diff needs), 2 tool or setup error, 3 no lane admits the diff — the lead escalates that.
#
# The single `LANE CHECK: …` line is the quotable evidence the run report records.
set -uo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

lane=""
base_revision=""
head_revision="HEAD"
install_arguments=()

while [[ "$#" -gt 0 ]]; do
    case "$1" in
        --base|--head)
            if [[ "$#" -lt 2 ]]; then
                echo "lane-check.sh: $1 expects a revision." >&2
                exit 2
            fi

            if [[ "$1" == "--base" ]]; then
                base_revision="$2"
            else
                head_revision="$2"
            fi

            shift 2
            ;;
        --no-install)
            install_arguments+=("$1")
            shift
            ;;
        -*)
            echo "lane-check.sh: unknown option '$1'." >&2
            exit 2
            ;;
        *)
            if [[ -n "$lane" ]]; then
                echo "lane-check.sh: expected exactly one lane." >&2
                exit 2
            fi

            lane="$1"
            shift
            ;;
    esac
done

if [[ -z "$lane" ]]; then
    echo "lane-check.sh: expected a lane (docs, tests, bug-fix, behavior-change, or tooling)." >&2
    exit 2
fi

if ! reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet; then
    exit 2
fi

repository_root="$(reihitsu_repo_root)"

if [[ -z "$base_revision" ]]; then
    if ! base_revision="$(git -C "$repository_root" merge-base origin/main HEAD)"; then
        echo "lane-check.sh: could not resolve a base revision. Pass --base explicitly." >&2
        exit 2
    fi
fi

diff_arguments=(diff --name-status --no-renames "$base_revision")

if [[ "$head_revision" != "worktree" ]]; then
    diff_arguments+=("$head_revision")
fi

if ! changes="$(git -C "$repository_root" "${diff_arguments[@]}")"; then
    echo "lane-check.sh: git diff failed." >&2
    exit 2
fi

# Only exit code 0 proves anything: a failed proof and a tool error both leave every C# file classified by its path
proof_exit_code=0
proof_output="$("$script_dir/verify-text-only.sh" --base "$base_revision" --head "$head_revision" "${install_arguments[@]+"${install_arguments[@]}"}" 2>&1)" || proof_exit_code=$?

printf '%s\n' "$proof_output" | grep '^TEXT-ONLY PROOF:' || echo "TEXT-ONLY PROOF: not available (exit code $proof_exit_code)"

text_only="fail"

if [[ "$proof_exit_code" -eq 0 ]]; then
    text_only="pass"
fi

checker_exit_code=0

printf '%s\n' "$changes" | dotnet run "$repository_root/scripts/lane-check/lane-check.cs" --verbosity quiet --property Configuration=Debug --property NoWarn=RH0002 -- --lane "$lane" --text-only "$text_only" || checker_exit_code=$?

case "$checker_exit_code" in
    0) exit 0 ;;
    11) exit 1 ;;
    13) exit 3 ;;
    *) exit 2 ;;
esac
