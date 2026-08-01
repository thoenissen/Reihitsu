#!/usr/bin/env bash
# Runs the Reihitsu test projects in Release configuration.
#
#   scripts/test.sh [options] [<extra dotnet test arguments>...]
#
#   --project <analyzer|formatter|core|cli|architecture|all>   default: all
#   --filter <expression>                                      focused run
#   --no-build                                                 reuse the previous Release build
#   --no-install                                               fail instead of installing the SDK
#
# Examples:
#   scripts/test.sh
#   scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3204"
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

project="all"
filter=""
install_arguments=()
test_arguments=()

while [ "$#" -gt 0 ]; do
    case "$1" in
        --project)
            project="${2:-}"
            shift 2
            ;;
        --filter)
            filter="${2:-}"
            shift 2
            ;;
        --no-install)
            install_arguments+=("$1")
            shift
            ;;
        *)
            test_arguments+=("$1")
            shift
            ;;
    esac
done

declare -a projects

case "$project" in
    analyzer) projects=("Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj") ;;
    formatter) projects=("Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj") ;;
    core) projects=("Reihitsu.Core.Test/Reihitsu.Core.Test.csproj") ;;
    cli) projects=("Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj") ;;
    architecture) projects=("Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj") ;;
    all)
        projects=("Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj"
                  "Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj"
                  "Reihitsu.Core.Test/Reihitsu.Core.Test.csproj"
                  "Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj")
        ;;
    *)
        echo "test.sh: unknown --project value '$project' (use analyzer, formatter, core, cli, architecture, or all)." >&2
        exit 2
        ;;
esac

reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet

cd "$(reihitsu_repo_root)"

for test_project in "${projects[@]}"; do
    echo "==> $test_project"

    if [ -n "$filter" ]; then
        dotnet test "$test_project" -c Release --verbosity minimal --filter "$filter" "${test_arguments[@]+"${test_arguments[@]}"}"
    else
        dotnet test "$test_project" -c Release --verbosity minimal "${test_arguments[@]+"${test_arguments[@]}"}"
    fi
done
