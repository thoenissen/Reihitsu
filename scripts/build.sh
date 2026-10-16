#!/usr/bin/env bash
# Builds the Reihitsu solution, in Release configuration unless told otherwise.
#
#   scripts/build.sh [--configuration <Release|Debug>] [--no-install] [<extra dotnet build arguments>...]
#
# The solution is addressed by absolute path, so the caller's working directory
# is never changed and relative arguments keep their meaning.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

configuration="Release"
install_arguments=()
build_arguments=()

while [[ "$#" -gt 0 ]]; do
    case "$1" in
        --configuration)
            if [[ "$#" -lt 2 || ( "$2" != "Release" && "$2" != "Debug" ) ]]; then
                echo "build.sh: --configuration expects Release or Debug." >&2
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
            build_arguments+=("$1")
            shift
            ;;
    esac
done

reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet

dotnet build "$(reihitsu_repo_root)/Reihitsu.sln" -c "$configuration" --verbosity minimal "${build_arguments[@]+"${build_arguments[@]}"}"
