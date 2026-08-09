#!/usr/bin/env bash
# Runs the code-fix surface of one diagnostic ID over a directory of fixtures.
#
#   scripts/apply-fix.sh <diagnostic-id> <fixture-directory> [--max-iterations N] [--no-install]
#
# Every C# file below the directory is analyzed and fixed under LF and under CRLF, and each
# arm reports a status token plus the unified diff the fix produced. The fixtures are only
# read. Paths are resolved against the caller's working directory, not the repository root,
# so the fixture directory can live anywhere — a read-only workflow keeps it outside the
# repository and removes it afterwards.
#
# Exit codes: 0 every fixture reached a supported end state (including "no diagnostic" and
# "no fix offered"), 1 a fixture never stopped reporting the diagnostic, 2 the command could
# not run. A tool failure therefore never reads as a clean sweep.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

install_arguments=()
runner_arguments=()

for argument in "$@"; do
    case "$argument" in
        --no-install) install_arguments+=("$argument") ;;
        -*) runner_arguments+=("$argument") ;;
        *)
            # Absolute paths survive the repository-root invocation below
            if [ -e "$argument" ]; then
                runner_arguments+=("$(cd "$(dirname "$argument")" && pwd)/$(basename "$argument")")
            else
                runner_arguments+=("$argument")
            fi
            ;;
    esac
done

if ! reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet; then
    exit 2
fi

runner_app="$(reihitsu_repo_root)/scripts/apply-fix/apply-fix.cs"

runner_exit_code=0

dotnet run "$runner_app" --verbosity quiet --property NoWarn=RH0002 -- "${runner_arguments[@]+"${runner_arguments[@]}"}" || runner_exit_code=$?

case "$runner_exit_code" in
    0) exit 0 ;;
    11) exit 1 ;;
    2) exit 2 ;;
    *) exit 2 ;;
esac
