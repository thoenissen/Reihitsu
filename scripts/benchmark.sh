#!/usr/bin/env bash
# Benchmarks released reihitsu-format tool versions from NuGet on a fixed corpus.
#
#   scripts/benchmark.sh [--version <v>]... [--commit <rev>] [--path <repo-path>]...
#                        [--repetitions N] [--warmups N] [--top N] [--work <dir>]
#                        [--no-perturb] [--roll-forward] [--no-install]
#
# The defaults compare 1.2.0 with 1.3.0 on src/Reihitsu.Formatter and src/Reihitsu.Analyzer
# at the commit tagged v1.2.0, with every "private " turned into "private  " so most files
# need rewriting. Each version formats a fresh copy of the corpus one file at a time, and the
# report gives total and per-file times. --roll-forward runs every version on the newest
# installed runtime instead of the one it targets. The working tree is only read; a commit missing
# from the clone is fetched into it. The corpus, the installed tools, and the raw per-file CSV live
# in the work directory, a new private temporary directory unless --work names one.
#
# Exit codes: 0 the benchmark ran, 2 invalid arguments or a tool, network, or setup failure.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

install_arguments=()

for argument in "$@"; do
    if [[ "$argument" == "--no-install" ]]; then
        install_arguments+=("$argument")
    fi
done

reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet

export REIHITSU_DOTNET_ROOT
REIHITSU_REPO_ROOT="$(reihitsu_repo_root)"
export REIHITSU_REPO_ROOT

benchmark_app="$REIHITSU_REPO_ROOT/scripts/benchmark/benchmark.cs"

runner_exit_code=0

dotnet run "$benchmark_app" --verbosity quiet -- "$@" || runner_exit_code=$?

case "$runner_exit_code" in
    0) exit 0 ;;
    *) exit 2 ;;
esac
