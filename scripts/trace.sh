#!/usr/bin/env bash
# Traces which top-level formatter phases change one C# file across repeated passes.
#
#   scripts/trace.sh <file> [--passes N] [--no-install]
#
# The input file is never modified. Paths are resolved against the caller's
# working directory, not the repository root.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

install_arguments=()
trace_arguments=()

for argument in "$@"; do
    case "$argument" in
        --no-install) install_arguments+=("$argument") ;;
        *) trace_arguments+=("$argument") ;;
    esac
done

reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet

trace_app="$(reihitsu_repo_root)/scripts/trace/trace.cs"

runner_exit_code=0

dotnet run "$trace_app" --verbosity quiet --property NoWarn=RH0002 -- "${trace_arguments[@]+"${trace_arguments[@]}"}" || runner_exit_code=$?

case "$runner_exit_code" in
    0) exit 0 ;;
    11) exit 1 ;;
    2) exit 2 ;;
    *) exit 2 ;;
esac
