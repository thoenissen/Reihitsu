#!/usr/bin/env bash
# Runs the Reihitsu test projects in Release configuration.
#
#   scripts/test.sh [options] [<extra dotnet test arguments>...]
#
#   --project <analyzer|formatter|core|cli|architecture|tooling|playground|all>   default: all
#   --filter <expression>                                              focused run
#   --no-build                                                         reuse the previous Release build
#   --no-install                                                       fail instead of installing the SDK
#
# Test projects are addressed by absolute path, so the caller's working
# directory is never changed and relative arguments keep their meaning.
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

while [[ "$#" -gt 0 ]]; do
    case "$1" in
        --project)
            if [[ "$#" -lt 2 ]]; then
                echo "test.sh: --project expects a value (analyzer, formatter, core, cli, architecture, tooling, playground, or all)." >&2
                exit 2
            fi

            project="$2"
            shift 2
            ;;
        --filter)
            if [[ "$#" -lt 2 ]]; then
                echo "test.sh: --filter expects an expression." >&2
                exit 2
            fi

            filter="$2"
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
    analyzer) projects=("src/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj") ;;
    formatter) projects=("src/Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj") ;;
    core) projects=("src/Reihitsu.Core.Test/Reihitsu.Core.Test.csproj") ;;
    cli) projects=("src/Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj") ;;
    architecture) projects=("src/Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj") ;;
    tooling) projects=("src/Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj") ;;
    playground) projects=("src/Reihitsu.Playground.Test/Reihitsu.Playground.Test.csproj") ;;
    all)
        projects=("src/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj"
                  "src/Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj"
                  "src/Reihitsu.Core.Test/Reihitsu.Core.Test.csproj"
                  "src/Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj"
                  "src/Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj"
                  "src/Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj"
                  "src/Reihitsu.Playground.Test/Reihitsu.Playground.Test.csproj")
        ;;
    *)
        echo "test.sh: unknown --project value '$project' (use analyzer, formatter, core, cli, architecture, tooling, playground, or all)." >&2
        exit 2
        ;;
esac

reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet

repository_root="$(reihitsu_repo_root)"

for test_project in "${projects[@]}"; do
    echo "==> $test_project"

    if [[ -n "$filter" ]]; then
        dotnet test "$repository_root/$test_project" -c Release --verbosity minimal --filter "$filter" "${test_arguments[@]+"${test_arguments[@]}"}"
    else
        dotnet test "$repository_root/$test_project" -c Release --verbosity minimal "${test_arguments[@]+"${test_arguments[@]}"}"
    fi
done
