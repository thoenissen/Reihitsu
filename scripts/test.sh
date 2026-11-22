#!/usr/bin/env bash
# Runs the Reihitsu test projects, in Release configuration unless told otherwise.
#
#   scripts/test.sh [options] [<extra dotnet test arguments>...]
#
#   --project <analyzer|formatter|core|cli|architecture|tooling|playground|selfhosting|all>   default: all
#   --filter <expression>                                              focused run
#   --configuration <Release|Debug>                                    default: Release
#   --no-build                                                         reuse the previous build of that configuration
#   --no-install                                                       fail instead of installing the SDK
#
# "all" is the default run; the all) branch below defines its projects, and
# scripts/README.md, "The default test run", states what it covers and why.
#
# Every test host runs under a hang timeout: when no test starts or finishes
# for that long, the host is ended and the run fails, whatever the hung code
# checks; scripts/README.md, "Hang timeout", states the limit and why.
#
# Test projects are addressed by absolute path, so the caller's working
# directory is never changed and relative arguments keep their meaning.
#
# Examples:
#   scripts/test.sh
#   scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3204"
#   scripts/test.sh --project selfhosting
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

project="all"
filter=""
configuration="Release"
install_arguments=()
test_arguments=()

while [[ "$#" -gt 0 ]]; do
    case "$1" in
        --project)
            if [[ "$#" -lt 2 ]]; then
                echo "test.sh: --project expects a value (analyzer, formatter, core, cli, architecture, tooling, playground, selfhosting, or all)." >&2
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
        --configuration)
            if [[ "$#" -lt 2 || ( "$2" != "Release" && "$2" != "Debug" ) ]]; then
                echo "test.sh: --configuration expects Release or Debug." >&2
                exit 2
            fi

            configuration="$2"
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
    analyzer) projects=("tests/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj") ;;
    formatter) projects=("tests/Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj") ;;
    core) projects=("tests/Reihitsu.Core.Test/Reihitsu.Core.Test.csproj") ;;
    cli) projects=("tests/Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj") ;;
    architecture) projects=("tests/Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj") ;;
    tooling) projects=("tests/Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj") ;;
    playground) projects=("tests/Reihitsu.Playground.Test/Reihitsu.Playground.Test.csproj") ;;
    selfhosting) projects=("tests/Reihitsu.SelfHosting.Test/Reihitsu.SelfHosting.Test.csproj") ;;
    all)
        projects=("tests/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj"
                  "tests/Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj"
                  "tests/Reihitsu.Core.Test/Reihitsu.Core.Test.csproj"
                  "tests/Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj"
                  "tests/Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj"
                  "tests/Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj"
                  "tests/Reihitsu.Playground.Test/Reihitsu.Playground.Test.csproj")
        ;;
    *)
        echo "test.sh: unknown --project value '$project' (use analyzer, formatter, core, cli, architecture, tooling, playground, selfhosting, or all)." >&2
        exit 2
        ;;
esac

reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet

# On glibc, the test host keeps the native memory it frees in malloc's arenas,
# so the analyzer test host grows to about 9 GiB over a whole-project run and a
# small machine kills it. A fixed mmap threshold returns large blocks to the
# system when they are freed, and two arenas bound what the small ones keep.
# The setting reaches the test host and the processes it starts, through a run
# setting, not the build or dotnet test itself, and a caller's own
# GLIBC_TUNABLES wins; scripts/README.md, "Test-host memory", has the
# measurements.
if [[ "$(uname -s)" == "Linux" && -z "${GLIBC_TUNABLES+set}" ]]; then
    test_host_setting="RunConfiguration.EnvironmentVariables.GLIBC_TUNABLES=glibc.malloc.mmap_threshold=131072:glibc.malloc.arena_max=2"
    has_run_settings_separator="false"

    for test_argument in "${test_arguments[@]+"${test_arguments[@]}"}"; do
        if [[ "$test_argument" == "--" ]]; then
            has_run_settings_separator="true"
        fi
    done

    if [[ "$has_run_settings_separator" == "false" ]]; then
        test_arguments+=("--")
    fi

    test_arguments+=("$test_host_setting")
fi

repository_root="$(reihitsu_repo_root)"

# The limit is the blame collector's inactivity time, so it bounds a hang, not
# a run. No dump is written, so the run needs no dump tool and leaves no large
# file behind.
hang_arguments=(--blame-hang-timeout 10min --blame-hang-dump-type none)

for test_project in "${projects[@]}"; do
    echo "==> $test_project"

    if [[ -n "$filter" ]]; then
        dotnet test "$repository_root/$test_project" -c "$configuration" --verbosity minimal "${hang_arguments[@]}" --filter "$filter" "${test_arguments[@]+"${test_arguments[@]}"}"
    else
        dotnet test "$repository_root/$test_project" -c "$configuration" --verbosity minimal "${hang_arguments[@]}" "${test_arguments[@]+"${test_arguments[@]}"}"
    fi
done
